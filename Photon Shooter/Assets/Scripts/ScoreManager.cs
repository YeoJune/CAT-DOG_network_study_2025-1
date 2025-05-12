using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform scoreboardContent;
    
    // 점수 관리용 딕셔너리
    private Dictionary<string, int> playerScores = new Dictionary<string, int>();
    
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
        // 초기 점수 0으로 설정
        InitializeScores();
        
        // 점수 UI 업데이트
        UpdateScoreUI();
    }
    
    // 점수 초기화
    private void InitializeScores()
    {
        playerScores.Clear();
        
        // --- TODO ---
        // 현재 룸에 있는 모든 플레이어에게 점수 할당
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerScores[player.UserId] = 0;
            
            // 초기 점수 정보 Room Properties에 저장
            if (PhotonNetwork.IsMasterClient)
            {
                SetPlayerScore(player, 0);
            }
        }
        // ------
    }
    
    // 점수 증가
    public void AddScore(Player player)
    {
        // --- TODO ---
        // Room Properties를 통해 점수 관리
        if (PhotonNetwork.IsMasterClient)
        {
            int currentScore = GetPlayerScore(player);
            int newScore = currentScore + 10; // 기본 10점 증가
            
            // Room Properties에 업데이트된 점수 저장
            SetPlayerScore(player, newScore);
        }
        // ------
    }
    
    // --- TODO ---
    // Room Properties에 플레이어 점수 설정
    private void SetPlayerScore(Player player, int score)
    {
        // 기존 Room Properties 가져오기
        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        
        // 플레이어 점수 키 생성 (예: "Score_userId")
        string scoreKey = "Score_" + player.UserId;
        
        // Room Properties 업데이트
        roomProps[scoreKey] = score;
        
        // 변경된 Room Properties 적용
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    
    // Room Properties에서 플레이어 점수 가져오기
    private int GetPlayerScore(Player player)
    {
        // 플레이어 점수 키
        string scoreKey = "Score_" + player.UserId;
        
        // Room Properties에서 점수 가져오기
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(scoreKey))
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties[scoreKey];
        }
        
        return 0; // 기본값
    }
    
    // Room Properties 변경 콜백
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 점수 정보 업데이트
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string scoreKey = "Score_" + player.UserId;
            
            if (propertiesThatChanged.ContainsKey(scoreKey))
            {
                // 로컬 점수 데이터 업데이트
                playerScores[player.UserId] = (int)propertiesThatChanged[scoreKey];
            }
        }
        
        // UI 업데이트
        UpdateScoreUI();
    }
    // ------
    
    // 점수 UI 업데이트
    private void UpdateScoreUI()
    {
        // 기존 항목 제거
        foreach (Transform child in scoreboardContent)
        {
            Destroy(child.gameObject);
        }
        
        // 플레이어 점수 목록 생성
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // 점수 항목 생성
            GameObject scoreEntry = Instantiate(scoreEntryPrefab, scoreboardContent);
            
            // 텍스트 업데이트
            Text[] texts = scoreEntry.GetComponentsInChildren<Text>();
            texts[0].text = player.NickName; // 이름
            
            int score = 0;
            if (playerScores.ContainsKey(player.UserId))
            {
                score = playerScores[player.UserId];
            }
            texts[1].text = score.ToString(); // 점수
            
            // 로컬 플레이어 표시
            if (player.IsLocal)
            {
                texts[0].color = Color.yellow;
            }
        }
    }
}