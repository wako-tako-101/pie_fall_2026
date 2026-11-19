using UnityEngine;
using System.Collections.Generic;

//script copied from Austin Stanbury's version
//script based on 1$ classifier

namespace SigilClassifier
{
    public class SigilRecognizer : MonoBehaviour
    {
        [System.Serializable]
            public class BankEntry
            {
                [Tooltip("What the recogniser reports when a stroke matches this picture. SpellCaster looks spells up by this name.")]
                public string name;

                [Tooltip(SigilImageTest.ImageNote)]
                public Texture2D image;
            }

            [Header("Sigil bank (what every stroke is compared against)")]
            [Tooltip("Circle, Triangle, Square, Line, V, Zigzag and Star, generated from geometry in SigilTemplates.cs. Turn off to compare against the pictures below only.")]
            public bool includeBuiltInShapes = false;

            [Tooltip("Your own sigils. Each picture is traced into a stroke and stored under its name. One black line on white, drawn in one stroke.")]
            public List<BankEntry> bank = new List<BankEntry>();

            [Header("$1 settings (see Classifier.cs)")]
            [Tooltip("Every stroke is resampled to this many evenly spaced points before comparison. 64 is the paper's choice.")]
            public int numPoints = 64;

            [Tooltip("How far (either way, in degrees) the recogniser may rotate a stroke hunting for a better fit. Bigger = more rotation-tolerant, but a V and a ^ start to look alike.")]
            public float angleRangeDegrees = 45f;

            [Tooltip("Stop the rotation search once the answer is pinned down to this many degrees.")]
            public float anglePrecisionDegrees = 2f;

            [Header("Decision")]
            [Tooltip("Scores below this are reported as 'no sigil'. Every stroke has SOME nearest template, so this is what stops a scribble from casting a fireball.")]
            [Range(0f, 1f)] public float confidenceThreshold = 0.8f;

            // The pictures in the bank are traced with these. Same defaults as SigilImageTest.
            // SIMPLE vs ROBUST: the robust version exposes them per picture. Clean drawings never need it.
            const int   BankTraceGridSize = 96;
            const float BankInkThreshold  = 0.5f;

            Classifier recognizer;

            /// The underlying recogniser, built on first use so it also works in Edit mode.
            public Classifier Recognizer
            {
                get
                {
                    if (recognizer == null) Build();
                    return recognizer;
                }
            }

            /// Every name in the bank, built-ins first, in the order they were added. For readouts.
            public List<string> KnownNames
            {
                get
                {
                    var names = new List<string>();
                    foreach (var t in Recognizer.Templates)
                        if (!names.Contains(t.Name)) names.Add(t.Name);
                    return names;
                }
            }

            void Awake() => Build();

            // Settings or bank changed in the Inspector: throw the old one away, rebuild on next use.
            void OnValidate() => recognizer = null;

            /// Make a fresh recogniser with the current settings and the whole bank.
            public void Build()
            {
                recognizer = new Classifier
                {
                    NumPoints             = Mathf.Max(8, numPoints),
                    AngleRangeDegrees     = Mathf.Max(0f, angleRangeDegrees),
                    AnglePrecisionDegrees = Mathf.Max(0.1f, anglePrecisionDegrees),
                };

                // if (includeBuiltInShapes) SigilTemplates.AddDefaults(recognizer);

                foreach (var entry in bank) AddPicture(entry);
            }

            /// Trace one bank picture into a stroke and store it under its name.
            void AddPicture(BankEntry entry)
            {
                if (entry.image == null) return;                            // an empty row, not an error
                if (string.IsNullOrWhiteSpace(entry.name))
                {
                    Debug.LogWarning($"[Sigil] {name}: bank picture '{entry.image.name}' has no name, so it was skipped.", this);
                    return;
                }

                var stroke = SigilImageTracer.TraceAny(entry.image, BankTraceGridSize, BankInkThreshold, out var info);
                if (stroke.Count < 2)
                {
                    Debug.LogWarning($"[Sigil] {name}: found no ink in bank picture '{entry.image.name}' ({entry.name}). {SigilImageTest.ImageNote}", this);
                    return;
                }
                if (info.Coverage < 0.8f)
                    Debug.LogWarning($"[Sigil] {name}: bank picture '{entry.image.name}' ({entry.name}) looks like more than one stroke; " +
                                    $"only {info.Coverage:P0} of the ink was reached. $1 is a unistroke recogniser.", this);

                SigilTemplates.AddFromStroke(recognizer, entry.name, stroke);
            }

            /// The normal path: a stroke whose start point and direction are meaningful (a real
            /// drawing, where we know which end the pen started at).
            public Classifier.Result Recognize(IList<Vector2> stroke) => Recognizer.Recognize(stroke);

            /// For strokes where we do NOT know where the drawing started or which way it went, e.g.
            /// one traced out of an image. $1 cares about both (see step 2 and PathDistance in
            /// Classifier.cs), so we simply try: `startOffsets` different starting points
            /// around the stroke, forwards and backwards, and keep the best score for each template.
            /// That is 2 * startOffsets recognitions, which is still only a few milliseconds.
            public Classifier.Result RecognizeAnyStartOrDirection(IList<Vector2> stroke, int startOffsets = 16)
            {
                if (stroke == null || stroke.Count < 2) return Recognizer.Recognize(stroke);

                var pts = new List<Vector2>(stroke);
                int n = pts.Count;
                var bestByName = new Dictionary<string, Classifier.Match>();

                for (int direction = 0; direction < 2; direction++)
                {
                    if (direction == 1) pts.Reverse();

                    for (int k = 0; k < startOffsets; k++)
                    {
                        int offset = (int)((long)k * n / startOffsets);

                        // Same points, same order, but starting `offset` points in and wrapping round.
                        var rotated = new List<Vector2>(n);
                        for (int i = 0; i < n; i++) rotated.Add(pts[(i + offset) % n]);

                        var r = Recognizer.Recognize(rotated);
                        foreach (var m in r.Matches)
                            if (!bestByName.TryGetValue(m.Name, out var existing) || m.Score > existing.Score)
                                bestByName[m.Name] = m;
                    }
                }

                var result = new Classifier.Result();
                result.Matches.AddRange(bestByName.Values);
                result.Matches.Sort((a, b) => b.Score.CompareTo(a.Score));
                if (result.Matches.Count > 0)
                {
                    result.Name  = result.Matches[0].Name;
                    result.Score = result.Matches[0].Score;
                }
                return result;
            }
    }
}
