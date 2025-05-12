using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text connectionStatusText;
    [SerializeField] private Button startButton;
    [SerializeField] private InputField roomNameField;
    [SerializeField] private Text errorText;
    
    private void Start()
    {
        // UI 초기 설정
        connectionStatusText.text = "연결 중...";
        startButton.interactable = false;
        errorText.text = "";
        
        // --- TODO ---
        // Photon 서버에 연결
        PhotonNetwork.ConnectUsingSettings();
        // ------
    }
    
    // --- TODO ---
    // 마스터 서버 연결 성공 시 호출
    public override void OnConnectedToMaster()
    {
        connectionStatusText.text = "온라인: 서버에 연결됨";
        startButton.interactable = true;
    }
    
    // 연결 끊김 시 호출
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionStatusText.text = "오프라인: 연결 끊김";
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
        Debug.Log("방에 입장했습니다: " + PhotonNetwork.CurrentRoom.Name);
        
        // 게임 씬으로 이동 (모든 플레이어 동시 이동)
        PhotonNetwork.LoadLevel("Game");
    }
    
    // 방 생성/참가 실패 시 호출
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorText.text = "방 입장 실패: " + message;
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "방 생성 실패: " + message;
    }
    // ------
}