using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
     private Transform spawnPoint;
     private GameObject player;
     private ItemsManagement items;
     
     private bool hasRespawned  = false;
    
    void Awake()
    {
        spawnPoint = gameObject.GetComponent<Transform>();
        player = GameObject.FindGameObjectWithTag("Player");
        items = player.GetComponent<ItemsManagement>();
        SpawnEnemy();

    }
    

    // Update is called once per frame
    void Update()
    {
        if (!hasRespawned && items.HasKey)
        {
            hasRespawned = true;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
    
}
