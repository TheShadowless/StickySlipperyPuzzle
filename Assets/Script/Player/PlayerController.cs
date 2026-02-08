using UnityEngine;
using UnityEngine.SceneManagement;   // สำหรับโหลดฉาก

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;

    [Header("Ground Check")]
    public Transform groundCheck;       // จุดเช็คพื้น (วางไว้ใต้เท้าใน Inspector)
    public float checkRadius = 0.2f;    // รัศมีวงกลมตรวจสอบ
    public LayerMask groundLayer;       // เลเยอร์ของพื้น

    private bool grounded;

    // เก็บค่า default ไว้
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    [Header("Next Scene")]
    public string nextSceneName;   // ใส่ชื่อฉากถัดไปจาก Inspector

    // ===== เพิ่มตัวแปรใหม่ =====
    private bool isInB2 = false;   // เช็คว่ากำลังชนกับ B2 อยู่หรือไม่

    void Start() {
        rb = GetComponent<Rigidbody2D>();

        // เก็บค่าปกติไว้ก่อน
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    void Update() {
        float move = Input.GetAxis("Horizontal");

        // เดินซ้าย-ขวา
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        // อัปเดตสถานะ grounded ทุกเฟรม
        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // กระโดด
        if (Input.GetButtonDown("Jump")) {
            if (isInB2) {
                // กำลังอยู่ใน B2 → กระโดดได้ตลอดเวลา
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            } 
            else if (grounded) {
                // กรณีปกติ → กระโดดได้เฉพาะตอนอยู่บนพื้น
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        // ยิงเจลซ้าย-ขวา
        if (Input.GetKeyDown(KeyCode.Z)) ShootGel("Sticky");
        if (Input.GetKeyDown(KeyCode.X)) ShootGel("Slippery");
    }

    void ShootGel(string type) {
        Debug.Log("Shoot " + type);
        // TODO: สร้าง prefab เจลแล้ว Instantiate ตรงนี้
    }

    // =====================
    // ระบบชนวัตถุพิเศษ (Trigger)
    // =====================
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("B1")) {
            moveSpeed = defaultMoveSpeed * 2f;
            jumpForce = defaultJumpForce * 2f;
        }
        else if (other.CompareTag("B2")) {
            moveSpeed = defaultMoveSpeed * 0.5f;
            jumpForce = defaultJumpForce * 0.5f;
            isInB2 = true;   // เปิดโหมดกระโดดไม่จำกัด
        }
        else if (other.CompareTag("D")) {
            // โหลดฉากปัจจุบันใหม่
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (other.CompareTag("GO")) {
            // โหลดฉากถัดไป (ที่กำหนดไว้ใน Inspector)
            if (!string.IsNullOrEmpty(nextSceneName)) {
                SceneManager.LoadScene(nextSceneName);
            } else {
                Debug.LogWarning("ยังไม่ได้ใส่ชื่อ Scene ใน PlayerController!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("B1") || other.CompareTag("B2")) {
            // รีเซ็ตกลับเป็นค่าเดิม
            moveSpeed = defaultMoveSpeed;
            jumpForce = defaultJumpForce;
        }

        if (other.CompareTag("B2")) {
            isInB2 = false;  // ออกจากโหมดกระโดดไม่จำกัด
        }
    }
}
