using UnityEngine;

// Keep the original spline layout and colliders; supply its missing road material
// and require the far-side gate before another finish crossing can count.
[DefaultExecutionOrder(-100)]
public class ClassicCircuit : MonoBehaviour
{
    Material roadMaterial;
    void Awake()
    {
        roadMaterial = new Material(Shader.Find("Sprites/Default"));
        roadMaterial.color = new Color(0.25f, 0.28f, 0.32f);
        GetComponent<MeshRenderer>().sharedMaterial = roadMaterial;
        var logic = FindAnyObjectByType<Logic>();
        logic.checkpointCount = 2;
        var gate = new GameObject("Far Side Checkpoint");
        gate.transform.SetParent(transform, true);
        gate.transform.position = new Vector3(-13, 71, 0);
        gate.transform.up = Vector3.left;
        var collider = gate.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10, 0.5f);
        collider.isTrigger = true;
        var checkpoint = gate.AddComponent<RaceCheckpoint>();
        checkpoint.index = 1;
        checkpoint.logic = logic;
    }
    void OnDestroy() { if (roadMaterial != null) Release(roadMaterial); }
    static void Release(Object asset)
    {
        if (Application.isPlaying) Destroy(asset);
        else DestroyImmediate(asset);
    }
}
