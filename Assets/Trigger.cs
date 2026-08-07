using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Logic logicScript;

    public bool collisionyes = false;
    void Start()
    {
        // Searches the scene directly for the Logic script component
        logicScript = FindAnyObjectByType<Logic>();

        if (logicScript == null)
        {
            Debug.LogError("Still couldn't find Logic! Is the GameObject disabled in the Hierarchy?");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collisionyes == false && collision.gameObject.name == "George Russell"){
            collisionyes = true;
            logicScript.addScore(1);
        }
    }
}