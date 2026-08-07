using UnityEngine;

public class CarScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public float fwdspeed = 7f;
    public float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.name = "George Russell";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) == true){
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) == true){
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow) == true){
            transform.position += Vector3.left * fwdspeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow) == true){
            transform.position += Vector3.right * fwdspeed * Time.deltaTime;
        }
    }
}
