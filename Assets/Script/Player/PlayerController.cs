using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Next Scene")]
    public string nextSceneName;

    // Components
    private Rigidbody2D rb;
    private Animator anim;

    // States
    private bool grounded;
    private bool isInB2 = false;

    // Default values
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        AnalyticsManager.Instance.OnLevelStart();

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        UpdateAnimation();
        HandleSkillInput();
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        if (move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
    }

    void HandleJump()
    {
        grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded || isInB2)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                anim.SetTrigger("Jump");
            }
        }
    }

    void UpdateAnimation()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("isGrounded", grounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) ShootGel("Sticky");
        if (Input.GetKeyDown(KeyCode.X)) ShootGel("Slippery");
    }

    void ShootGel(string type)
    {
        AnalyticsManager.Instance.OnBulletUsed(type);
        Debug.Log("Shoot " + type);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("B1"))
        {
            moveSpeed = defaultMoveSpeed * 2f;
            jumpForce = defaultJumpForce * 2f;
        }
        else if (other.CompareTag("B2"))
        {
            moveSpeed = defaultMoveSpeed * 0.5f;
            jumpForce = defaultJumpForce * 0.5f;
            isInB2 = true;
        }
        else if (other.CompareTag("D"))
        {
            AnalyticsManager.Instance.OnPlayerDeath();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (other.CompareTag("GO"))
        {
            AnalyticsManager.Instance.OnLevelComplete();

            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("B1") || other.CompareTag("B2"))
        {
            moveSpeed = defaultMoveSpeed;
            jumpForce = defaultJumpForce;
        }

        if (other.CompareTag("B2"))
            isInB2 = false;
    }
}