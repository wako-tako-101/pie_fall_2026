using UnityEngine;
using System.Collections.Generic;

//This script is based off the 1$ recognizer
//annotations brought by Prof. Austin Stanbury
public class Classifier
{
    //data types
    public class Template
    {
        public string Name;
        public List<Vector2> Points;
    }
    public struct Match
    {
        public string Name;
        public float Score; // 1 = identical, 0 = as different as the maths allows
        public float Distance; // the raw average point-to-point distance, in "square units"
    }
    public class Result
    {
        public string Name = "(nothing)";
        public float Score = 0f;
        public List<Match> Matches = new List<Match>();
        public bool IsConfident(float threshold) => Score >= threshold;
    }

    //settings
    public int NumPoints = 64;
    public float SquareSize = 250f;
    public float AngleRangeDegrees = 45f;
    public float AnglePrecisionDegrees = 2f;
    static readonly float Phi = 0.5f * (-1f + Mathf.Sqrt(5f));
    float HalfDiagonal => 0.5f * Mathf.Sqrt(2f * SquareSize * SquareSize);
    readonly List<Template> templates = new List<Template>();
    public IReadOnlyList<Template> Templates => templates;

    //API
    public void AddTemplate(string name, IList<Vector2> rawPoints)
    {
        templates.Add(new Template { Name = name, Points = Normalize(rawPoints) });
    }
    public void ClearTemplates() => templates.Clear();
    public Result Recognize(IList<Vector2> rawPoints)
    {
        var result = new Result();
        if (rawPoints == null || rawPoints.Count < 2 || PathLength(rawPoints) < 1e-4f)
            return result; // draw nothing, say nothing

        // put candidates through same process and compare with like
        var candidate = Normalize(rawPoints);

        // score every template & keep the best per name
        var bestByName = new Dictionary<string, Match>();
        foreach (var t in templates)
        {
            // determine the distance via best angle of rotation
            float d = DistanceAtBestAngle(candidate, t.Points,
                                            -AngleRangeDegrees * Mathf.Deg2Rad,
                                            AngleRangeDegrees * Mathf.Deg2Rad,
                                            AnglePrecisionDegrees * Mathf.Deg2Rad);

            // turn into a score from 0 to 1
            float score = 1f - d / HalfDiagonal;

            if (!bestByName.TryGetValue(t.Name, out var existing) || score > existing.Score)
                bestByName[t.Name] = new Match { Name = t.Name, Score = score, Distance = d };
        }
        
        result.Matches.AddRange(bestByName.Values);
        result.Matches.Sort((a, b) => b.Score.CompareTo(a.Score));   // best first
        if (result.Matches.Count > 0)
        {
            result.Name  = result.Matches[0].Name;
            result.Score = result.Matches[0].Score;
        }
        return result;
    }

    //normalization

    //remove speed by placing posts at equal distances along the path of the shape
    public List<Vector2> Normalize(IList<Vector2> rawPoints)
    {
        var p = Resample(rawPoints, NumPoints);        // 1. speed no longer matters
        float angle = IndicativeAngle(p);              // 2. (find how it is tilted...)
        p = RotateBy(p, -angle);                       //    ...and un-tilt it)
        p = ScaleTo(p, SquareSize);                    // 3. size no longer matters
        p = TranslateTo(p, Vector2.zero);              // 4. position no longer matters
        return p;
    }
    public static List<Vector2> Resample(IList<Vector2> points, int n)
    {
        float interval = PathLength(points) / (n - 1);   // desired gap between fence posts
        float accumulated = 0f;                          // distance walked since last post

        var src = new List<Vector2>(points);              // copy: we insert into it below
        var dst = new List<Vector2>(n) { src[0] };        // the first post is the first point

        for (int i = 1; i < src.Count; i++)
        {
            float d = Vector2.Distance(src[i - 1], src[i]);
            if (accumulated + d >= interval && d > 0f)
            {
                // find next post on the segment via linear interpolation
                float t = (interval - accumulated) / d;
                var q = Vector2.LerpUnclamped(src[i - 1], src[i], t);
                dst.Add(q);

                // Insert q into the source so the NEXT walk starts from the post we just
                // placed, not from the original point before it. Then reset the odometer.
                src.Insert(i, q);
                accumulated = 0f;
            }
            else
            {
                accumulated += d;
            }
        }

        // Floating point rounding can leave us one short. If so, the last original point is
        // the missing post.
        while (dst.Count < n) dst.Add(src[src.Count - 1]);
        return dst;
    }

    //place the starting point at the top for comparison perposes
     public static float IndicativeAngle(IList<Vector2> points)
    {
        var c = Centroid(points);
        return Mathf.Atan2(c.y - points[0].y, c.x - points[0].x);
    }
    public static List<Vector2> RotateBy(IList<Vector2> points, float radians)
    {
        var c = Centroid(points);
        float cos = Mathf.Cos(radians), sin = Mathf.Sin(radians);
        var outPts = new List<Vector2>(points.Count);
        foreach (var p in points)
        {
            float dx = p.x - c.x, dy = p.y - c.y;
            outPts.Add(new Vector2(dx * cos - dy * sin + c.x,
                                    dx * sin + dy * cos + c.y));
        }
        return outPts;
    }

    //fix bounding box to a square and scale to comparable sizes
    public static List<Vector2> ScaleTo(IList<Vector2> points, float size)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in points)
        {
            minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
        }
        float w = maxX - minX, h = maxY - minY;
        float longest = Mathf.Max(w, h);
        if (longest < 1e-6f) longest = 1f;                      // all points identical

        bool oneDimensional = Mathf.Min(w, h) / longest < 0.25f;
        float sx = oneDimensional ? size / longest : size / w;
        float sy = oneDimensional ? size / longest : size / h;

        var outPts = new List<Vector2>(points.Count);
        foreach (var p in points) outPts.Add(new Vector2(p.x * sx, p.y * sy));
        return outPts;
    }

    //translate it to the origin for comparison purposes
    public static List<Vector2> TranslateTo(IList<Vector2> points, Vector2 target)
    {
        var c = Centroid(points);
        var outPts = new List<Vector2>(points.Count);
        foreach (var p in points) outPts.Add(p - c + target);
        return outPts;
    }
    
    //comparison
    //try to align roation even closer with smaller increments to find the best match via "wiggling it"
    static float DistanceAtBestAngle(List<Vector2> candidate, List<Vector2> template,
                                         float thetaA, float thetaB, float thetaDelta)
    {
        float x1 = Phi * thetaA + (1f - Phi) * thetaB;
        float f1 = DistanceAtAngle(candidate, template, x1);
        float x2 = (1f - Phi) * thetaA + Phi * thetaB;
        float f2 = DistanceAtAngle(candidate, template, x2);

        while (Mathf.Abs(thetaB - thetaA) > thetaDelta)
        {
            if (f1 < f2)
            {
                // The dip is on the left. Shrink the bracket from the right.
                thetaB = x2;
                x2 = x1; f2 = f1;
                x1 = Phi * thetaA + (1f - Phi) * thetaB;
                f1 = DistanceAtAngle(candidate, template, x1);
            }
            else
            {
                // The dip is on the right. Shrink the bracket from the left.
                thetaA = x1;
                x1 = x2; f1 = f2;
                x2 = (1f - Phi) * thetaA + Phi * thetaB;
                f2 = DistanceAtAngle(candidate, template, x2);
            }
        }
        return Mathf.Min(f1, f2);
    }

    /// Rotate the candidate by a trial angle, then measure how far it is from the template.
    static float DistanceAtAngle(List<Vector2> candidate, List<Vector2> template, float radians)
    {
        var rotated = RotateBy(candidate, radians);
        return PathDistance(rotated, template);
    }

    //comparing the distance between the candidate's path and the template's path to find best match
    static float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        float sum = 0f;
        int n = Mathf.Min(a.Count, b.Count);
        for (int i = 0; i < n; i++) sum += Vector2.Distance(a[i], b[i]);
        return sum / n;
    }

    //helpers
    /// Total length of the stroke: the sum of the gaps between consecutive points.
    public static float PathLength(IList<Vector2> points)
    {
        float len = 0f;
        for (int i = 1; i < points.Count; i++) len += Vector2.Distance(points[i - 1], points[i]);
        return len;
    }

    /// The average of all the points. "The middle" of the stroke.
    public static Vector2 Centroid(IList<Vector2> points)
    {
        var sum = Vector2.zero;
        foreach (var p in points) sum += p;
        return sum / points.Count;
    }
}
