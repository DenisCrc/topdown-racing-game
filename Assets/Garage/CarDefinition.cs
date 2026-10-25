using UnityEngine;

[CreateAssetMenu(menuName = "Racing/Car Definition")]
public class CarDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public int order;
    public Sprite straight;
    public Sprite left;
    public Sprite right;
    public float maxSpeed = 12;
    public float acceleration = 14;
    public float steering = 3.2f;
    public float driftFactor = 0.92f;
    public Vector2 colliderSize = new Vector2(0.65f, 1.2f);
    public Vector2 colliderOffset;
    public Vector2 leftTire = new Vector2(-0.28f, -0.45f);
    public Vector2 rightTire = new Vector2(0.28f, -0.45f);
}
