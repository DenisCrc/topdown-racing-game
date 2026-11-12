using UnityEngine;
using UnityEngine.SceneManagement;

// Builds each new circuit from a sampled centre line, with matching visible curbs,
// solid boundaries and ordered timing gates. No generated mesh asset is required.
[DefaultExecutionOrder(-100)]
public class CircuitTrack : MonoBehaviour
{
    public int layout;
    int Samples => layout >= 3 ? 768 : 384;
    public int GateCount => layout >= 3 ? 12 : 8;
    Mesh roadMesh;
    Material roadMaterial;

    // Original Grand Prix-style routes: asymmetric outlines, long straights,
    // slow chicanes and linked fast bends. Each corner has its own rounding.
    static readonly CircuitPath[] Paths = {
        new CircuitPath(new[] {
            new Vector2(48,-36), new Vector2(48,28), new Vector2(36,44), new Vector2(10,48),
            new Vector2(-8,34), new Vector2(-26,36), new Vector2(-44,20), new Vector2(-38,0),
            new Vector2(-18,-4), new Vector2(-14,-20), new Vector2(-32,-28), new Vector2(-30,-44),
            new Vector2(-8,-48), new Vector2(12,-32), new Vector2(32,-44)
        }, 8, new[] { 9f, 12f, 14f, 10f, 8f, 10f, 10f, 9f, 6f, 5.5f, 5.5f, 8f, 9f, 6f, 6f }),
        new CircuitPath(new[] {
            new Vector2(40,-42), new Vector2(40,32), new Vector2(24,48), new Vector2(6,44),
            new Vector2(-6,26), new Vector2(-28,32), new Vector2(-42,16), new Vector2(-38,-4),
            new Vector2(-20,-12), new Vector2(-30,-28), new Vector2(-16,-42), new Vector2(0,-32),
            new Vector2(14,-48), new Vector2(28,-54)
        }, 8, new[] { 7f, 10f, 8f, 7f, 5f, 7f, 8f, 7f, 6.5f, 5f, 5f, 5f, 7f, 7f }),
        new CircuitPath(new[] {
            new Vector2(44,-48), new Vector2(44,34), new Vector2(28,34), new Vector2(28,52),
            new Vector2(10,60), new Vector2(-12,58), new Vector2(-32,42), new Vector2(-44,20),
            new Vector2(-44,-2), new Vector2(-28,-6), new Vector2(-28,-24), new Vector2(-42,-28),
            new Vector2(-36,-48), new Vector2(-12,-58), new Vector2(16,-60), new Vector2(36,-56)
        }, 8, new[] { 10f, 5.5f, 5.5f, 10f, 12f, 15f, 15f, 12f, 9f, 6f, 6f, 7f, 12f, 15f, 12f, 8f }),
        new CircuitPath(new[] {
            new Vector2(68,-50), new Vector2(68,42), new Vector2(48,62), new Vector2(18,54),
            new Vector2(-4,66), new Vector2(-32,54), new Vector2(-48,26), new Vector2(-80,6),
            new Vector2(-66,-24), new Vector2(-42,-28), new Vector2(-28,-8), new Vector2(-8,-12),
            new Vector2(10,-42), new Vector2(32,-50), new Vector2(40,-30), new Vector2(22,-14),
            new Vector2(26,4), new Vector2(44,10), new Vector2(50,-12), new Vector2(48,-50)
        }, 8, new[] { 8f, 12f, 14f, 11f, 14f, 14f, 15f, 14f, 12f, 10f, 8f, 8f, 10f, 7f, 7f, 7f, 8f, 8f, 10f, 8f }),
        new CircuitPath(new[] {
            new Vector2(88,-52), new Vector2(88,50), new Vector2(60,62), new Vector2(42,44),
            new Vector2(48,22), new Vector2(22,12), new Vector2(4,28), new Vector2(-18,24),
            new Vector2(-36,44), new Vector2(-70,42), new Vector2(-88,14), new Vector2(-70,-10),
            new Vector2(-40,-8), new Vector2(-34,-30), new Vector2(-64,-46), new Vector2(-56,-66),
            new Vector2(-16,-60), new Vector2(8,-34), new Vector2(30,-40), new Vector2(42,-18),
            new Vector2(60,-20), new Vector2(64,-44)
        }, 8, new[] { 10f, 12f, 10f, 9f, 8f, 6.5f, 8f, 7f, 10f, 12f, 12f, 10f, 8f, 7f, 7f, 9f, 12f, 8f, 7f, 7f, 7f, 8f }),
    };

    public static Vector2 Point(int layout, float angle) => Paths[Mathf.Clamp(layout, 0, Paths.Length - 1)].Sample(angle);

    void Awake()
    {
        int index = System.Array.IndexOf(LevelCatalog.Scenes, SceneManager.GetActiveScene().name);
        if (index >= 0) layout = LevelCatalog.Layouts[index];
        var theme = PastelTheme.Current;
        float width = layout >= 3 ? 6.5f : layout == 0 ? 6 : layout == 2 ? 5.5f : 4.5f;
        float h = width / 2;
        // Cross-section bands from one side of the track to the other: runoff, dark outline,
        // red/white curb, white edge line, asphalt, then mirrored. The colliders stay on ±h.
        const float Runoff = 1.1f, Outline = 0.14f, Curb = 0.5f, Line = 0.12f;
        float[] edges = { -h - Runoff, -h - Outline, -h, -h + Curb, -h + Curb + Line,
                           h - Curb - Line, h - Curb, h, h + Outline, h + Runoff };
        int[] kind = { 0, 1, 2, 3, 4, 3, 2, 1, 0 };
        // Draw order: runoff first so asphalt and curbs always sit on top where sections pass close.
        int[] drawOrder = { 0, 8, 1, 7, 2, 6, 3, 5, 4 };
        Color runoff = Color.Lerp(theme.accent, theme.ink, 0.12f);
        Color curbRed = new Color32(204, 88, 72, 255);
        Color asphalt = Color.Lerp(theme.road, theme.ink, 0.3f);
        int bands = kind.Length;
        var points = new Vector2[Samples];
        var normals = new Vector2[Samples];
        var inner = new Vector2[Samples + 1];
        var outer = new Vector2[Samples + 1];
        for (int i = 0; i < Samples; i++)
        {
            float angle = i * Mathf.PI * 2 / Samples;
            points[i] = Point(layout, angle);
            Vector2 tangent = (Point(layout, angle + 0.01f) - Point(layout, angle - 0.01f)).normalized;
            normals[i] = new Vector2(-tangent.y, tangent.x);
            outer[i] = points[i] - normals[i] * h;
            inner[i] = points[i] + normals[i] * h;
        }
        outer[Samples] = outer[0]; inner[Samples] = inner[0];
        // One flat-coloured quad per band per segment, so stripes and edges stay crisp.
        var vertices = new Vector3[Samples * bands * 4];
        var colors = new Color[vertices.Length];
        var triangles = new int[Samples * bands * 6];
        int q = 0;
        foreach (int band in drawOrder)
            for (int i = 0; i < Samples; i++, q++)
            {
                int j = (i + 1) % Samples;
                Color c = kind[band] switch
                {
                    0 => runoff,
                    1 => theme.ink,
                    2 => i / 4 % 2 == 0 ? theme.panel : curbRed,
                    3 => theme.panel,
                    _ => i < 2 ? theme.panel : asphalt,
                };
                // Mesh vertex colors are linear; UI and material Color properties are converted by Unity.
                if (QualitySettings.activeColorSpace == ColorSpace.Linear) c = c.linear;
                int v = q * 4;
                vertices[v] = points[i] + normals[i] * edges[band];
                vertices[v + 1] = points[i] + normals[i] * edges[band + 1];
                vertices[v + 2] = points[j] + normals[j] * edges[band];
                vertices[v + 3] = points[j] + normals[j] * edges[band + 1];
                colors[v] = colors[v + 1] = colors[v + 2] = colors[v + 3] = c;
                int t = q * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
        gameObject.AddComponent<EdgeCollider2D>().points = outer;
        gameObject.AddComponent<EdgeCollider2D>().points = inner;
        roadMesh = new Mesh { name = "Circuit Road", vertices = vertices, triangles = triangles, colors = colors };
        roadMesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().sharedMesh = roadMesh;
        roadMaterial = new Material(Resources.Load<Material>("CarMaterial"));
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;

        var logic = FindAnyObjectByType<Logic>();
        logic.checkpointCount = GateCount;
        for (int i = 0; i < GateCount; i++)
        {
            float angle = i * Mathf.PI * 2 / GateCount;
            Vector2 direction = (Point(layout, angle + 0.01f) - Point(layout, angle - 0.01f)).normalized;
            var gate = new GameObject(i == 0 ? "Finish" : "Checkpoint " + i);
            gate.transform.SetParent(transform, false);
            gate.transform.localPosition = Point(layout, angle);
            gate.transform.up = direction;
            gate.AddComponent<BoxCollider2D>().size = new Vector2(width, 0.3f);
            gate.GetComponent<BoxCollider2D>().isTrigger = true;
            var checkpoint = gate.AddComponent<RaceCheckpoint>();
            checkpoint.index = i;
            checkpoint.logic = logic;
        }
        var car = FindAnyObjectByType<CarScript>();
        car.transform.position = (Vector3)Point(layout, -0.15f) + new Vector3(0, 0, -2);
        Vector2 forward = Point(layout, -0.14f) - Point(layout, -0.16f);
        car.transform.up = forward;
        var camera = Camera.main;
        camera.backgroundColor = theme.background;
        camera.transform.position = car.transform.position + new Vector3(0, 0, -8);
        camera.transform.rotation = car.transform.rotation;
        camera.orthographicSize = 9;
    }

    void OnDestroy()
    {
        if (roadMesh != null) Release(roadMesh);
        if (roadMaterial != null) Release(roadMaterial);
    }
    static void Release(Object asset)
    {
        if (Application.isPlaying) Destroy(asset);
        else DestroyImmediate(asset);
    }
}
