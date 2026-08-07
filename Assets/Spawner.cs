using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject obstacolPrefab;
    public float spawnRate = 2f;
    private float timer = 0f;
    public float heightOffset = 10f;
    public bool spawnEnabled = true;
    public Logic logicScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = FindAnyObjectByType<Logic>();
        SpawnObstacol();
    }

    // Update is called once per frame
    void Update()
    {
        if(!spawnEnabled) return;
        else if(logicScript.alive == false) return;
        else    
            if(timer < spawnRate){
                timer += Time.deltaTime;
            } else {
            SpawnObstacol();
            timer = 0f;
            }
    
    }

    void SpawnObstacol(){
        float lowestPoint = transform.position.y - heightOffset;
        float highestPoint = transform.position.y + heightOffset;
        float randomY = Random.Range(lowestPoint, highestPoint);
        Instantiate(obstacolPrefab, new Vector3(transform.position.x, randomY, transform.position.z), Quaternion.identity);
    }
}
