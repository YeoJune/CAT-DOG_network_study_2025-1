using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public enum PlayerRole { Runner, Tagger }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public float gameDuration = 60f;
    public float scoreInterval = 2f;
    public float tagCooldown = 2f;
    public float invulnerabilityTime = 1f;  // 태그된 후 무적 시간
    
    [Header("References")]
    public Transform[] spawnPoints;
    public GameObject aiPlayerPrefab;
    public int aiPlayerCount = 3;
    
    [Header("Play Area")]
    public float playAreaWidth = 18f;
    public float playAreaHeight = 10f;
    public Vector3 playAreaCenter = Vector3.zero;
    
    // Game State
    private List<PlayerController> allPlayers = new List<PlayerController>();
    private Dictionary<PlayerController, int> playerScores = new Dictionary<PlayerController, int>();
    private Dictionary<PlayerController, float> lastTaggedTime = new Dictionary<PlayerController, float>();
    private float gameTime;
    private float lastScoreTime;
    private bool gameActive = false;
    private float lastTagTime = -10f;
    
    // Events
    public System.Action<PlayerController, PlayerController> OnTagEvent;  // tagger, target
    public System.Action<PlayerController> OnRoleChanged;
    
    // Play Area Bounds
    public float MinX => playAreaCenter.x - playAreaWidth / 2;
    public float MaxX => playAreaCenter.x + playAreaWidth / 2;
    public float MinY => playAreaCenter.y - playAreaHeight / 2;
    public float MaxY => playAreaCenter.y + playAreaHeight / 2;
    
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
        InitializeGame();
    }
    
    private void Update()
    {
        if (!gameActive) return;
        
        UpdateGameTime();
        UpdateScores();
        ValidateGameState();
    }
    
    private void InitializeGame()
    {
        // Clear existing data
        allPlayers.Clear();
        playerScores.Clear();
        lastTaggedTime.Clear();
        
        // Find human player
        PlayerController humanPlayer = FindObjectOfType<PlayerController>();
        if (humanPlayer != null && humanPlayer.GetComponent<AIPlayer>() == null)
        {
            RegisterPlayer(humanPlayer);
        }
        
        // Spawn AI players
        for (int i = 0; i < aiPlayerCount && i < spawnPoints.Length; i++)
        {
            Vector3 spawnPos = spawnPoints[i].position;
            GameObject aiObject = Instantiate(aiPlayerPrefab, spawnPos, Quaternion.identity);
            AIPlayer aiPlayer = aiObject.GetComponent<AIPlayer>();
            
            if (aiPlayer != null)
            {
                RegisterPlayer(aiPlayer);
            }
        }
        
        // Set initial roles
        AssignInitialRoles();
        
        // Initialize game timer
        gameTime = gameDuration;
        lastScoreTime = Time.time;
        gameActive = true;
        
        Debug.Log($"Game started with {allPlayers.Count} players");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }
    }
    
    private void RegisterPlayer(PlayerController player)
    {
        if (player != null && !allPlayers.Contains(player))
        {
            allPlayers.Add(player);
            playerScores[player] = 0;
            lastTaggedTime[player] = -10f;  // 충분히 과거 시간
            
            // 플레이어에게 GameManager 참조 전달
            player.SetGameManager(this);
        }
    }
    
    private void AssignInitialRoles()
    {
        // Everyone is runner first
        foreach (var player in allPlayers)
        {
            if (player != null)
            {
                player.SetRole(PlayerRole.Runner);
            }
        }
        
        // Pick random tagger
        if (allPlayers.Count > 0)
        {
            var validPlayers = allPlayers.Where(p => p != null).ToList();
            if (validPlayers.Count > 0)
            {
                int randomIndex = Random.Range(0, validPlayers.Count);
                validPlayers[randomIndex].SetRole(PlayerRole.Tagger);
            }
        }
    }
    
    public bool CanPlayerBeTagged(PlayerController player)
    {
        if (player == null) return false;
        if (!allPlayers.Contains(player)) return false;
        if (player.CurrentRole == PlayerRole.Tagger) return false;
        
        // 무적 시간 체크
        if (lastTaggedTime.ContainsKey(player))
        {
            return Time.time - lastTaggedTime[player] >= invulnerabilityTime;
        }
        
        return true;
    }
    
    public bool CanPlayerTag(PlayerController player)
    {
        if (player == null) return false;
        if (!allPlayers.Contains(player)) return false;
        if (player.CurrentRole != PlayerRole.Tagger) return false;
        
        // 전역 태그 쿨다운 체크
        return Time.time - lastTagTime >= tagCooldown;
    }
    
    public void OnPlayerTagged(PlayerController tagger, PlayerController target)
    {
        if (!gameActive) return;
        if (!CanPlayerTag(tagger)) return;
        if (!CanPlayerBeTagged(target)) return;
        
        Debug.Log($"{GetPlayerName(tagger)} tagged {GetPlayerName(target)}!");
        
        // 태그 시간 기록
        lastTagTime = Time.time;
        lastTaggedTime[target] = Time.time;
        
        // 역할 변경
        SwitchRoles(tagger, target);
        
        // 이벤트 발생
        OnTagEvent?.Invoke(tagger, target);
        
        // AI에게 태그 이벤트 알림
        NotifyAIPlayersOfTag(tagger, target);
    }
    
    private void SwitchRoles(PlayerController oldTagger, PlayerController newTagger)
    {
        oldTagger.SetRole(PlayerRole.Runner);
        newTagger.SetRole(PlayerRole.Tagger);
        
        OnRoleChanged?.Invoke(oldTagger);
        OnRoleChanged?.Invoke(newTagger);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }
    }
    
    private void NotifyAIPlayersOfTag(PlayerController tagger, PlayerController target)
    {
        foreach (var player in allPlayers)
        {
            AIPlayer aiPlayer = player as AIPlayer;
            if (aiPlayer != null)
            {
                aiPlayer.OnTagEvent(tagger, target);
            }
        }
    }
    
    private void UpdateGameTime()
    {
        gameTime -= Time.deltaTime;
        
        if (gameTime <= 0)
        {
            EndGame();
        }
    }
    
    private void UpdateScores()
    {
        if (Time.time - lastScoreTime >= scoreInterval)
        {
            // Give points to all runners
            foreach (var player in allPlayers.Where(p => p != null))
            {
                if (player.CurrentRole == PlayerRole.Runner)
                {
                    playerScores[player]++;
                    player.UpdateScore(playerScores[player]);
                }
            }
            
            lastScoreTime = Time.time;
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }
        }
    }
    
    private void ValidateGameState()
    {
        // 유효한 플레이어 목록 갱신
        allPlayers.RemoveAll(p => p == null);
        
        // 태거가 없으면 새로 지정
        var currentTagger = allPlayers.FirstOrDefault(p => p.CurrentRole == PlayerRole.Tagger);
        if (currentTagger == null && allPlayers.Count > 0)
        {
            var validPlayers = allPlayers.Where(p => p != null).ToList();
            if (validPlayers.Count > 0)
            {
                int randomIndex = Random.Range(0, validPlayers.Count);
                validPlayers[randomIndex].SetRole(PlayerRole.Tagger);
                Debug.Log($"New tagger assigned: {GetPlayerName(validPlayers[randomIndex])}");
            }
        }
        
        // 게임 종료 조건 (플레이어가 1명 이하)
        if (allPlayers.Count <= 1)
        {
            EndGame();
        }
    }
    
    private void EndGame()
    {
        gameActive = false;
        
        if (playerScores.Count > 0)
        {
            var validScores = playerScores.Where(kvp => kvp.Key != null);
            if (validScores.Any())
            {
                var winner = validScores.OrderByDescending(kvp => kvp.Value).First();
                Debug.Log($"Game Over! Winner: {GetPlayerName(winner.Key)} with {winner.Value} points");
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowGameOver(winner.Key, winner.Value);
                }
            }
        }
    }
    
    // Utility Methods
    public Vector3 ClampToPlayArea(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, MinX, MaxX),
            Mathf.Clamp(position.y, MinY, MaxY),
            position.z
        );
    }
    
    public bool IsInPlayArea(Vector3 position)
    {
        return position.x >= MinX && position.x <= MaxX &&
               position.y >= MinY && position.y <= MaxY;
    }
    
    public List<PlayerController> GetAllPlayers()
    {
        return allPlayers.Where(p => p != null).ToList();
    }
    
    public List<PlayerController> GetPlayersWithRole(PlayerRole role)
    {
        return allPlayers.Where(p => p != null && p.CurrentRole == role).ToList();
    }
    
    public PlayerController GetClosestPlayer(Vector3 position, PlayerRole role, PlayerController excludePlayer = null)
    {
        return allPlayers
            .Where(p => p != null && p != excludePlayer && p.CurrentRole == role)
            .OrderBy(p => Vector3.Distance(position, p.transform.position))
            .FirstOrDefault();
    }
    
    public string GetPlayerName(PlayerController player)
    {
        if (player == null) return "Unknown";
        
        if (player.GetComponent<AIPlayer>() != null)
        {
            return $"AI_{player.GetInstanceID()}";
        }
        return "Human";
    }
    
    // Getters for UI
    public float GetRemainingTime() => Mathf.Max(0, gameTime);
    public PlayerController GetHumanPlayer()
    {
        return allPlayers.FirstOrDefault(p => p != null && p.GetComponent<AIPlayer>() == null);
    }
    public Dictionary<PlayerController, int> GetScores() => playerScores;
    public bool IsGameActive() => gameActive;
    
    // 플레이 영역 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(playAreaCenter, new Vector3(playAreaWidth, playAreaHeight, 0));
    }
}