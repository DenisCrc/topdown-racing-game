using UnityEngine;

// A closed racing line made of straights and rounded, tangent-continuous corners.
public sealed class CircuitPath
{
    struct Segment
    {
        public Vector2 start, control, end;
        public bool curve;
        public float length;
        public Vector2 Evaluate(float t)
        {
            if (!curve) return Vector2.LerpUnclamped(start, end, t);
            float s = 1 - t;
            return s * s * start + 2 * s * t * control + t * t * end;
        }
    }
    readonly Segment[] segments;
    readonly float[] ends;
    readonly float length;
    readonly float origin;

    public CircuitPath(Vector2[] corners, float cornerCut, float[] cornerCuts = null)
    {
        int count = corners.Length;
        var entry = new Vector2[count];
        var exit = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            Vector2 incoming = corners[i] - corners[(i + count - 1) % count];
            Vector2 outgoing = corners[(i + 1) % count] - corners[i];
            float desiredCut = cornerCuts == null ? cornerCut : cornerCuts[i];
            float cut = Mathf.Min(desiredCut, .4f * Mathf.Min(incoming.magnitude, outgoing.magnitude));
            entry[i] = corners[i] - incoming.normalized * cut;
            exit[i] = corners[i] + outgoing.normalized * cut;
        }
        segments = new Segment[count * 2]; ends = new float[segments.Length];
        float total = 0;
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            segments[i * 2] = new Segment { start = exit[i], end = entry[next], length = Vector2.Distance(exit[i], entry[next]) };
            var corner = new Segment { start = entry[next], control = corners[next], end = exit[next], curve = true };
            Vector2 previous = corner.start;
            for (int step = 1; step <= 24; step++)
            {
                Vector2 p = corner.Evaluate(step / 24f);
                corner.length += Vector2.Distance(previous, p); previous = p;
            }
            segments[i * 2 + 1] = corner;
        }
        for (int i = 0; i < segments.Length; i++) { total += segments[i].length; ends[i] = total; }
        length = total;
        // The finish and spawn sit on the main straight, away from tight corners.
        origin = segments[0].length * .5f;
    }
    public Vector2 Sample(float angle)
    {
        float distance = Mathf.Repeat(Mathf.Repeat(angle / (Mathf.PI * 2), 1) * length + origin, length);
        int low = 0, high = segments.Length - 1;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (distance < ends[mid]) high = mid; else low = mid + 1;
        }
        float start = low == 0 ? 0 : ends[low - 1];
        return segments[low].Evaluate((distance - start) / Mathf.Max(segments[low].length, .001f));
    }
}
