using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private TextMeshProUGUI errorText;

    string[] randomNames = new string[] {
        "Alex", "Ben", "Casey", "Dana", "Eli", "Finn", "Gray", "Hiro", 
        "Ivy", "Jay", "Kim", "Lee", "Max", "Nova", "Oren", "Piper", 
        "Quinn", "Ray", "Sam", "Tyler", "Uma", "Val", "Will", "Xan", 
        "Yara", "Zoe"
    };
    
    private void Start()
    {
        // 오브젝트 초기화
        connectionStatusText = GameObject.Find("connectionStatusText").GetComponent<TextMeshProUGUI>();
        startButton = GameObject.Find("startButton").GetComponent<Button>();
        roomNameField = GameObject.Find("roomNameField").GetComponent<TMP_InputField>();
        errorText = GameObject.Find("errorText").GetComponent<TextMeshProUGUI>();

        // UI 초기 설정
        connectionStatusText.text = "Connecting...";
        startButton.interactable = false;
        errorText.text = "";

        // 랜덤하게 이름 선택
        string randomName = randomNames[Random.Range(0, randomNames.Length)];
        // 중복 방지를 위해 숫자 추가
        PhotonNetwork.NickName = randomName + Random.Range(1, 10);
        
        // --- TODO ---
        // Photon 서버에 연결
        PhotonNetwork.ConnectUsingSettings();
        // ------
    }
    
    // --- TODO ---
    // 마스터 서버 연결 성공 시 호출
    public override void OnConnectedToMaster()
    {
        connectionStatusText.text = "Online: Connected to Master";
        startButton.interactable = true;
    }
    
    // 연결 끊김 시 호출
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionStatusText.text = "Offline: Disconnected";
        startButton.interactable = false;
    }
    // ------
    
    // 게임 시작 버튼 (방 생성 또는 참가)
    public void StartGame()
    {
        // 방 이름이 비어있으면 랜덤 이름 생성
        string roomName = string.IsNullOrEmpty(roomNameField.text) 
            ? "Room" + Random.Range(1000, 10000) 
            : roomNameField.text;
        
        // 방 옵션 설정
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };
        
        // --- TODO ---
        // 방 생성 또는 참가 시도
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        // ------
    }
    
    // --- TODO ---
    // 방 생성/참가 성공 시 호출
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        
        // 게임 씬으로 이동 (모든 플레이어 동시 이동)
        PhotonNetwork.LoadLevel("Game");
    }
    
    // 방 생성/참가 실패 시 호출
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed to join room: " + message;
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed to create room: " + message;
    }
    // ------
}