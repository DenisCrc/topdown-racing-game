using UnityEngine;

// A gate's local up vector points in the driving direction.
public class RaceCheckpoint : MonoBehaviour
{
    public int index;
    public Logic logic;
    void OnTriggerEnter2D(Collider2D other)
    {
        var car = other.GetComponentInParent<CarScript>();
        if (car != null && car.alive &&
            Vector2.Dot(car.myRigidbody.linearVelocity, transform.up) > 0.1f)
            logic.PassCheckpoint(index);
    }
}
