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
        // 스폰 포인트 초기화
        GameObject SpawnPointParent = GameObject.Find("SpawnPoints");
        spawnPoints = new Transform[SpawnPointParent.transform.childCount];
        for (int i = 0; i < SpawnPointParent.transform.childCount; i++)
        {
            spawnPoints[i] = SpawnPointParent.transform.GetChild(i);
        }

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
        
        // Photon을 통해 플레이어 생성
        // Resources 폴더 내의 Player 프리팹 사용
        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
    }
    
    public void GameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            GameObject.Find("UIManager").GetComponent<UIManager>().ShowGameOver();
        }
    }
    
    public void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    // 방을 떠난 후 호출
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}