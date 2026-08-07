using UnityEngine;

public class CarScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public float fwdspeed = 7f;
    public float speed = 5f;
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
        if (Input.GetKey(KeyCode.UpArrow) == true && alive == true){
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) == true && alive == true){
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow) == true && alive == true){
            transform.position += Vector3.left * fwdspeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow) == true && alive == true){
            transform.position += Vector3.right * fwdspeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
            logic.gameOver();
            alive = false;
    }
}
