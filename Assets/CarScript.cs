using UnityEngine;

public class CarScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public Logic logic;
    public bool alive = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.name = "George Russell";
        logic = FindAnyObjectByType<Logic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision) {
            logic.gameOver();
            alive = false;
    }
}
