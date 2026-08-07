using UnityEngine;

public class Obstacol : MonoBehaviour
{
    public float speed = 5;
    public float destroyX = -20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
    
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if(transform.position.x < destroyX){
            //Debug.Log("Destroying " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
