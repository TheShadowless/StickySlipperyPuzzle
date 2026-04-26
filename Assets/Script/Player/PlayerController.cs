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

    private Rigidbody2D rb;
    private Animator anim;
    private Shoot shootController;
    private Collider2D playerCollider;

    private bool grounded;
    private bool isInB2 = false;

    private float defaultMoveSpeed;
    private float defaultJumpForce;
    private Vector3 startPosition;
    private bool gameplayEnabled = true;

    public bool IsGrounded => grounded;
    public Vector3 StartPosition => startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        shootController = GetComponent<Shoot>();
        playerCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
    }

    void Start()
    {
        AnalyticsManager.Instance?.OnLevelStart();

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
    }

    void Update()
    {
        if (!gameplayEnabled)
            return;

        HandleMovement();
        HandleJump();
        UpdateAnimation();
        HandleSkillInput();
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

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
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                anim.SetTrigger("Jump");
            }
        }
    }

    void UpdateAnimation()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isGrounded", grounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void HandleSkillInput()
    {
        if (shootController == null)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
            shootController.SelectBullet(0, true);

        if (Input.GetKeyDown(KeyCode.X))
            shootController.SelectBullet(1, true);
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
            GameSessionManager.Instance?.HandlePlayerDeath(this);
        }
        else if (other.CompareTag("GO"))
        {
            GameSessionManager.Instance?.HandleLevelComplete(this);
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

    public void SetGameplayEnabled(bool enabled)
    {
        gameplayEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            if (playerCollider != null)
                playerCollider.enabled = false;
            return;
        }

        rb.simulated = true;
        if (playerCollider != null)
            playerCollider.enabled = true;
    }

    public void Respawn(Vector3 position)
    {
        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        isInB2 = false;
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        SetGameplayEnabled(true);
    }
}
