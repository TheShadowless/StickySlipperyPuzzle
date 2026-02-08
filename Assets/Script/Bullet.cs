using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // เปลี่ยนจาก OnCollisionEnter2D → OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null)
        {
            // หาตำแหน่งที่ชน (ใช้ตำแหน่งตัวเองแทน เพราะ Trigger ไม่มี contact point)
            Vector2 hitPoint = transform.position;
            Debug.Log("Bullet triggered with Tilemap at: " + hitPoint);

            // หยุดการเคลื่อนที่ของกระสุน
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // ทำให้กระสุนหยุดอยู่กับที่
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
