using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying the current recipe at the top of the screen.
/// Shows recipe name, required ingredients, and completion progress.
/// </summary>
public class RecipeCardUI : MonoBehaviour
{
    [Header("Recipe Card Elements")]
    [SerializeField] private GameObject recipeCardPanel;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private Slider recipeProgressBar;
    [SerializeField] private Transform ingredientListParent;
    
    [Header("Ingredient Display")]
    [SerializeField] private GameObject ingredientItemPrefab;
    [SerializeField] private Color completedIngredientColor = Color.green;
    [SerializeField] private Color incompleteIngredientColor = Color.white;
    
    [Header("Animation Settings")]
    [SerializeField] private float showAnimationDuration = 0.5f;
    [SerializeField] private float hideAnimationDuration = 0.3f;
    [SerializeField] private float progressAnimationDuration = 0.3f;
    
    // Current state
    private Recipe currentRecipe;
    private List<IngredientItemUI> ingredientItems = new List<IngredientItemUI>();
    private RecipeManager recipeManager;
    
    // Animation tracking
    private Tween currentTween;
    
    void Start()
    {
        InitializeRecipeCard();
    }
    
    /// <summary>
    /// Initialize recipe card and find references
    /// </summary>
    private void InitializeRecipeCard()
    {
        recipeManager = FindFirstObjectByType<RecipeManager>();
        
        if (recipeManager != null)
        {
            recipeManager.OnRecipeStarted += ShowRecipe;
            recipeManager.OnRecipeCompleted += OnRecipeCompleted;
            recipeManager.OnIngredientCollected += OnIngredientCollected;
            recipeManager.OnRecipeProgressChanged += OnProgressChanged;
        }
        
        // Hide card initially
        if (recipeCardPanel != null)
        {
            recipeCardPanel.SetActive(false);
        }
        
        // Initialize progress bar
        if (recipeProgressBar != null)
        {
            recipeProgressBar.minValue = 0f;
            recipeProgressBar.maxValue = 1f;
            recipeProgressBar.value = 0f;
        }
    }
    
    /// <summary>
    /// Show recipe card with animation
    /// </summary>
    public void ShowRecipe(Recipe recipe)
    {
        if (recipe == null) return;
        
        currentRecipe = recipe;
        
        // Update UI elements
        UpdateRecipeDisplay();
        
        // Show panel
        if (recipeCardPanel != null)
        {
            recipeCardPanel.SetActive(true);
            
            // Animate in from top
            RectTransform rectTransform = recipeCardPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 originalPosition = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = originalPosition + Vector3.up * 200f;
                
                currentTween?.Kill();
                currentTween = rectTransform.DOAnchorPos(originalPosition, showAnimationDuration)
                    .SetEase(Ease.OutBack);
            }
        }
    }
    
    /// <summary>
    /// Hide recipe card with animation
    /// </summary>
    public void HideRecipe()
    {
        if (recipeCardPanel != null && recipeCardPanel.activeInHierarchy)
        {
            RectTransform rectTransform = recipeCardPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 hidePosition = rectTransform.anchoredPosition + Vector2.up * 200f;
                
                currentTween?.Kill();
                currentTween = rectTransform.DOAnchorPos(hidePosition, hideAnimationDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => recipeCardPanel.SetActive(false));
            }
            else
            {
                recipeCardPanel.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Update recipe display with current recipe information
    /// </summary>
    private void UpdateRecipeDisplay()
    {
        if (currentRecipe == null) return;
        
        // Update recipe name
        if (recipeNameText != null)
        {
            recipeNameText.text = currentRecipe.recipeName;
        }
        
        // Update recipe icon
        if (recipeIcon != null && currentRecipe.recipeIcon != null)
        {
            recipeIcon.sprite = currentRecipe.recipeIcon;
            recipeIcon.gameObject.SetActive(true);
        }
        else if (recipeIcon != null)
        {
            recipeIcon.gameObject.SetActive(false);
        }
        
        // Update ingredient list
        UpdateIngredientList();
        
        // Update progress
        UpdateProgress(0f);
    }
    
    /// <summary>
    /// Update ingredient list display
    /// </summary>
    private void UpdateIngredientList()
    {
        if (ingredientListParent == null || currentRecipe == null) return;
        
        // Clear existing items
        ClearIngredientItems();
        
        // Create new items for each required ingredient
        foreach (var requirement in currentRecipe.requiredIngredients)
        {
            CreateIngredientItem(requirement);
        }
    }
    
    /// <summary>
    /// Clear existing ingredient items
    /// </summary>
    private void ClearIngredientItems()
    {
        foreach (var item in ingredientItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        ingredientItems.Clear();
    }
    
    /// <summary>
    /// Create an ingredient item UI
    /// </summary>
    private void CreateIngredientItem(IngredientRequirement requirement)
    {
        if (ingredientItemPrefab == null) return;
        
        GameObject itemObj = Instantiate(ingredientItemPrefab, ingredientListParent);
        IngredientItemUI itemUI = itemObj.GetComponent<IngredientItemUI>();
        
        if (itemUI == null)
        {
            itemUI = itemObj.AddComponent<IngredientItemUI>();
        }
        
        itemUI.Initialize(requirement, 0); // Start with 0 collected
        ingredientItems.Add(itemUI);
    }
    
    /// <summary>
    /// Handle ingredient collection
    /// </summary>
    private void OnIngredientCollected(Tile.TileType ingredientType, int amount)
    {
        UpdateIngredientProgress(ingredientType);
    }
    
    /// <summary>
    /// Handle recipe progress change
    /// </summary>
    private void OnProgressChanged(Recipe recipe, float progress)
    {
        UpdateProgress(progress);
    }
    
    /// <summary>
    /// Update progress bar
    /// </summary>
    private void UpdateProgress(float progress)
    {
        if (recipeProgressBar != null)
        {
            currentTween?.Kill();
            currentTween = recipeProgressBar.DOValue(progress, progressAnimationDuration)
                .SetEase(Ease.OutQuad);
        }
    }
    
    /// <summary>
    /// Update ingredient progress display
    /// </summary>
    private void UpdateIngredientProgress(Tile.TileType ingredientType)
    {
        if (recipeManager == null) return;
        
        foreach (var itemUI in ingredientItems)
        {
            if (itemUI.IngredientType == ingredientType)
            {
                int collected = recipeManager.GetCollectedAmount(ingredientType);
                itemUI.UpdateProgress(collected);
                break;
            }
        }
    }
    
    /// <summary>
    /// Handle recipe completion
    /// </summary>
    private void OnRecipeCompleted(Recipe recipe, int reward)
    {
        // Show completion animation
        StartCoroutine(PlayCompletionAnimation());
    }
    
    /// <summary>
    /// Play recipe completion animation
    /// </summary>
    private System.Collections.IEnumerator PlayCompletionAnimation()
    {
        // Flash the card green
        if (recipeCardPanel != null)
        {
            Image panelImage = recipeCardPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color originalColor = panelImage.color;
                panelImage.DOColor(completedIngredientColor, 0.2f)
                    .SetLoops(3, LoopType.Yoyo)
                    .OnComplete(() => panelImage.color = originalColor);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        // Hide the card
        HideRecipe();
    }
    
    void OnDestroy()
    {
        // Clean up events and tweens
        if (recipeManager != null)
        {
            recipeManager.OnRecipeStarted -= ShowRecipe;
            recipeManager.OnRecipeCompleted -= OnRecipeCompleted;
            recipeManager.OnIngredientCollected -= OnIngredientCollected;
            recipeManager.OnRecipeProgressChanged -= OnProgressChanged;
        }
        
        currentTween?.Kill();
    }
}

/// <summary>
/// UI component for individual ingredient items
/// </summary>
public class IngredientItemUI : MonoBehaviour
{
    [Header("Ingredient UI Elements")]
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TextMeshProUGUI ingredientText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Colors")]
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color incompleteColor = Color.white;
    
    private IngredientRequirement requirement;
    private int currentProgress;
    
    public Tile.TileType IngredientType => requirement?.ingredientType ?? Tile.TileType.Red;
    
    /// <summary>
    /// Initialize ingredient item with requirement data
    /// </summary>
    public void Initialize(IngredientRequirement ingredientRequirement, int startingProgress)
    {
        requirement = ingredientRequirement;
        currentProgress = startingProgress;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update progress for this ingredient
    /// </summary>
    public void UpdateProgress(int newProgress)
    {
        currentProgress = newProgress;
        UpdateDisplay();
        
        // Animate if progress changed
        if (backgroundImage != null)
        {
            backgroundImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }
    }
    
    /// <summary>
    /// Update visual display
    /// </summary>
    private void UpdateDisplay()
    {
        if (requirement == null) return;
        
        // Update icon
        if (ingredientIcon != null && requirement.ingredientIcon != null)
        {
            ingredientIcon.sprite = requirement.ingredientIcon;
        }
        
        // Update text
        if (ingredientText != null)
        {
            ingredientText.text = requirement.ingredientName;
        }
        
        // Update progress text
        if (progressText != null)
        {
            int clamped = Mathf.Min(currentProgress, requirement.requiredAmount);
            progressText.text = $"{clamped}/{requirement.requiredAmount}";
        }
        
        // Update colors based on completion
        bool isCompleted = currentProgress >= requirement.requiredAmount;
        Color targetColor = isCompleted ? completedColor : incompleteColor;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = targetColor;
        }
        
        if (ingredientText != null)
        {
            ingredientText.color = isCompleted ? Color.white : Color.black;
        }
    }
}