using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private Transform[] spawnPoints;
    
    private bool gameOver = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
        else
        {
            // 연결이 되어있지 않은 경우 로비로 돌아감
            SceneManager.LoadScene("Lobby");
        }
    }
    
    private void SpawnPlayer()
    {
        // 랜덤 스폰 포인트 선택
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];
        
        // --- TODO ---
        // Photon을 통해 플레이어 생성
        // Resources 폴더 내의 Player 프리팹 사용
        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
        // ------
    }
    
    public void GameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            Invoke("ReturnToLobby", 3f);
        }
    }
    
    private void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    // --- TODO ---
    // 방을 떠난 후 호출
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
    // ------
}