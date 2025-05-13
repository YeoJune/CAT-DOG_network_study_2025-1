using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private TextMeshPro playerNameText;
    [SerializeField] private GameObject localIndicator;
    
    // 네트워크 동기화를 위한 변수들
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float lerpSpeed = 15f;
    
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }
    
    private void Start()
    {
        // --- TODO ---
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
        // ------
    }
    
    private void Update()
    {
        // --- TODO ---
        // 로컬 플레이어만 직접 제어
        if (photonView.IsMine)
        {
            // 이동 입력 처리
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 moveInput = new Vector2(horizontalInput, verticalInput).normalized;
            
            // 이동 적용
            rb.velocity = moveInput * moveSpeed;
            
            // 마우스 방향으로 회전
            LookAtMouse();
        }
        // 원격 플레이어는 네트워크 위치로 보간
        else
        {
            // 위치와 회전을 네트워크 값으로 부드럽게 보간
            transform.position = Vector3.Lerp(transform.position, networkPosition, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, lerpSpeed * Time.deltaTime);
        }
        // ------
    }
    
    private void LookAtMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        
        Vector3 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // --- TODO ---
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
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
            
            // 지연시간에 따른 위치 예측
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (Vector3)rb.velocity * lag;
        }
    }
    // ------
    
    // 충돌 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 적과 충돌했을 때 처리
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 자신의 플레이어인 경우만 처리
            if (photonView.IsMine)
            {
                // 게임 오버 처리
                Debug.Log("Game Over!");
                GameManager.Instance.GameOver();
                
                // --- TODO ---
                // 플레이어 제거 (네트워크 동기화)
                PhotonNetwork.Destroy(gameObject);
                // ------
            }
        }
    }
}