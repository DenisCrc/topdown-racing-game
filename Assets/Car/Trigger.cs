using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Logic logic;
    void Start() { logic = FindAnyObjectByType<Logic>(); }
    void OnTriggerEnter2D(Collider2D other)
    {
        var car = other.GetComponentInParent<CarScript>();
        if (car != null && car.alive && car.myRigidbody.linearVelocity.x > 0.1f)
            logic.completeLap();
    }
}
