using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Warning panel UI that displays when ingredients are insufficient.
/// Shows countdown timer and warning message to gather ingredients.
/// </summary>
public class IngredientWarningPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private TextMeshProUGUI warningTimerText;
    [SerializeField] private Image warningBackground;
    
    [Header("Visual Settings")]
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange
    [SerializeField] private Color urgentColor = new Color(1f, 0f, 0f, 0.9f); // Red
    [SerializeField] private float urgentThreshold = 10f; // Seconds when color becomes urgent
    
    [Header("Animation Settings")]
    [SerializeField] private bool enablePulseAnimation = true;
    [SerializeField] private float pulseSpeed = 2f;
    
    // State
    private bool isVisible = false;
    private float pulseTimer = 0f;
    
    void Start()
    {
        // Hide panel initially
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }
    
    /// <summary>
    /// Show the warning panel with message
    /// </summary>
    public void ShowWarning(string message = "Malzemeler eksik! Lütfen topla!")
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
            isVisible = true;
        }
        
        if (warningMessageText != null)
        {
            warningMessageText.text = message;
        }
        
        UpdateWarningAppearance(30f); // Start with normal warning color
    }
    
    /// <summary>
    /// Hide the warning panel
    /// </summary>
    public void HideWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
            isVisible = false;
        }
    }
    
    /// <summary>
    /// Update the warning timer display
    /// </summary>
    public void UpdateWarningTimer(float remainingTime)
    {
        if (warningTimerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            warningTimerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        UpdateWarningAppearance(remainingTime);
    }
    
    /// <summary>
    /// Update warning appearance based on remaining time
    /// </summary>
    private void UpdateWarningAppearance(float remainingTime)
    {
        if (warningBackground == null) return;
        
        // Change color based on urgency
        Color targetColor = remainingTime <= urgentThreshold ? urgentColor : warningColor;
        warningBackground.color = targetColor;
    }
    
    void Update()
    {
        if (!isVisible || !enablePulseAnimation) return;
        
        // Pulse animation for warning background
        if (warningBackground != null)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float alpha = Mathf.Lerp(0.7f, 1f, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            Color currentColor = warningBackground.color;
            currentColor.a = alpha;
            warningBackground.color = currentColor;
        }
    }
    
    public bool IsVisible => isVisible;
}
