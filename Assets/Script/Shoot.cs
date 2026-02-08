using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Transform shootPoint;           // จุดยิง
    [SerializeField] Rigidbody2D[] bulletPrefabs;    // กระสุนหลายแบบ (ลากใส่ใน Inspector)
    [SerializeField] float bulletSpeed = 10f;        // ความเร็วกระสุน
    [SerializeField] AudioSource shootSound;         // เสียงยิง

    private int currentBulletIndex = 0;              // เก็บว่าใช้อยู่แบบไหน

    void Update()
    {
        // ยิงเมื่อกด O
        if (Input.GetKeyDown(KeyCode.O))
        {
            ShootBullet();
        }

        // สลับกระสุนเมื่อกด I
        if (Input.GetKeyDown(KeyCode.I))
        {
            SwitchBullet();
        }
    }

    void ShootBullet()
    {
        // ตรวจว่ามีกระสุนให้เลือกไหม
        if (bulletPrefabs.Length == 0) return;

        // สร้างกระสุนตามชนิดที่เลือก
        Rigidbody2D bullet = Instantiate(bulletPrefabs[currentBulletIndex], shootPoint.position, shootPoint.rotation);

        // ใส่ความเร็วให้กระสุน
        bullet.velocity = shootPoint.right * bulletSpeed;

        // เล่นเสียง
        if (shootSound != null)
        {
            shootSound.Play();
        }

        // ลบกระสุนหลัง 5 วิ
        Destroy(bullet.gameObject, 15f);
    }

    void SwitchBullet()
    {
        currentBulletIndex++;
        if (currentBulletIndex >= bulletPrefabs.Length)
        {
            currentBulletIndex = 0; // วนกลับไปแบบแรก
        }

        Debug.Log("Switched to bullet type: " + currentBulletIndex);
    }
}
