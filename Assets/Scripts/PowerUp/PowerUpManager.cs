using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Manages power-ups, special tiles, and their effects in the match-3 game.
/// Handles power-up creation, activation, and visual effects.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private List<PowerUpData> availablePowerUps = new List<PowerUpData>();
    [SerializeField] private int maxStoredPowerUps = 5;
    [SerializeField] private float powerUpCooldown = 3f;
    
    [Header("Special Tile Creation")]
    [SerializeField] private int bombTileComboThreshold = 4;
    [SerializeField] private int rainbowTileComboThreshold = 5;
    [SerializeField] private float specialTileCreationChance = 0.3f;
    
    [Header("Effects")]
    [SerializeField] private GameObject bombEffectPrefab;
    [SerializeField] private GameObject rainbowEffectPrefab;
    [SerializeField] private Transform effectParent;
    [SerializeField] private AudioClip powerUpSound;
    [SerializeField] private AudioClip specialTileCreateSound;
    
    // Current state
    private Dictionary<PowerUpType, int> storedPowerUps = new Dictionary<PowerUpType, int>();
    private float lastPowerUpTime = 0f;
    private List<Vector2Int> pendingSpecialTiles = new List<Vector2Int>();
    
    // Events
    public System.Action<PowerUpType, int> OnPowerUpGained;
    public System.Action<PowerUpType> OnPowerUpUsed;
    public System.Action<Vector2Int, SpecialTileType> OnSpecialTileCreated;
    
    // References
    private GridManager gridManager;
    private GameManager gameManager;
    private AudioSource audioSource;
    
    // Properties
    public Dictionary<PowerUpType, int> StoredPowerUps => storedPowerUps;
    
    public enum PowerUpType
    {
        Bomb,               // Pizza Oven Bomb - destroys 3x3 area of ingredients
        IngredientSelector, // Magic Spatula - converts random tiles to needed ingredient
        TimeAdd,            // Extra Time - adds extra time to customer orders
        Rainbow,            // Rainbow Ingredient - matches with any ingredient type
        DoubleScore,        // Chef's Special - doubles points for next few moves
        ExtraMove           // Extra Energy - adds extra moves to current level
    }
    
    public enum SpecialTileType
    {
        Bomb,           // Pizza Oven Bomb - destroys 3x3 area
        Rainbow,        // Magic Ingredient - matches with any ingredient
        Lightning,      // Lightning Knife - destroys entire row/column
        Star            // Star Chef Special - destroys all tiles of one ingredient type
    }
    
    void Start()
    {
        InitializePowerUpManager();
    }
    
    /// <summary>
    /// Initialize power-up manager and find references
    /// </summary>
    private void InitializePowerUpManager()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize power-up storage
        InitializePowerUpStorage();
        
        // Subscribe to game events
        if (gridManager != null)
        {
            gridManager.OnTilesCleared += OnTilesCleared;
        }
    }
    
    /// <summary>
    /// Initialize power-up storage dictionary
    /// </summary>
    private void InitializePowerUpStorage()
    {
        storedPowerUps.Clear();
        foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
        {
            storedPowerUps[type] = 0;
        }
    }
    
    /// <summary>
    /// Handle tiles being cleared - chance to create special tiles or award power-ups
    /// </summary>
    private void OnTilesCleared(int tilesCleared)
    {
        // Check for special tile creation based on combo size
        if (tilesCleared >= bombTileComboThreshold)
        {
            CheckForSpecialTileCreation(tilesCleared);
        }
        
        // Award power-ups based on performance
        CheckForPowerUpReward(tilesCleared);
    }
    
    /// <summary>
    /// Check if special tiles should be created
    /// </summary>
    private void CheckForSpecialTileCreation(int tilesCleared)
    {
        if (Random.value > specialTileCreationChance) return;
        
        SpecialTileType specialType;
        
        if (tilesCleared >= rainbowTileComboThreshold)
        {
            specialType = SpecialTileType.Rainbow;
        }
        else if (tilesCleared >= bombTileComboThreshold)
        {
            specialType = SpecialTileType.Bomb;
        }
        else
        {
            return; // Not enough tiles for special creation
        }
        
        // Find a position to create the special tile (simplified - would be more complex in real implementation)
        Vector2Int position = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
        CreateSpecialTile(position, specialType);
    }
    
    /// <summary>
    /// Create a special tile at the specified position
    /// </summary>
    public void CreateSpecialTile(Vector2Int position, SpecialTileType type)
    {
        OnSpecialTileCreated?.Invoke(position, type);
        
        // Play creation sound
        if (audioSource != null && specialTileCreateSound != null)
        {
            audioSource.PlayOneShot(specialTileCreateSound);
        }
        
        Debug.Log($"Created {type} special tile at {position}");
    }
    
    /// <summary>
    /// Check for power-up rewards based on performance
    /// </summary>
    private void CheckForPowerUpReward(int tilesCleared)
    {
        // Award power-ups for good performance
        if (tilesCleared >= 6)
        {
            PowerUpType rewardType = GetRandomPowerUpType();
            GainPowerUp(rewardType, 1);
        }
    }
    
    /// <summary>
    /// Get a random power-up type
    /// </summary>
    private PowerUpType GetRandomPowerUpType()
    {
        PowerUpType[] types = (PowerUpType[])System.Enum.GetValues(typeof(PowerUpType));
        return types[Random.Range(0, types.Length)];
    }
    
    /// <summary>
    /// Gain a power-up
    /// </summary>
    public void GainPowerUp(PowerUpType type, int amount)
    {
        int currentAmount = storedPowerUps[type];
        int newAmount = Mathf.Min(currentAmount + amount, maxStoredPowerUps);
        storedPowerUps[type] = newAmount;
        
        OnPowerUpGained?.Invoke(type, newAmount);
        
        Debug.Log($"Gained {amount} {type} power-up(s). Total: {newAmount}");
    }
    
    /// <summary>
    /// Use a power-up if available
    /// </summary>
    public bool UsePowerUp(PowerUpType type)
    {
        if (storedPowerUps[type] <= 0) return false;
        if (Time.time - lastPowerUpTime < powerUpCooldown) return false;
        
        storedPowerUps[type]--;
        lastPowerUpTime = Time.time;
        
        ExecutePowerUpEffect(type);
        OnPowerUpUsed?.Invoke(type);
        
        return true;
    }
    
    /// <summary>
    /// Execute the effect of a specific power-up
    /// </summary>
    private void ExecutePowerUpEffect(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Bomb:
                ExecuteBombEffect();
                break;
            case PowerUpType.IngredientSelector:
                ExecuteIngredientSelectorEffect();
                break;
            case PowerUpType.TimeAdd:
                ExecuteTimeAddEffect();
                break;
            case PowerUpType.Rainbow:
                ExecuteRainbowEffect();
                break;
            case PowerUpType.DoubleScore:
                ExecuteDoubleScoreEffect();
                break;
            case PowerUpType.ExtraMove:
                ExecuteExtraMoveEffect();
                break;
        }
        
        // Play power-up sound
        if (audioSource != null && powerUpSound != null)
        {
            audioSource.PlayOneShot(powerUpSound);
        }
    }
    
    /// <summary>
    /// Execute pizza oven bomb power-up effect - destroys 3x3 area of ingredients
    /// </summary>
    private void ExecuteBombEffect()
    {
        // Create pizza oven explosion effect at center of screen
        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 10));
        
        if (bombEffectPrefab != null && effectParent != null)
        {
            GameObject effect = Instantiate(bombEffectPrefab, screenCenter, Quaternion.identity, effectParent);
            Destroy(effect, 2f);
        }
        
        // In a real implementation, this would destroy pizza ingredients in a 3x3 area around a selected position
        Debug.Log("Pizza Oven Bomb used! Clearing ingredients in 3x3 area.");
    }
    
    /// <summary>
    /// Execute magic spatula power-up effect - converts random tiles to needed pizza ingredients
    /// </summary>
    private void ExecuteIngredientSelectorEffect()
    {
        PizzaOrderManager pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
        {
            // Get required ingredients for current pizza order
            List<Tile.TileType> requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
            
            if (requiredIngredients.Count > 0)
            {
                // In a real implementation, this would convert 3-5 random tiles on the grid
                // to the most needed ingredient type
                Tile.TileType neededIngredient = requiredIngredients[Random.Range(0, requiredIngredients.Count)];
                Debug.Log($"Magic Spatula used! Converting tiles to {neededIngredient} for current pizza order.");
            }
            else
            {
                Debug.Log("Magic Spatula used! No specific ingredients needed right now.");
            }
        }
        else
        {
            // Fallback behavior
            Debug.Log("Magic Spatula used! Converting random tiles to helpful ingredients.");
        }
    }
    
    /// <summary>
    /// Execute time add power-up effect - adds extra time to current pizza order
    /// </summary>
    private void ExecuteTimeAddEffect()
    {
        PizzaOrderManager pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
        {
            // Add extra time to current pizza order
            pizzaOrderManager.AddExtraTime(20f); // Add 20 seconds
            Debug.Log("Extra Time power-up used! Added 20 seconds to current pizza order.");
        }
        else
        {
            // Fallback to old customer manager system if pizza system not available
            CustomerManager customerManager = FindFirstObjectByType<CustomerManager>();
            if (customerManager != null)
            {
                // Add time to all active orders
                foreach (var order in customerManager.ActiveOrders)
                {
                    order.remainingTime += 15f; // Add 15 seconds
                }
            }
            Debug.Log("Time Add power-up used! Extended order time.");
        }
    }
    
    /// <summary>
    /// Execute rainbow power-up effect
    /// </summary>
    private void ExecuteRainbowEffect()
    {
        if (rainbowEffectPrefab != null && effectParent != null)
        {
            Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 10));
            GameObject effect = Instantiate(rainbowEffectPrefab, screenCenter, Quaternion.identity, effectParent);
            Destroy(effect, 2f);
        }
        
        // In a real implementation, this would create a rainbow tile that matches with anything
        Debug.Log("Rainbow power-up used! Created rainbow tile that matches with any ingredient.");
    }
    
    /// <summary>
    /// Execute double score power-up effect
    /// </summary>
    private void ExecuteDoubleScoreEffect()
    {
        // In a real implementation, this would activate a temporary score multiplier
        Debug.Log("Double Score power-up used! Next 3 matches will give double points.");
    }
    
    /// <summary>
    /// Execute extra move power-up effect
    /// </summary>
    private void ExecuteExtraMoveEffect()
    {
        if (gameManager != null)
        {
            // Add extra moves (this would need to be implemented in GameManager)
            Debug.Log("Extra Move power-up used! Added 3 extra moves.");
        }
    }
    
    /// <summary>
    /// Get the count of a specific power-up type
    /// </summary>
    public int GetPowerUpCount(PowerUpType type)
    {
        return storedPowerUps.ContainsKey(type) ? storedPowerUps[type] : 0;
    }
    
    /// <summary>
    /// Check if a power-up can be used
    /// </summary>
    public bool CanUsePowerUp(PowerUpType type)
    {
        return storedPowerUps[type] > 0 && (Time.time - lastPowerUpTime >= powerUpCooldown);
    }
    
    /// <summary>
    /// Get remaining cooldown time
    /// </summary>
    public float GetRemainingCooldown()
    {
        return Mathf.Max(0f, powerUpCooldown - (Time.time - lastPowerUpTime));
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gridManager != null)
        {
            gridManager.OnTilesCleared -= OnTilesCleared;
        }
    }
}

/// <summary>
/// Data structure for power-up configuration
/// </summary>
[System.Serializable]
public class PowerUpData
{
    public PowerUpManager.PowerUpType type;
    public string name;
    public string description;
    public Sprite icon;
    public int maxStorage = 5;
    public float cooldown = 3f;
}