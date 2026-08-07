using UnityEngine;

public class Obstacol : MonoBehaviour
{
    public float speed = 5;
    public float destroyX = -20;
    public Logic logicScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        logicScript = FindAnyObjectByType<Logic>();
    }

    // Update is called once per frame
    void Update()
    {
        if(logicScript.alive == false) return;
        else
        transform.position += Vector3.left * speed * Time.deltaTime;
        if(transform.position.x < destroyX){
            //Debug.Log("Destroying " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
