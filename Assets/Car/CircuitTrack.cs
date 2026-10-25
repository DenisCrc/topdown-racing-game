using UnityEngine;

// Builds each new circuit from a sampled centre line, with matching visible curbs,
// solid boundaries and ordered timing gates. No generated mesh asset is required.
[DefaultExecutionOrder(-100)]
public class CircuitTrack : MonoBehaviour
{
    public int layout;
    const int Samples = 192;
    Mesh roadMesh;
    Material roadMaterial;

    public static Vector2 Point(int layout, float angle)
    {
        if (layout == 0)
            return new Vector2(24 * Mathf.Cos(angle), 16 * Mathf.Sin(angle));
        float radius = 23 + 5 * Mathf.Cos(3 * angle);
        return new Vector2(radius * Mathf.Cos(angle), 0.85f * radius * Mathf.Sin(angle));
    }

    void Awake()
    {
        float width = layout == 0 ? 6 : 4.5f;
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
                    ? (i / 4 % 2 == 0 ? Color.white : new Color(0.9f, 0.25f, 0.15f))
                    : (i == 0 ? Color.white : new Color(0.22f, 0.25f, 0.29f));
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
        roadMaterial = new Material(Shader.Find("Sprites/Default"));
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;

        var logic = FindAnyObjectByType<Logic>();
        logic.checkpointCount = 4;
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI / 2;
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
        camera.backgroundColor = layout == 0 ? new Color(0.08f, 0.28f, 0.25f) : new Color(0.32f, 0.19f, 0.12f);
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
