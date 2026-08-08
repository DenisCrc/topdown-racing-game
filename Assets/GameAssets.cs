using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public float fwdspeed = 7f;
    public float speed = 5f;
    public Logic logic;
    public CarScript carScript;
    
    // Reference for your particle object
    public GameObject reactorBoost; 

    void Start()
    {
        gameObject.name = "George Russell";
        logic = FindAnyObjectByType<Logic>();
        carScript = FindAnyObjectByType<CarScript>();
        
        // Ensure the particles are hidden when the game starts
        if (reactorBoost != null)
        {
            reactorBoost.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) == true && carScript.alive == true)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.DownArrow) == true && carScript.alive == true)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.LeftArrow) == true && carScript.alive == true)
        {
            transform.position += Vector3.left * fwdspeed * Time.deltaTime;
        }

        // Check if the RightArrow is being held down and the car is alive
        if (Input.GetKey(KeyCode.RightArrow) == true && carScript.alive == true)
        {
            transform.position += Vector3.right * fwdspeed * Time.deltaTime;
            
            // Turn ON the reactor boost
            if (reactorBoost != null && !reactorBoost.activeSelf)
            {
                reactorBoost.SetActive(true);
            }
        }
        else 
        {
            // Turn OFF the reactor boost if the RightArrow is released or the car is destroyed
            if (reactorBoost != null && reactorBoost.activeSelf)
            {
                reactorBoost.SetActive(false);
            }
        }
    }
}