using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager that controls game state, win/lose conditions, and overall flow.
/// Implements simple target score system with limited moves for mobile-friendly sessions.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int targetScore = 1000;
    [SerializeField] private int movesLimit = 30;
    [SerializeField] private int pointsPerTile = 10;
    [SerializeField] private int comboMultiplier = 2;
    
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PizzaOrderManager pizzaOrderManager; // Pizza order system integration
    
    // Game state
    private int currentScore = 0;
    private int movesRemaining;
    private GameState gameState = GameState.Playing;
    private int currentCombo = 0;
    
    public enum GameState
    {
        Playing,
        Won,
        Lost,
        Paused
    }
    
    // Properties
    public int CurrentScore => currentScore;
    public int MovesRemaining => movesRemaining;
    public int TargetScore => targetScore;
    public GameState State => gameState;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnMovesChanged;
    public System.Action<GameState> OnGameStateChanged;
    
    void Start()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// Initialize the game with starting values and pizza order system
    /// </summary>
    private void InitializeGame()
    {
        movesRemaining = movesLimit;
        currentScore = 0;
        gameState = GameState.Playing;
        currentCombo = 0;
        
        // Find pizza order manager if not assigned
        if (pizzaOrderManager == null)
        {
            pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        }
        
        // Subscribe to grid events
        if (gridManager != null)
        {
            gridManager.OnTilesCleared += OnTilesCleared;
            gridManager.OnGridRefilled += OnGridRefilled;
        }
        
        // Subscribe to pizza order events
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnOrderCompleted += OnPizzaOrderCompleted;
            pizzaOrderManager.OnLevelChanged += OnLevelChanged;
        }
        
        // Update UI
        UpdateUI();
        
        Debug.Log($"Pizza Match-3 game started! Level: {pizzaOrderManager?.CurrentLevel ?? 1}");
    }
    
    /// <summary>
    /// Handle tiles being cleared and award points
    /// </summary>
    private void OnTilesCleared(int tilesCleared)
    {
        if (gameState != GameState.Playing) return;
        
        // Calculate score with combo multiplier
        int basePoints = tilesCleared * pointsPerTile;
        int bonusPoints = currentCombo > 0 ? basePoints * comboMultiplier * currentCombo : 0;
        int totalPoints = basePoints + bonusPoints;
        
        AddScore(totalPoints);
        
        // Ka� tane karonun temizlendi�ini kaydet
        if (scoreManager != null)
        {
            scoreManager.RecordTilesCleared(tilesCleared);
        }
        
        // Combo artt�r ve kaydet
        currentCombo++;
        
        // ScoreManager'a yeni combo bilgisini g�nder
        if (scoreManager != null)
        {
            scoreManager.RecordCombo(currentCombo);
        }
        
        Debug.Log($"Cleared {tilesCleared} tiles! +{totalPoints} points (Combo x{currentCombo})");
    }
    
    /// <summary>
    /// Handle grid refill completion
    /// </summary>
    private void OnGridRefilled()
    {
        if (gameState != GameState.Playing) return;
        
        // Reset combo after refill
        currentCombo = 0;
        
        // Consume a move after the refill is complete
        movesRemaining--;
        OnMovesChanged?.Invoke(movesRemaining);
        
        // Check win/lose conditions
        CheckGameEnd();
        
        UpdateUI();
    }
    
    /// <summary>
    /// Handle pizza order completion and level progression
    /// </summary>
    private void OnPizzaOrderCompleted(PizzaOrder order, bool success)
    {
        if (success)
        {
            // Award bonus points for completing pizza order
            int orderReward = order.CalculateReward(Time.time);
            AddScore(orderReward);
            
            Debug.Log($"Pizza order completed! {order.customerName} is happy. Bonus: {orderReward} points");
        }
        else
        {
            Debug.Log($"Pizza order failed! {order.customerName} is disappointed.");
        }
    }
    
    /// <summary>
    /// Handle level progression from pizza order system
    /// </summary>
    private void OnLevelChanged(int newLevel)
    {
        Debug.Log($"Advanced to level {newLevel}!");
        
        // Could add level-specific bonuses or difficulty adjustments here
        // For now, just update UI if needed
        UpdateUI();
    }
    
    /// <summary>
    /// Add points to the current score
    /// </summary>
    private void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Update score manager if available
        if (scoreManager != null)
        {
            scoreManager.UpdateScore(currentScore);
        }
    }
    
    /// <summary>
    /// Check if the game should end (win or lose conditions)
    /// Modified for pizza order system - game continues as long as player completes orders
    /// </summary>
    private void CheckGameEnd()
    {
        // In pizza mode, the game doesn't end based on score or moves
        // Instead, it continues level by level with pizza orders
        // Game only ends if player runs out of time on too many orders
        
        // For now, keep the traditional win condition as a backup
        if (currentScore >= targetScore)
        {
            // Player reached high score milestone
            gameState = GameState.Won;
            OnGameStateChanged?.Invoke(gameState);
            
            Debug.Log("Congratulations! You've reached the target score!");
            
            if (uiManager != null)
                uiManager.ShowGameEnd(true, currentScore);
        }
        else if (movesRemaining <= 0)
        {
            // No more moves - restart the level or show game over
            gameState = GameState.Lost;
            OnGameStateChanged?.Invoke(gameState);
            
            Debug.Log("No more moves! Game over.");
            
            if (uiManager != null)
                uiManager.ShowGameEnd(false, currentScore);
        }
    }
    
    /// <summary>
    /// Update UI elements with current game state
    /// </summary>
    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
            uiManager.UpdateMoves(movesRemaining);
            uiManager.UpdateProgress(currentScore, targetScore);
        }
    }
    
    /// <summary>
    /// Restart the current game
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Go to main menu
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu...");
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (gameState == GameState.Playing)
        {
            gameState = GameState.Paused;
            OnGameStateChanged?.Invoke(gameState);
            
            //if (gridManager != null)
            //    gridManager.SetInputEnabled(false);
                
            Time.timeScale = 0f;
            Debug.Log("Game paused");
        }
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (gameState == GameState.Paused)
        {
            gameState = GameState.Playing;
            OnGameStateChanged?.Invoke(gameState);
            
            //if (gridManager != null)
            //    gridManager.SetInputEnabled(true);
                
            Time.timeScale = 1f;
            Debug.Log("Game resumed");
        }
    }
    
    /// <summary>
    /// Get current game progress as percentage
    /// </summary>
    public float GetProgress()
    {
        return (float)currentScore / targetScore;
    }
    
    /// <summary>
    /// Check if the game is active (not won/lost)
    /// </summary>
    public bool IsGameActive()
    {
        return gameState == GameState.Playing;
    }
    
    /// <summary>
    /// Get a summary of the current game state
    /// </summary>
    public string GetGameSummary()
    {
        return $"Score: {currentScore}/{targetScore} | Moves: {movesRemaining} | State: {gameState}";
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gridManager != null)
        {
            gridManager.OnTilesCleared -= OnTilesCleared;
            gridManager.OnGridRefilled -= OnGridRefilled;
        }
        
        // Unsubscribe from pizza order events
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnOrderCompleted -= OnPizzaOrderCompleted;
            pizzaOrderManager.OnLevelChanged -= OnLevelChanged;
        }
        
        // Reset time scale
        Time.timeScale = 1f;
    }
}