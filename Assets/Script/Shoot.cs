using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Transform shootPoint;
    [SerializeField] Rigidbody2D[] bulletPrefabs;
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] AudioSource shootSound;

    private int currentBulletIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ShootBullet();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            SwitchBullet();
        }
    }

    void ShootBullet()
    {
        if (bulletPrefabs.Length == 0) return;

        Rigidbody2D bullet = Instantiate(
            bulletPrefabs[currentBulletIndex],
            shootPoint.position,
            shootPoint.rotation
        );

        bullet.velocity = shootPoint.right * bulletSpeed;

        if (shootSound != null)
            shootSound.Play();

        // กำหนดชื่อ bullet สำหรับ analytics
        string bulletName = "yellowbullet";

        if (currentBulletIndex == 1)
            bulletName = "redbullet";

        // ส่ง event ไป analytics
        AnalyticsManager.Instance.OnBulletUsed(bulletName);

        Destroy(bullet.gameObject, 15f);
    }

    void SwitchBullet()
    {
        currentBulletIndex++;

        if (currentBulletIndex >= bulletPrefabs.Length)
            currentBulletIndex = 0;

        Debug.Log("Switched to bullet type: " + currentBulletIndex);
    }
}