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
            ShootCurrentBullet();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            SwitchBullet();
        }
    }

    public void ShootCurrentBullet()
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0)
        {
            AnalyticsManager.Instance?.OnShootBlocked("no_bullet_prefab");
            return;
        }

        if (shootPoint == null)
        {
            AnalyticsManager.Instance?.OnShootBlocked("missing_shoot_point");
            return;
        }

        Rigidbody2D bullet = Instantiate(
            bulletPrefabs[currentBulletIndex],
            shootPoint.position,
            shootPoint.rotation
        );

        bullet.velocity = shootPoint.right * bulletSpeed;

        if (shootSound != null)
            shootSound.Play();

        string bulletName = GetBulletName(currentBulletIndex);
        AnalyticsManager.Instance?.OnBulletUsed(currentBulletIndex, bulletName, bulletSpeed);

        Destroy(bullet.gameObject, 15f);
    }

    public void SwitchBullet()
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0)
        {
            AnalyticsManager.Instance?.OnShootBlocked("no_bullet_prefab");
            return;
        }

        currentBulletIndex++;

        if (currentBulletIndex >= bulletPrefabs.Length)
            currentBulletIndex = 0;

        AnalyticsManager.Instance?.OnBulletSwitch(currentBulletIndex, GetBulletName(currentBulletIndex));
        Debug.Log("Switched to bullet type: " + currentBulletIndex);
    }

    public void SelectBullet(int bulletIndex, bool fireImmediately)
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0)
        {
            AnalyticsManager.Instance?.OnShootBlocked("no_bullet_prefab");
            return;
        }

        int clampedIndex = Mathf.Clamp(bulletIndex, 0, bulletPrefabs.Length - 1);
        bool changedBullet = currentBulletIndex != clampedIndex;
        currentBulletIndex = clampedIndex;

        if (changedBullet)
            AnalyticsManager.Instance?.OnBulletSwitch(currentBulletIndex, GetBulletName(currentBulletIndex));

        if (fireImmediately)
            ShootCurrentBullet();
    }

    private string GetBulletName(int bulletIndex)
    {
        return bulletIndex == 1 ? "redbullet" : "yellowbullet";
    }
}
