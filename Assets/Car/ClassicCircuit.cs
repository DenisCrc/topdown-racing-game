using UnityEngine;

// Keep the original spline layout and colliders; supply its missing road material
// and require the far-side gate before another finish crossing can count.
[DefaultExecutionOrder(-100)]
public class ClassicCircuit : MonoBehaviour
{
    Material roadMaterial;
    void Awake()
    {
        roadMaterial = new Material(Resources.Load<Material>("CarMaterial"));
        roadMaterial.color = PastelTheme.Current.road;
        GetComponent<MeshRenderer>().sharedMaterial = roadMaterial;
        Camera.main.backgroundColor = PastelTheme.Current.background;
        var finish = FindAnyObjectByType<Trigger>();
        var finishCollider = finish.GetComponent<BoxCollider2D>();
        Vector3 center = finish.transform.TransformPoint(finishCollider.offset);
        var stripe = new GameObject("Finish Stripe");
        stripe.transform.SetParent(transform, true);
        stripe.transform.position = new Vector3(center.x, center.y, -1);
        stripe.transform.localScale = new Vector3(0.3f, finishCollider.size.y, 1);
        var stripeRenderer = stripe.AddComponent<SpriteRenderer>();
        stripeRenderer.sprite = Resources.Load<Sprite>("FinishStripe");
        stripeRenderer.sharedMaterial = Resources.Load<Material>("CarMaterial");
        stripeRenderer.sortingOrder = 1;
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
