using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// UI component for displaying pizza order progress and next pizza preview.
/// </summary>
public class PizzaSliderUI : MonoBehaviour
{
    [Header("Pizza Progress Display")]
    [SerializeField] private Image pizzaProgressRing; // Circular progress indicator
    [SerializeField] private Image pizzaIcon; // Center pizza icon
    [SerializeField] private TextMeshProUGUI progressText; // "3/5" style text
    [SerializeField] private TextMeshProUGUI pizzaNameText; // Name of current pizza order

    [Header("Next Pizza Preview")]
    [SerializeField] private GameObject nextPizzaPanel; // Container for next pizza UI
    [SerializeField] private TextMeshProUGUI nextPizzaNameText; // Name of next pizza
    [SerializeField] private Transform ingredientIconsContainer; // Container for ingredient icons
    [SerializeField] private GameObject ingredientItemPrefab; // Prefab with icon and count text

    [Header("Ingredient Icons")]
    [SerializeField] private Sprite tomatoSprite;
    [SerializeField] private Sprite cheeseSprite;
    [SerializeField] private Sprite pepperoniSprite;
    [SerializeField] private Sprite mushroomSprite;
    [SerializeField] private Sprite pepperSprite;
    [SerializeField] private Sprite onionSprite;
    [SerializeField] private Sprite olivesSprite;
    [SerializeField] private Sprite butterSprite;

    [Header("Customer Display")]
    [SerializeField] private Image customerAvatar;
    [SerializeField] private Transform customerContainer;
    [SerializeField] private TextMeshProUGUI customerNameText;

    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalTimeColor = Color.white;
    [SerializeField] private Color urgentTimeColor = Color.red;
    [SerializeField] private float urgentTimeThreshold = 30f;

    [Header("Animation Settings")]
    [SerializeField] private float progressAnimationDuration = 0.5f;
    [SerializeField] private float completionCelebrationDuration = 1f;
    [SerializeField] private GameObject completionEffect;

    // Internal state
    private PizzaOrderManager pizzaOrderManager;
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private bool isAnimatingProgress = false;
    private List<GameObject> ingredientItems = new List<GameObject>();

    void Start()
    {
        InitializePizzaSliderUI();
    }

    /// <summary>
    /// Initialize the pizza slider UI and connect to pizza order manager
    /// </summary>
    private void InitializePizzaSliderUI()
    {
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();

        if (pizzaOrderManager != null)
        {
            // Subscribe to pizza order events
            pizzaOrderManager.OnOrderStarted += OnOrderStarted;
            pizzaOrderManager.OnOrderCompleted += OnOrderCompleted;
            pizzaOrderManager.OnOrderProgressChanged += OnProgressChanged;
            pizzaOrderManager.OnOrderTimeChanged += OnTimeChanged;
            pizzaOrderManager.OnIngredientCollected += OnIngredientCollected;

            // Subscribe to next pizza order events
            pizzaOrderManager.OnNextOrderPrepared += UpdateNextPizzaInfo;

            // Initialize with current next order if available
            if (pizzaOrderManager.NextOrder != null)
            {
                UpdateNextPizzaInfo(pizzaOrderManager.NextOrder);
            }
        }

        ResetProgressDisplay();

        if (nextPizzaPanel != null && (pizzaOrderManager == null || pizzaOrderManager.NextOrder == null))
        {
            nextPizzaPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Update next pizza preview with name and ingredient icons
    /// </summary>
    private void UpdateNextPizzaInfo(PizzaOrder nextOrder)
    {
        if (nextOrder == null || nextPizzaPanel == null) return;

        // Show panel
        nextPizzaPanel.SetActive(true);

        // Update pizza name
        if (nextPizzaNameText != null)
        {
            nextPizzaNameText.text = nextOrder.pizzaName;
        }

        // Update ingredient icons
        UpdateIngredientIcons(nextOrder);

        // Add a small bounce animation
        nextPizzaPanel.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
    }

    /// <summary>
    /// Update ingredient icons with quantities
    /// </summary>
    private void UpdateIngredientIcons(PizzaOrder nextOrder)
    {
        // Clear existing ingredient items
        ClearIngredientItems();

        if (ingredientIconsContainer == null || ingredientItemPrefab == null) return;

        // Add new ingredient items
        foreach (var ingredient in nextOrder.requiredIngredients)
        {
            // Create ingredient item
            GameObject itemObj = Instantiate(ingredientItemPrefab, ingredientIconsContainer);
            ingredientItems.Add(itemObj);

            // Get components
            Image iconImage = itemObj.GetComponentInChildren<Image>();
            TextMeshProUGUI countText = itemObj.GetComponentInChildren<TextMeshProUGUI>();

            // Set icon based on ingredient type
            if (iconImage != null)
            {
                Sprite ingredientSprite = GetIngredientSprite(ingredient.ingredientType);
                if (ingredientSprite != null)
                {
                    iconImage.sprite = ingredientSprite;
                }
            }

            // Set count text
            if (countText != null)
            {
                countText.text = $"x{ingredient.requiredAmount}";
            }
        }
    }

    /// <summary>
    /// Clear existing ingredient items
    /// </summary>
    private void ClearIngredientItems()
    {
        foreach (var item in ingredientItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        ingredientItems.Clear();
    }

    /// <summary>
    /// Get sprite for ingredient type
    /// </summary>
    private Sprite GetIngredientSprite(Tile.TileType ingredientType)
    {
        switch (ingredientType)
        {
            case Tile.TileType.Tomato:
                return tomatoSprite;
            case Tile.TileType.Cheese:
                return cheeseSprite;
            case Tile.TileType.Pepperoni:
                return pepperoniSprite;
            case Tile.TileType.Mushroom:
                return mushroomSprite;
            case Tile.TileType.Pepper:
                return pepperSprite;
            case Tile.TileType.Onion:
                return onionSprite;
            case Tile.TileType.Olives:
                return olivesSprite;
            case Tile.TileType.Butter:
                return butterSprite;
            default:
                return null;
        }
    }

    /// <summary>
    /// Handle new pizza order started
    /// </summary>
    private void OnOrderStarted(PizzaOrder order)
    {
        if (order == null) return;

        if (customerAvatar != null && order.customerAvatar != null)
        {
            customerAvatar.sprite = order.customerAvatar;
            customerAvatar.gameObject.SetActive(true);
        }

        if (customerNameText != null)
        {
            customerNameText.text = order.customerName;
        }

        if (pizzaNameText != null)
        {
            pizzaNameText.text = order.pizzaName;
        }

        ResetProgressDisplay();
        ShowPizzaOrderUI();
    }

    /// <summary>
    /// Handle pizza order completion
    /// </summary>
    private void OnOrderCompleted(PizzaOrder order, bool success)
    {
        if (success)
        {
            PlayCompletionCelebration();
        }
        else
        {
            PlayFailureAnimation();
        }

        HidePizzaOrderUI();
    }

    private void OnProgressChanged(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
        AnimateProgressChange();
    }

    private void OnTimeChanged(float remainingTime)
    {
        UpdateTimerDisplay(remainingTime);
    }

    private void OnIngredientCollected(Tile.TileType ingredientType, int amount)
    {
        PlayIngredientCollectionEffect(ingredientType, amount);
    }

    private void ResetProgressDisplay()
    {
        currentProgress = 0f;
        targetProgress = 0f;

        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.fillAmount = 0f;
        }

        if (progressText != null)
        {
            progressText.text = "0%";
        }
    }

    private void AnimateProgressChange()
    {
        if (isAnimatingProgress) return;

        isAnimatingProgress = true;

        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.DOFillAmount(targetProgress, progressAnimationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => isAnimatingProgress = false);
        }

        if (progressText != null)
        {
            DOTween.To(() => currentProgress, x => {
                currentProgress = x;
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
            }, targetProgress, progressAnimationDuration)
            .SetEase(Ease.OutQuad);
        }

        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 10, 1f);
        }
    }

    private void UpdateTimerDisplay(float remainingTime)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        Color timeColor = remainingTime <= urgentTimeThreshold ? urgentTimeColor : normalTimeColor;
        timerText.color = timeColor;

        if (remainingTime <= urgentTimeThreshold && remainingTime > 0f)
        {
            if (timerText.transform.localScale == Vector3.one)
            {
                timerText.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            timerText.transform.DOKill();
            timerText.transform.localScale = Vector3.one;
        }
    }

    private void PlayCompletionCelebration()
    {
        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOScale(1.3f, completionCelebrationDuration * 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    pizzaIcon.transform.DOScale(1f, completionCelebrationDuration * 0.5f)
                        .SetEase(Ease.InBack);
                });
        }

        if (completionEffect != null)
        {
            GameObject effect = Instantiate(completionEffect, transform);
            Destroy(effect, completionCelebrationDuration);
        }

        if (pizzaProgressRing != null)
        {
            Color originalColor = pizzaProgressRing.color;
            pizzaProgressRing.color = Color.green;
            pizzaProgressRing.DOColor(originalColor, completionCelebrationDuration);
        }
    }

    private void PlayFailureAnimation()
    {
        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOShakePosition(1f, 10f, 10, 90f);
        }

        if (pizzaProgressRing != null)
        {
            Color originalColor = pizzaProgressRing.color;
            pizzaProgressRing.color = Color.red;
            pizzaProgressRing.DOColor(originalColor, 1f);
        }
    }

    private void PlayIngredientCollectionEffect(Tile.TileType ingredientType, int amount)
    {
        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 1f);
        }
    }

    private void ShowPizzaOrderUI()
    {
        gameObject.SetActive(true);

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + Vector2.up * 100f;
            rectTransform.DOAnchorPos(originalPosition, 0.5f).SetEase(Ease.OutBack);
        }

        if (customerAvatar != null)
        {
            customerAvatar.transform.localScale = Vector3.zero;
            customerAvatar.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }
    }

    private void HidePizzaOrderUI()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 exitPosition = rectTransform.anchoredPosition + Vector2.up * 100f;
            rectTransform.DOAnchorPos(exitPosition, 0.5f)
                .SetEase(Ease.InBack)
                .SetDelay(2f)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    void OnDestroy()
    {
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnOrderStarted -= OnOrderStarted;
            pizzaOrderManager.OnOrderCompleted -= OnOrderCompleted;
            pizzaOrderManager.OnOrderProgressChanged -= OnProgressChanged;
            pizzaOrderManager.OnOrderTimeChanged -= OnTimeChanged;
            pizzaOrderManager.OnIngredientCollected -= OnIngredientCollected;
            pizzaOrderManager.OnNextOrderPrepared -= UpdateNextPizzaInfo;
        }

        transform.DOKill();
        if (pizzaIcon != null) pizzaIcon.transform.DOKill();
        if (pizzaProgressRing != null) pizzaProgressRing.transform.DOKill();
        if (timerText != null) timerText.transform.DOKill();
        if (nextPizzaPanel != null) nextPizzaPanel.transform.DOKill();

        ClearIngredientItems();
    }
}