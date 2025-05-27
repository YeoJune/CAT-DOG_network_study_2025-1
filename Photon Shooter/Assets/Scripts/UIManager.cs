using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI roomInfoText;
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI masterClientText;
    [SerializeField] private TextMeshProUGUI networkStatsText;
    
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

            // 🎯 MasterClient 정보 표시
            if (masterClientText != null)
            {
                masterClientText.text = "Master: " + PhotonNetwork.MasterClient.NickName;
            }
        }
        else
        {
            roomInfoText.text = "Not in a room";
            if (masterClientText != null)
            {
                masterClientText.text = "";
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateRoomInfo();
        Debug.Log("🔄 New MasterClient: " + newMasterClient.NickName);
    }
    
    // 🎯 UpdatePing 메서드 확장
    private void UpdatePing()
    {
        // 기존 핑 표시 (간단한 버전)
        if (pingText != null)
        {
            pingText.text = "Ping: " + PhotonNetwork.GetPing() + "ms";
        }
        // 🎯 상세 네트워크 정보 표시
        if (networkStatsText != null && PhotonNetwork.InRoom)
        {
            string stats = $"Network Status\n";
            stats += $"Ping: {PhotonNetwork.GetPing()}ms\n";
            stats += $"Send Rate: {PhotonNetwork.SendRate}/s\n";
            stats += $"Region: {PhotonNetwork.CloudRegion}\n";
            stats += $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}\n";
            stats += $"Connected: {(PhotonNetwork.IsConnected ? "V" : "X")}";
            networkStatsText.text = stats;
            // 🎨 핑에 따른 색상 변경
            if (PhotonNetwork.GetPing() < 50)
                networkStatsText.color = Color.green;
            else if (PhotonNetwork.GetPing() < 150)
                networkStatsText.color = Color.yellow;
            else
                networkStatsText.color = Color.red;
        }
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