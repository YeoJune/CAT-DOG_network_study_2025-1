using UnityEngine;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;

// MonoBehaviourPunCallbacks으로 상속을 변경
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private TextMeshPro playerNameText;
    [SerializeField] private GameObject localIndicator;

    [Header("Health System")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshPro healthText;
    private int currentHealth;
    private float lastDamageTime = 0f;
    private float damageCooldown = 1.0f;

    
    // 네트워크 동기화를 위한 변수들
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float lerpSpeed = 15f;
    
    private Rigidbody2D rb;
    private Camera mainCamera;

    private float maxPredictionTime = 0.2f;      // 최대 예측 시간 제한
    private float snapThreshold = 1.0f;          // 즉시 보정 임계값

    private Vector3 lastValidatedPosition;
    private float lastValidationTime;
    private float maxAllowedSpeed;
    private float validationInterval = 0.2f;  // 200ms마다 검증

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    
        // 허용 속도 설정 (50% 여유)
        maxAllowedSpeed = moveSpeed * 1.5f;
        lastValidatedPosition = transform.position;
        lastValidationTime = Time.time;
    }
    
    private void Start()
    {
        // 로컬 플레이어 설정
        if (photonView.IsMine)
        {
            // 내 플레이어는 노란색으로 표시
            playerSprite.color = Color.yellow;
            // 로컬 플레이어 표시기 활성화
            localIndicator.SetActive(true);
            // 이름 표시
            playerNameText.text = "me";
        }
        else
        {
            // 다른 플레이어는 빨간색으로 표시
            playerSprite.color = Color.red;
            // 로컬 표시기 비활성화
            localIndicator.SetActive(false);
            // 다른 플레이어 이름 표시
            playerNameText.text = photonView.Owner.NickName;
            // 네트워크 위치 초기화
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }

        // 🎯 체력 시스템 초기화
        if (photonView.IsMine)
        {
            // 로컬 플레이어: 최대 체력으로 시작
            currentHealth = maxHealth;
            SetPlayerHealth(currentHealth);
        }
        else
        {
            // 원격 플레이어: Properties에서 체력 정보 가져오기
            currentHealth = GetPlayerHealth();
        }

        // 모든 플레이어의 체력 UI 업데이트
        UpdateHealthDisplay();
    }
    
    private void Update()
    {
        // 로컬 플레이어만 직접 제어
        if (photonView.IsMine)
        {
            // 주기적으로 이동 속도 검증
            if (Time.time - lastValidationTime >= validationInterval)
            {
                ValidateMovementSpeed();
            }
            // 기존 이동 처리...
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 moveInput = new Vector2(horizontalInput, verticalInput).normalized;
            rb.velocity = moveInput * moveSpeed;
            LookAtMouse();
        }
        // 원격 플레이어는 네트워크 위치로 보간
        else
        {
            // 🎯 거리에 따른 적응적 보간
            float distance = Vector3.Distance(transform.position, networkPosition);

            // 거리가 멀면 빠르게, 가까우면 천천히
            float adaptiveLerpSpeed = distance > 0.5f ? lerpSpeed * 1.5f : lerpSpeed;

            // 🎯 개선된 보간 적용
            transform.position = Vector3.Lerp(transform.position, networkPosition,
                adaptiveLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation,
                lerpSpeed * Time.deltaTime);
        }
    }
    
    private void LookAtMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        
        Vector3 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // IPunObservable 인터페이스 구현
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 데이터 보내기 (로컬 플레이어인 경우)
        if (stream.IsWriting)
        {
            // 위치, 회전, 속도 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
        }
        // 데이터 받기 (원격 플레이어인 경우)
        else
        {
            // 전송된 데이터 읽기
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
            Vector2 receivedVelocity = (Vector2)stream.ReceiveNext();

            // 🎯 지연시간 계산 및 제한
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            lag = Mathf.Min(lag, maxPredictionTime);  // 최대 200ms로 제한

            // 🎯 개선된 위치 예측
            Vector3 predictedPosition = receivedPosition + (Vector3)receivedVelocity * lag;

            // 🎯 예측 위치와 현재 위치 차이 계산
            float distance = Vector3.Distance(transform.position, predictedPosition);

            if (distance > snapThreshold)
            {
                // 차이가 크면 즉시 보정 (순간이동)
                networkPosition = predictedPosition;
                transform.position = predictedPosition;
            }
            else
            {
                // 차이가 작으면 부드러운 보간 목표 설정
                networkPosition = predictedPosition;
            }

            networkRotation = receivedRotation;
            rb.velocity = receivedVelocity;
        }
    }
    
    // 🎯 쿨다운 기반 충돌 처리 (연속 데미지 방지)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && photonView.IsMine)
        {
            // 쿨다운 확인으로 연속 데미지 방지
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                TakeDamage(25);
                lastDamageTime = Time.time;
            }
        }
    }

    // 🎯 체력 설정 (Player Properties 활용)
    private void SetPlayerHealth(int health)
    {
        Hashtable playerProps = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProps["Health"] = health;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    // 🎯 체력 조회
    private int GetPlayerHealth()
    {
        if (photonView.Owner.CustomProperties.ContainsKey("Health"))
        {
            return (int)photonView.Owner.CustomProperties["Health"];
        }
        return maxHealth;
    }

    // 🎯 데미지 처리
    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;  // 본인만 체력 조작 가능

        currentHealth = Mathf.Max(0, currentHealth - damage);
        SetPlayerHealth(currentHealth);

        if (currentHealth <= 0)
        {
            GameManager.Instance.GameOver();  // 게임 오버 처리

            PhotonNetwork.Destroy(gameObject);  // 플레이어 제거
        }
    }

    
    // 🎯 체력 UI 업데이트 및 색상 변경
    private void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;

            // 🎨 체력에 따른 색상 변경
            float healthRatio = (float)currentHealth / maxHealth;
            if (healthRatio <= 0.3f)
                healthText.color = Color.red;      // 위험
            else if (healthRatio <= 0.6f)
                healthText.color = Color.yellow;   // 주의
            else
                healthText.color = Color.green;    // 안전
        }
    }

    // 🎯 Player Properties 변경 감지
    public override void OnPlayerPropertiesUpdate(Player targetPlayer,
        Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("Health"))
        {
            currentHealth = (int)changedProps["Health"];
            UpdateHealthDisplay();
        }
    }

    // 🎯 이동 속도 검증 메서드
    private void ValidateMovementSpeed()
    {
        float deltaTime = Time.time - lastValidationTime;
        float distance = Vector3.Distance(lastValidatedPosition, transform.position);
        float currentSpeed = distance / deltaTime;
        if (currentSpeed > maxAllowedSpeed)
        {
            // 🚨 치트 감지 - 이전 유효 위치로 되돌림
            transform.position = lastValidatedPosition;
            rb.velocity = Vector2.zero;
            Debug.LogWarning($"🚨 Speed cheat detected and corrected! " +
                            $"Speed: {currentSpeed:F2} > Max: {maxAllowedSpeed:F2}");
        }
        else
        {
            // ✅ 정상 이동 - 유효 위치 업데이트
            lastValidatedPosition = transform.position;
        }
        lastValidationTime = Time.time;
    }

}