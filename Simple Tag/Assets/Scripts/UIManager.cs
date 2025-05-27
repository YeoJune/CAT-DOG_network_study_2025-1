using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Elements")]
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerCountText;  // 플레이어 수 표시 추가
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    
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
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            UpdateUI();
        }
    }
    
    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;
        
        var humanPlayer = GameManager.Instance.GetHumanPlayer();
        if (humanPlayer != null)
        {
            // Update role text
            if (roleText != null)
            {
                string roleString = humanPlayer.CurrentRole == PlayerRole.Tagger ? "TAGGER" : "RUNNER";
                roleText.text = $"You are: {roleString}";
                roleText.color = humanPlayer.CurrentRole == PlayerRole.Tagger ? Color.red : Color.blue;
            }
            
            // Update score text
            if (scoreText != null)
            {
                scoreText.text = $"Score: {humanPlayer.Score}";
            }
        }
        
        // Update timer
        if (timerText != null)
        {
            float remainingTime = GameManager.Instance.GetRemainingTime();
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        
        // Update player count
        if (playerCountText != null)
        {
            var allPlayers = GameManager.Instance.GetAllPlayers();
            var taggerCount = GameManager.Instance.GetPlayersWithRole(PlayerRole.Tagger).Count;
            var runnerCount = GameManager.Instance.GetPlayersWithRole(PlayerRole.Runner).Count;
            playerCountText.text = $"Players: {allPlayers.Count} (Tagger: {taggerCount}, Runners: {runnerCount})";
        }
    }
    
    public void ShowGameOver(PlayerController winner, int winnerScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null && GameManager.Instance != null)
            {
                var humanPlayer = GameManager.Instance.GetHumanPlayer();
                bool playerWon = (winner == humanPlayer);
                
                string message;
                if (playerWon)
                {
                    message = $"🎉 You Won! 🎉\nFinal Score: {winnerScore}";
                }
                else
                {
                    string winnerName = GameManager.Instance.GetPlayerName(winner);
                    int playerScore = humanPlayer != null ? humanPlayer.Score : 0;
                    message = $"Game Over!\n{winnerName} Won with {winnerScore} points\nYour Score: {playerScore}";
                }
                
                gameOverText.text = message;
            }
        }
    }
    
    private void RestartGame()
    {
        var sceneManager = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneManager.buildIndex);
    }
}