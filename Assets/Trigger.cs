using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Logic logic;

    void Start()
    {
        logic = FindAnyObjectByType<Logic>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            logic.completeLap();
        }
    }
}
