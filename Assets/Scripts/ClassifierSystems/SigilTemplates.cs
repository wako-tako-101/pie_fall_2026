using UnityEngine;
using System.Collections.Generic;

//This script is based off Prof. Austin Stanbury's SigilTemplates script, based of the 1$ recognizer
//annotations brought by Prof. Austin Stanbury
public class SigilTemplates
{
    /// Add the stroke as drawn, then again backwards, under the same name.
    public static void AddBothDirections(Classifier r, string name, params Vector2[] pts)
    {
        r.AddTemplate(name, pts);
        var reversed = new List<Vector2>(pts);
        reversed.Reverse();
        r.AddTemplate(name, reversed);
    }

    // starts at the top most point of ink and traces the stroke (regardless or drawn order)
    // if the shape is open it starts lower down
    // closed shapes are when ends are within 15% of the strokes' length
     public static void AddFromStroke(Classifier r, string name, IList<Vector2> stroke, int startOffsets = 16)
    {
        int n = stroke.Count;
        float endGap = Vector2.Distance(stroke[0], stroke[n - 1]);
        bool closed  = endGap < 0.15f * Classifier.PathLength(stroke);

        var asArray = new List<Vector2>(stroke).ToArray();
        if (!closed)
        {
            AddBothDirections(r, name, asArray);
            return;
        }

        for (int k = 0; k < startOffsets; k++)
        {
            int offset = k * n / startOffsets;
            var rotated = new Vector2[n];
            for (int i = 0; i < n; i++) rotated[i] = asArray[(i + offset) % n];
            AddBothDirections(r, name, rotated);
        }
    }
}
