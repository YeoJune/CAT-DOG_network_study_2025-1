using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI roomInfoText;
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private GameObject gameOverPanel;
    
    // 핑 업데이트 주기
    private float pingUpdateInterval = 1.0f;
    private float nextPingUpdateTime = 0f;
    
    private void Start()
    {
        // 방 정보 업데이트
        UpdateRoomInfo();
        
        // 스코어보드 기본 숨김
        scoreboardPanel.SetActive(false);
        
        // 게임오버 패널 기본 숨김
        gameOverPanel.SetActive(false);
    }
    
    private void Update()
    {
        // 탭 키를 누르면 스코어보드 표시
        if (Input.GetKey(KeyCode.Tab))
        {
            scoreboardPanel.SetActive(true);
        }
        else
        {
            scoreboardPanel.SetActive(false);
        }
        
        // 핑 주기적 업데이트
        if (Time.time >= nextPingUpdateTime)
        {
            UpdatePing();
            nextPingUpdateTime = Time.time + pingUpdateInterval;
        }
    }
    
    // 방 정보 업데이트
    private void UpdateRoomInfo()
    {
        if (PhotonNetwork.InRoom)
        {
            roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + 
                                "\nPlayer: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + 
                                PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        else
        {
            roomInfoText.text = "Not in a room";
        }
    }
    
    // 핑 업데이트
    private void UpdatePing()
    {
        pingText.text = "Ping: " + PhotonNetwork.GetPing() + "ms";
    }
    
    // 게임오버 패널 표시
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }
    
    // 로비로 돌아가기 버튼
    public void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
}