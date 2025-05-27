using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private Transform[] spawnPoints;
    
    private bool gameOver = false;

    public enum GameState { Waiting, Playing, GameOver }
    private GameState currentGameState = GameState.Waiting;

    
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

        // 게임 상태 초기화
        if (PhotonNetwork.IsMasterClient)
        {
            SetGameState(GameState.Playing);  // 게임 시작
        }
        else
        {
            currentGameState = GetGameState();  // 현재 상태 동기화
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
            // 게임 상태를 GameOver로 변경
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameState(GameState.GameOver);
            }
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

    // 🎯 게임 상태 설정 (MasterClient만 가능)
    private void SetGameState(GameState newState)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        roomProps["GameState"] = (int)newState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    // 🎯 게임 상태 조회
    private GameState GetGameState()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState"))
        {
            return (GameState)(int)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];
        }
        return GameState.Waiting;
    }

    // 🎯 Room Properties 변경 감지
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("GameState"))
        {
            currentGameState = (GameState)(int)propertiesThatChanged["GameState"];
            Debug.Log($"🎮 Game State Changed: {currentGameState}");
            // 상태별 처리 (필요시 구현)
            HandleGameStateChange(currentGameState);
        }
    }

    // 🎯 MasterClient 변경 시 상태 유지
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 새로운 MasterClient가 되면 현재 게임 상태 확인
            currentGameState = GetGameState();
            Debug.Log($"🔄 MasterClient switched. Current game state: {currentGameState}");
            // 필요시 상태 복구 로직 (예: 게임 재시작)
        }
    }
    // 🎯 상태별 처리 메서드 (확장 가능)
    private void HandleGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Waiting:
                Debug.Log("⏳ Game is waiting for players");
                break;
            case GameState.Playing:
                Debug.Log("🎮 Game is now playing");
                break;
            case GameState.GameOver:
                Debug.Log("🏁 Game is over");
                break;
        }
    }
}