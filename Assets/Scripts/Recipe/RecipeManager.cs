using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Manages recipe progression, ingredient collection, and cooking animations.
/// Integrates with the match-3 system to track ingredient collection.
/// </summary>
public class RecipeManager : MonoBehaviour
{
    [Header("Recipe Settings")]
    [SerializeField] private List<Recipe> availableRecipes = new List<Recipe>();
    [SerializeField] private Recipe currentRecipe;
    [SerializeField] private int maxActiveRecipes = 1;
    
    [Header("Animation Settings")]
    [SerializeField] private GameObject cookingEffectPrefab;
    [SerializeField] private Transform cookingEffectParent;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip recipeCompleteSound;
    
    // Current state
    private Dictionary<Tile.TileType, int> collectedIngredients = new Dictionary<Tile.TileType, int>();
    private bool isRecipeActive = false;
    
    // Events
    public System.Action<Recipe> OnRecipeStarted;
    public System.Action<Recipe, int> OnRecipeCompleted;
    public System.Action<Tile.TileType, int> OnIngredientCollected;
    public System.Action<Recipe, float> OnRecipeProgressChanged;
    
    // References
    private GameManager gameManager;
    private UIManager uiManager;
    private AudioSource audioSource;
    
    // Properties
    public Recipe CurrentRecipe => currentRecipe;
    public Dictionary<Tile.TileType, int> CollectedIngredients => collectedIngredients;
    public bool IsRecipeActive => isRecipeActive;
    
    void Start()
    {
        InitializeRecipeManager();
    }
    
    /// <summary>
    /// Initialize the recipe manager and find references
    /// </summary>
    private void InitializeRecipeManager()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Subscribe to grid events for ingredient collection
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager != null)
        {
            gridManager.OnTilesCleared += OnTilesCleared;
        }
        
        // Initialize ingredient collection dictionary
        InitializeIngredientTracking();
        
        // Start first recipe if available
        if (availableRecipes.Count > 0 && !isRecipeActive)
        {
            StartRandomRecipe();
        }
    }
    
    /// <summary>
    /// Initialize ingredient tracking for all tile types
    /// </summary>
    private void InitializeIngredientTracking()
    {
        collectedIngredients.Clear();
        foreach (Tile.TileType tileType in System.Enum.GetValues(typeof(Tile.TileType)))
        {
            collectedIngredients[tileType] = 0;
        }
    }
    
    /// <summary>
    /// Start a random recipe from available recipes
    /// </summary>
    public void StartRandomRecipe()
    {
        if (availableRecipes.Count == 0) return;
        
        Recipe randomRecipe = availableRecipes[Random.Range(0, availableRecipes.Count)];
        StartRecipe(randomRecipe);
    }
    
    /// <summary>
    /// Start a specific recipe
    /// </summary>
    public void StartRecipe(Recipe recipe)
    {
        if (recipe == null || isRecipeActive) return;
        
        currentRecipe = recipe;
        isRecipeActive = true;
        
        // Reset ingredient collection for new recipe
        InitializeIngredientTracking();
        
        OnRecipeStarted?.Invoke(currentRecipe);
        
        Debug.Log($"Started recipe: {recipe.recipeName}");
    }
    
    /// <summary>
    /// Handle tiles being cleared and collect ingredients
    /// </summary>
    private void OnTilesCleared(int tilesCleared)
    {
        if (!isRecipeActive || currentRecipe == null) return;
        
        // This is a simplified version - in a real implementation,
        // you'd need to know which specific tiles were cleared
        // For now, we'll collect ingredients based on tile matches
        CollectIngredientsFromLastMatch();
    }
    
    /// <summary>
    /// Collect ingredients from the last match (simplified implementation)
    /// In a real implementation, this would receive the actual tiles that were matched
    /// </summary>
    private void CollectIngredientsFromLastMatch()
    {
        if (!isRecipeActive || currentRecipe == null) return;
        
        // Simplified: randomly collect some ingredients needed for the current recipe
        foreach (var requirement in currentRecipe.requiredIngredients)
        {
            if (collectedIngredients[requirement.ingredientType] < requirement.requiredAmount)
            {
                // Simulate collecting 1-3 of this ingredient type
                int collected = Random.Range(1, 4);
                CollectIngredient(requirement.ingredientType, collected);
                break; // Only collect one type per match for now
            }
        }
    }
    
    /// <summary>
    /// Collect a specific ingredient
    /// </summary>
    public void CollectIngredient(Tile.TileType ingredientType, int amount)
    {
        if (!isRecipeActive || currentRecipe == null) return;
        
        // Check if this ingredient is needed for current recipe
        bool isNeeded = false;
        foreach (var requirement in currentRecipe.requiredIngredients)
        {
            if (requirement.ingredientType == ingredientType)
            {
                isNeeded = true;
                break;
            }
        }
        
        if (!isNeeded) return;
        
        // Add to collection
        collectedIngredients[ingredientType] += amount;
        
        OnIngredientCollected?.Invoke(ingredientType, amount);
        
        // Check progress
        float progress = currentRecipe.GetCompletionProgress(collectedIngredients);
        OnRecipeProgressChanged?.Invoke(currentRecipe, progress);
        
        // Check if recipe is completed
        if (currentRecipe.IsCompleted(collectedIngredients))
        {
            CompleteRecipe();
        }
        
        Debug.Log($"Collected {amount} {ingredientType}. Progress: {progress:P}");
    }
    
    /// <summary>
    /// Complete the current recipe and show cooking animation
    /// </summary>
    private void CompleteRecipe()
    {
        if (!isRecipeActive || currentRecipe == null) return;
        
        StartCoroutine(PlayCookingAnimation());
    }
    
    /// <summary>
    /// Play cooking animation and award rewards
    /// </summary>
    private IEnumerator PlayCookingAnimation()
    {
        // Play cooking sound
        if (audioSource != null && cookingSound != null)
        {
            audioSource.PlayOneShot(cookingSound);
        }
        
        // Show cooking effect
        if (cookingEffectPrefab != null && cookingEffectParent != null)
        {
            GameObject effect = Instantiate(cookingEffectPrefab, cookingEffectParent);
            
            // Animate the effect
            effect.transform.localScale = Vector3.zero;
            effect.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            
            // Wait for animation duration
            yield return new WaitForSeconds(currentRecipe.cookingAnimationDuration);
            
            // Clean up effect
            if (effect != null)
            {
                effect.transform.DOScale(0f, 0.3f).OnComplete(() => Destroy(effect));
            }
        }
        else
        {
            yield return new WaitForSeconds(currentRecipe.cookingAnimationDuration);
        }
        
        // Play completion sound
        if (audioSource != null && recipeCompleteSound != null)
        {
            audioSource.PlayOneShot(recipeCompleteSound);
        }
        
        // Award rewards
        int totalReward = currentRecipe.GetTotalReward();
        if (gameManager != null)
        {
            // Add bonus score through the existing scoring system
            gameManager.OnScoreChanged?.Invoke(totalReward);
        }
        
        // Trigger completion event
        OnRecipeCompleted?.Invoke(currentRecipe, totalReward);
        
        // Reset recipe state
        Recipe completedRecipe = currentRecipe;
        currentRecipe = null;
        isRecipeActive = false;
        
        Debug.Log($"Recipe completed: {completedRecipe.recipeName}! Reward: {totalReward}");
        
        // Start next recipe after a delay
        yield return new WaitForSeconds(1f);
        StartRandomRecipe();
    }
    
    /// <summary>
    /// Get progress of current recipe
    /// </summary>
    public float GetCurrentRecipeProgress()
    {
        if (!isRecipeActive || currentRecipe == null) return 0f;
        return currentRecipe.GetCompletionProgress(collectedIngredients);
    }
    
    /// <summary>
    /// Get ingredients needed for current recipe
    /// </summary>
    public List<IngredientRequirement> GetCurrentRecipeRequirements()
    {
        if (!isRecipeActive || currentRecipe == null) return new List<IngredientRequirement>();
        return currentRecipe.requiredIngredients;
    }
    
    /// <summary>
    /// Get collected amount for a specific ingredient type
    /// </summary>
    public int GetCollectedAmount(Tile.TileType ingredientType)
    {
        return collectedIngredients.ContainsKey(ingredientType) ? collectedIngredients[ingredientType] : 0;
    }
    
    /// <summary>
    /// Add a recipe to available recipes
    /// </summary>
    public void AddAvailableRecipe(Recipe recipe)
    {
        if (recipe != null && !availableRecipes.Contains(recipe))
        {
            availableRecipes.Add(recipe);
        }
    }
    
    /// <summary>
    /// Remove a recipe from available recipes
    /// </summary>
    public void RemoveAvailableRecipe(Recipe recipe)
    {
        if (recipe != null && availableRecipes.Contains(recipe))
        {
            availableRecipes.Remove(recipe);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager != null)
        {
            gridManager.OnTilesCleared -= OnTilesCleared;
        }
    }
}