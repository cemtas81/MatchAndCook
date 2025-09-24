using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying and managing power-ups.
/// Shows available power-ups, cooldowns, and handles activation.
/// </summary>
public class PowerUpUI : MonoBehaviour
{
    [Header("Power-Up UI Elements")]
    [SerializeField] private Transform powerUpContainer;
    [SerializeField] private GameObject powerUpButtonPrefab;
    [SerializeField] private int maxVisiblePowerUps = 6;
    
    [Header("Animation Settings")]
    [SerializeField] private float cooldownAnimationDuration = 0.3f;
    [SerializeField] private float activationAnimationDuration = 0.5f;
    
    // Current state
    private Dictionary<PowerUpManager.PowerUpType, PowerUpButtonUI> powerUpButtons = new Dictionary<PowerUpManager.PowerUpType, PowerUpButtonUI>();
    private PowerUpManager powerUpManager;
    
    void Start()
    {
        InitializePowerUpUI();
    }
    
    /// <summary>
    /// Initialize power-up UI and find references
    /// </summary>
    private void InitializePowerUpUI()
    {
        powerUpManager = FindFirstObjectByType<PowerUpManager>();
        
        if (powerUpManager != null)
        {
            powerUpManager.OnPowerUpGained += OnPowerUpGained;
            powerUpManager.OnPowerUpUsed += OnPowerUpUsed;
        }
        
        // Create power-up buttons for each type
        CreatePowerUpButtons();
    }
    
    /// <summary>
    /// Create buttons for all power-up types
    /// </summary>
    private void CreatePowerUpButtons()
    {
        if (powerUpButtonPrefab == null || powerUpContainer == null) return;
        
        foreach (PowerUpManager.PowerUpType type in System.Enum.GetValues(typeof(PowerUpManager.PowerUpType)))
        {
            CreatePowerUpButton(type);
        }
    }
    
    /// <summary>
    /// Create a button for a specific power-up type
    /// </summary>
    private void CreatePowerUpButton(PowerUpManager.PowerUpType type)
    {
        GameObject buttonObj = Instantiate(powerUpButtonPrefab, powerUpContainer);
        PowerUpButtonUI buttonUI = buttonObj.GetComponent<PowerUpButtonUI>();
        
        if (buttonUI == null)
        {
            buttonUI = buttonObj.AddComponent<PowerUpButtonUI>();
        }
        
        buttonUI.Initialize(type, OnPowerUpButtonClicked);
        powerUpButtons[type] = buttonUI;
        
        // Update initial state
        UpdatePowerUpButton(type);
    }
    
    /// <summary>
    /// Handle power-up button click
    /// </summary>
    private void OnPowerUpButtonClicked(PowerUpManager.PowerUpType type)
    {
        if (powerUpManager != null && powerUpManager.CanUsePowerUp(type))
        {
            bool success = powerUpManager.UsePowerUp(type);
            if (success)
            {
                PlayActivationAnimation(type);
            }
        }
    }
    
    /// <summary>
    /// Handle power-up gained
    /// </summary>
    private void OnPowerUpGained(PowerUpManager.PowerUpType type, int newAmount)
    {
        UpdatePowerUpButton(type);
        PlayGainedAnimation(type);
    }
    
    /// <summary>
    /// Handle power-up used
    /// </summary>
    private void OnPowerUpUsed(PowerUpManager.PowerUpType type)
    {
        UpdatePowerUpButton(type);
    }
    
    /// <summary>
    /// Update a specific power-up button
    /// </summary>
    private void UpdatePowerUpButton(PowerUpManager.PowerUpType type)
    {
        if (!powerUpButtons.ContainsKey(type) || powerUpManager == null) return;
        
        PowerUpButtonUI button = powerUpButtons[type];
        int count = powerUpManager.GetPowerUpCount(type);
        bool canUse = powerUpManager.CanUsePowerUp(type);
        float cooldown = powerUpManager.GetRemainingCooldown();
        
        button.UpdateState(count, canUse, cooldown);
    }
    
    /// <summary>
    /// Play power-up gained animation
    /// </summary>
    private void PlayGainedAnimation(PowerUpManager.PowerUpType type)
    {
        if (!powerUpButtons.ContainsKey(type)) return;
        
        PowerUpButtonUI button = powerUpButtons[type];
        button.PlayGainedAnimation();
    }
    
    /// <summary>
    /// Play power-up activation animation
    /// </summary>
    private void PlayActivationAnimation(PowerUpManager.PowerUpType type)
    {
        if (!powerUpButtons.ContainsKey(type)) return;
        
        PowerUpButtonUI button = powerUpButtons[type];
        button.PlayActivationAnimation();
    }
    
    void Update()
    {
        // Update cooldown timers
        if (powerUpManager != null)
        {
            float remainingCooldown = powerUpManager.GetRemainingCooldown();
            if (remainingCooldown > 0)
            {
                foreach (var button in powerUpButtons.Values)
                {
                    button.UpdateCooldown(remainingCooldown);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up events
        if (powerUpManager != null)
        {
            powerUpManager.OnPowerUpGained -= OnPowerUpGained;
            powerUpManager.OnPowerUpUsed -= OnPowerUpUsed;
        }
    }
}

/// <summary>
/// UI component for individual power-up buttons
/// </summary>
public class PowerUpButtonUI : MonoBehaviour
{
    [Header("Button Elements")]
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    
    [Header("Visual States")]
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color cooldownColor = Color.red;
    
    // State
    private PowerUpManager.PowerUpType powerUpType;
    private System.Action<PowerUpManager.PowerUpType> onClickCallback;
    private Tween currentTween;
    
    /// <summary>
    /// Initialize power-up button
    /// </summary>
    public void Initialize(PowerUpManager.PowerUpType type, System.Action<PowerUpManager.PowerUpType> onClick)
    {
        powerUpType = type;
        onClickCallback = onClick;
        
        // Find components if not assigned
        if (button == null) button = GetComponent<Button>();
        if (icon == null) icon = GetComponentInChildren<Image>();
        if (countText == null) countText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Set up button callback
        if (button != null)
        {
            button.onClick.AddListener(() => onClickCallback?.Invoke(powerUpType));
        }
        
        // Set up cooldown overlay
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.gameObject.SetActive(false);
        }
        
        // Set power-up icon (placeholder - would use actual sprites in real game)
        UpdateIcon();
    }
    
    /// <summary>
    /// Update button state
    /// </summary>
    public void UpdateState(int count, bool canUse, float cooldown)
    {
        // Update count display
        if (countText != null)
        {
            countText.text = count > 0 ? count.ToString() : "";
            countText.gameObject.SetActive(count > 0);
        }
        
        // Update button interactability
        if (button != null)
        {
            button.interactable = canUse && count > 0;
        }
        
        // Update visual state
        Color targetColor = (canUse && count > 0) ? enabledColor : disabledColor;
        if (icon != null)
        {
            icon.color = targetColor;
        }
        
        // Show/hide cooldown overlay
        if (cooldownOverlay != null)
        {
            bool showCooldown = cooldown > 0 && count > 0;
            cooldownOverlay.gameObject.SetActive(showCooldown);
        }
    }
    
    /// <summary>
    /// Update cooldown display
    /// </summary>
    public void UpdateCooldown(float remainingTime)
    {
        if (cooldownOverlay != null && cooldownOverlay.gameObject.activeInHierarchy)
        {
            // Assuming max cooldown is 3 seconds (from PowerUpManager)
            float maxCooldown = 3f;
            float fillAmount = remainingTime / maxCooldown;
            cooldownOverlay.fillAmount = fillAmount;
        }
        
        if (cooldownText != null && remainingTime > 0)
        {
            cooldownText.text = remainingTime.ToString("F1") + "s";
            cooldownText.gameObject.SetActive(true);
        }
        else if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Play gained animation
    /// </summary>
    public void PlayGainedAnimation()
    {
        currentTween?.Kill();
        currentTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 8)
            .SetEase(Ease.OutBounce);
    }
    
    /// <summary>
    /// Play activation animation
    /// </summary>
    public void PlayActivationAnimation()
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(1.2f, 0.1f)
            .OnComplete(() => transform.DOScale(1f, 0.2f));
    }
    
    /// <summary>
    /// Update power-up icon based on type
    /// </summary>
    private void UpdateIcon()
    {
        if (icon == null) return;
        
        // Set placeholder colors for different power-up types
        Color iconColor = Color.white;
        switch (powerUpType)
        {
            case PowerUpManager.PowerUpType.Bomb:
                iconColor = Color.red;
                break;
            case PowerUpManager.PowerUpType.IngredientSelector:
                iconColor = Color.green;
                break;
            case PowerUpManager.PowerUpType.TimeAdd:
                iconColor = Color.blue;
                break;
            case PowerUpManager.PowerUpType.Rainbow:
                iconColor = Color.magenta;
                break;
            case PowerUpManager.PowerUpType.DoubleScore:
                iconColor = Color.yellow;
                break;
            case PowerUpManager.PowerUpType.ExtraMove:
                iconColor = Color.cyan;
                break;
        }
        
        icon.color = iconColor;
    }
    
    void OnDestroy()
    {
        currentTween?.Kill();
    }
}