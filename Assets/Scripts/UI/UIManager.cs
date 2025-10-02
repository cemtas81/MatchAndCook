using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the game UI including score display, moves counter, and game end screens.
/// Optimized for mobile touch interaction with clear, large UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Slider progressBar;
    
    [Header("Game End UI")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI gameEndTitle;
    [SerializeField] private TextMeshProUGUI gameEndScore;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    
    [Header("Menu UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    
    [Header("Settings")]
    [SerializeField] private Color winColor = Color.green;
    [SerializeField] private Color loseColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    
    // References
    private GameManager gameManager;
    private PizzaOrderManager pizzaOrderManager;
    
    void Start()
    {
        InitializeUI();
        SetupButtons();
        SubscribeToEvents();
    }
    
    /// <summary>
    /// Initialize UI elements and find references
    /// </summary>
    private void InitializeUI()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        
        // Hide panels initially
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
            
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        // Initialize progress bar
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            
            // E�er GameManager bulunduysa, max de�eri hedef skora ayarla
            if (gameManager != null)
            {
                progressBar.maxValue = gameManager.TargetScore;
            }
            else
            {
                // GameManager bulunamad�ysa varsay�lan bir de�er kullan
                progressBar.maxValue = 1000f; 
            }
            
            progressBar.value = 0f;
        }
        
        // Initialize money display
        if (pizzaOrderManager != null)
        {
            UpdateMoney(pizzaOrderManager.TotalMoney);
        }
    }
    
    /// <summary>
    /// Set up button event listeners
    /// </summary>
    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => {
                if (gameManager != null)
                    gameManager.RestartGame();
            });
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(() => {
                if (gameManager != null)
                    gameManager.GoToMainMenu();
            });
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() => {
                if (gameManager != null)
                    gameManager.PauseGame();
                ShowPausePanel(true);
            });
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => {
                if (gameManager != null)
                    gameManager.ResumeGame();
                ShowPausePanel(false);
            });
        }
    }
    
    /// <summary>
    /// Subscribe to game events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnMoneyChanged += UpdateMoney;
        }
    }
    
    /// <summary>
    /// Update the score display
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {FormatNumber(score)}";
        }
    }
    
    /// <summary>
    /// Update the moves counter
    /// </summary>
    public void UpdateMoves(int moves)
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {moves}";
            
            // Change color when moves are running low
            if (moves <= 5)
            {
                movesText.color = loseColor;
            }
            else if (moves <= 10)
            {
                movesText.color = Color.yellow;
            }
            else
            {
                movesText.color = normalColor;
            }
        }
    }
    
    /// <summary>
    /// Update the target score display
    /// </summary>
    public void UpdateTarget(int target)
    {
        if (targetText != null)
        {
            targetText.text = $"Target: {FormatNumber(target)}";
        }
    }
    
    /// <summary>
    /// Update the money display
    /// </summary>
    public void UpdateMoney(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${FormatNumber(money)}";
        }
    }
    
    /// <summary>
    /// Update the progress bar
    /// </summary>
    public void UpdateProgress(int currentScore, int targetScore)
    {
        if (progressBar != null)
        {
            // Slider'�n min ve max de�erlerini ayarla
            progressBar.minValue = 0;
            progressBar.maxValue = targetScore;
            
            // Slider de�erini do�rudan currentScore olarak ayarla
            progressBar.value = currentScore;
            
            // Renk de�i�imi i�in normalize edilmi� ilerleme de�eri hesapla
            float normalizedProgress = targetScore > 0 ? (float)currentScore / targetScore : 0;
            
            // Change color based on progress
            Image fillImage = progressBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (normalizedProgress >= 1f)
                {
                    fillImage.color = winColor;
                }
                else if (normalizedProgress >= 0.75f)
                {
                    fillImage.color = Color.Lerp(Color.yellow, winColor, (normalizedProgress - 0.75f) * 4f);
                }
                else if (normalizedProgress >= 0.5f)
                {
                    fillImage.color = Color.yellow;
                }
                else
                {
                    fillImage.color = Color.yellow;
                }
            }
        }
        
        // Update target display
        UpdateTarget(targetScore);
    }
    
    /// <summary>
    /// Show game end screen
    /// </summary>
    public void ShowGameEnd(bool won, int finalScore)
    {
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
            
            if (gameEndTitle != null)
            {
                gameEndTitle.text = won ? "You Win!" : "Game Over";
                gameEndTitle.color = won ? winColor : loseColor;
            }
            
            if (gameEndScore != null)
            {
                string scoreMessage = won ? 
                    $"Congratulations!\nFinal Score: {FormatNumber(finalScore)}" :
                    $"Better luck next time!\nFinal Score: {FormatNumber(finalScore)}";
                    
                gameEndScore.text = scoreMessage;
            }
        }
    }
    
    /// <summary>
    /// Show or hide pause panel
    /// </summary>
    public void ShowPausePanel(bool show)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(show);
        }
    }
    
    /// <summary>
    /// Format numbers for display (1000 -> 1K, etc.)
    /// </summary>
    private string FormatNumber(int number)
    {
        if (number >= 1000000)
        {
            return $"{(number / 1000000f):F1}M";
        }
        else if (number >= 1000)
        {
            return $"{(number / 1000f):F1}K";
        }
        else
        {
            return number.ToString();
        }
    }
    
    /// <summary>
    /// Show feedback message (for combos, achievements, etc.)
    /// </summary>
    public void ShowFeedback(string message, Vector3 worldPosition, Color color)
    {
        // This could be expanded to show floating text or other feedback
        Debug.Log($"Feedback: {message} at {worldPosition}");
    }
    
    /// <summary>
    /// Animate UI element (for juice and polish)
    /// </summary>
    public void AnimateElement(Transform element, float scale = 1.2f, float duration = 0.2f)
    {
        if (element != null)
        {
            StartCoroutine(ScaleAnimation(element, scale, duration));
        }
    }
    
    /// <summary>
    /// Simple scale animation coroutine
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimation(Transform element, float targetScale, float duration)
    {
        Vector3 originalScale = element.localScale;
        Vector3 targetScaleVector = originalScale * targetScale;
        
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / (duration * 0.5f);
            element.localScale = Vector3.Lerp(originalScale, targetScaleVector, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale back down
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / (duration * 0.5f);
            element.localScale = Vector3.Lerp(targetScaleVector, originalScale, progress);
            yield return null;
        }
        
        element.localScale = originalScale;
    }
    
    /// <summary>
    /// Update all UI elements at once (useful for initialization)
    /// </summary>
    public void UpdateAllUI(int score, int moves, int target)
    {
        UpdateScore(score);
        UpdateMoves(moves);
        UpdateProgress(score, target);
    }
    
    /// <summary>
    /// Enable or disable UI interaction
    /// </summary>
    public void SetUIEnabled(bool enabled)
    {
        // This could disable/enable various UI buttons when needed
        if (pauseButton != null)
            pauseButton.interactable = enabled;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnMoneyChanged -= UpdateMoney;
        }
    }
}