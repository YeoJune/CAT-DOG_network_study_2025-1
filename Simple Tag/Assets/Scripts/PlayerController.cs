using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color runnerColor = Color.blue;
    public Color taggerColor = Color.red;
    public Color invulnerableColor = Color.gray;
    
    // State
    private PlayerRole currentRole = PlayerRole.Runner;
    private int score = 0;
    private Rigidbody2D rb;
    private GameManager gameManager;
    
    // Properties
    public PlayerRole CurrentRole => currentRole;
    public int Score => score;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    protected virtual void Start()
    {
        UpdateVisuals();
    }
    
    protected virtual void Update()
    {
        HandleMovement();
        EnforcePlayAreaBounds();
        UpdateVisuals();
    }
    
    protected virtual void HandleMovement()
    {
        // Human player input (override in AIPlayer)
        Vector2 moveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        ).normalized;
        
        if (rb != null)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }
    
    public void EnforcePlayAreaBounds()
    {
        if (gameManager != null)
        {
            transform.position = gameManager.ClampToPlayArea(transform.position);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController otherPlayer = other.GetComponent<PlayerController>();
        if (otherPlayer != null && otherPlayer != this)
        {
            HandlePlayerCollision(otherPlayer);
        }
    }
    
    private void HandlePlayerCollision(PlayerController otherPlayer)
    {
        if (gameManager != null && gameManager.IsGameActive())
        {
            // 내가 태거이고 상대가 러너일 때만 태그 가능
            if (currentRole == PlayerRole.Tagger && gameManager.CanPlayerTag(this) && gameManager.CanPlayerBeTagged(otherPlayer))
            {
                gameManager.OnPlayerTagged(this, otherPlayer);
            }
        }
    }
    
    public void SetRole(PlayerRole newRole)
    {
        currentRole = newRole;
        UpdateVisuals();
    }
    
    public void UpdateScore(int newScore)
    {
        score = newScore;
    }
    
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }
    
    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer != null && gameManager != null)
        {
            // 무적 상태 체크
            bool isInvulnerable = !gameManager.CanPlayerBeTagged(this) && currentRole == PlayerRole.Runner;
            
            if (isInvulnerable)
            {
                spriteRenderer.color = invulnerableColor;
            }
            else
            {
                spriteRenderer.color = currentRole == PlayerRole.Tagger ? taggerColor : runnerColor;
            }
        }
    }
}