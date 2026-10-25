using UnityEngine;

// Builds each new circuit from a sampled centre line, with matching visible curbs,
// solid boundaries and ordered timing gates. No generated mesh asset is required.
[DefaultExecutionOrder(-100)]
public class CircuitTrack : MonoBehaviour
{
    public int layout;
    int Samples => layout >= 3 ? 384 : 192;
    public int GateCount => layout >= 3 ? 8 : 4;
    Mesh roadMesh;
    Material roadMaterial;

    public static Vector2 Point(int layout, float angle)
    {
        switch (layout)
        {
            case 0: return new Vector2(24 * Mathf.Cos(angle), 16 * Mathf.Sin(angle));
            case 1:
                float canyonRadius = 23 + 5 * Mathf.Cos(3 * angle);
                return new Vector2(canyonRadius * Mathf.Cos(angle), 0.85f * canyonRadius * Mathf.Sin(angle));
            case 2: return new Vector2(28 * Mathf.Cos(angle) + 4 * Mathf.Sin(2 * angle), 20 * Mathf.Sin(angle));
            case 3: return new Vector2((44 + 8 * Mathf.Cos(2 * angle)) * Mathf.Cos(angle), 30 * Mathf.Sin(angle));
            default:
                float enduranceRadius = 42 + 7 * Mathf.Cos(3 * angle);
                return new Vector2(1.15f * enduranceRadius * Mathf.Cos(angle), 0.95f * enduranceRadius * Mathf.Sin(angle));
        }
    }

    void Awake()
    {
        var theme = PastelTheme.Current;
        float width = layout >= 3 ? 6.5f : layout == 0 ? 6 : layout == 2 ? 5.5f : 4.5f;
        var vertices = new Vector3[Samples * 4];
        var colors = new Color[vertices.Length];
        var triangles = new int[Samples * 18];
        var inner = new Vector2[Samples + 1];
        var outer = new Vector2[Samples + 1];
        for (int i = 0; i < Samples; i++)
        {
            float angle = i * Mathf.PI * 2 / Samples;
            Vector2 p = Point(layout, angle);
            Vector2 tangent = (Point(layout, angle + 0.01f) - Point(layout, angle - 0.01f)).normalized;
            Vector2 normal = new Vector2(-tangent.y, tangent.x);
            float[] offsets = { -width / 2, -width / 2 + 0.25f, width / 2 - 0.25f, width / 2 };
            for (int lane = 0; lane < 4; lane++)
            {
                vertices[i * 4 + lane] = p + normal * offsets[lane];
                colors[i * 4 + lane] = lane == 0 || lane == 3
                    ? (i / 4 % 2 == 0 ? theme.panel : theme.coral)
                    : (i == 0 ? theme.panel : theme.road);
                // Mesh vertex colors are linear; UI and material Color properties are converted by Unity.
                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                    colors[i * 4 + lane] = colors[i * 4 + lane].linear;
            }
            outer[i] = p - normal * width / 2;
            inner[i] = p + normal * width / 2;
            for (int lane = 0; lane < 3; lane++)
            {
                int a = i * 4 + lane, b = ((i + 1) % Samples) * 4 + lane;
                int t = i * 18 + lane * 6;
                triangles[t] = a; triangles[t + 1] = b; triangles[t + 2] = a + 1;
                triangles[t + 3] = a + 1; triangles[t + 4] = b; triangles[t + 5] = b + 1;
            }
        }
        outer[Samples] = outer[0]; inner[Samples] = inner[0];
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
