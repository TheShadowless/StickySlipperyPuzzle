using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    public GameObject rockPrefab;   // หิน prefab
    public float spawnInterval = 2f; // เวลาห่างแต่ละก้อน
    public float spawnRangeX = 5f;   // ระยะสุ่มแกน X

    void Start()
    {
        // เรียกฟังก์ชัน SpawnRock ทุกๆ ช่วงเวลา
        InvokeRepeating("SpawnRock", 1f, spawnInterval);
    }

    void SpawnRock()
    {
        // สุ่มตำแหน่ง X รอบๆ Spawner
        float randomX = transform.position.x + Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);

        // สร้างหิน
        Instantiate(rockPrefab, spawnPos, Quaternion.identity);
    }
}