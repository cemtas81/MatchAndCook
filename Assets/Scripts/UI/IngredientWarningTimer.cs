using UnityEngine;

/// <summary>
/// Manages the countdown timer when ingredients are insufficient at order start.
/// Provides a grace period for players to gather missing ingredients.
/// </summary>
public class IngredientWarningTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float warningDuration = 30f; // Grace period for gathering ingredients
    
    // State
    private float remainingWarningTime;
    private bool isWarningActive = false;
    
    // Events
    public System.Action<float> OnWarningTimeChanged;
    public System.Action OnWarningExpired;
    public System.Action OnWarningResolved;
    
    /// <summary>
    /// Start the warning timer
    /// </summary>
    public void StartWarningTimer()
    {
        remainingWarningTime = warningDuration;
        isWarningActive = true;
        OnWarningTimeChanged?.Invoke(remainingWarningTime);
    }
    
    /// <summary>
    /// Stop and resolve the warning timer
    /// </summary>
    public void ResolveWarning()
    {
        if (!isWarningActive) return;
        
        isWarningActive = false;
        OnWarningResolved?.Invoke();
    }
    
    /// <summary>
    /// Cancel the warning timer without triggering resolution
    /// </summary>
    public void CancelWarning()
    {
        isWarningActive = false;
    }
    
    void Update()
    {
        if (!isWarningActive) return;
        
        remainingWarningTime -= Time.deltaTime;
        OnWarningTimeChanged?.Invoke(remainingWarningTime);
        
        if (remainingWarningTime <= 0f)
        {
            isWarningActive = false;
            OnWarningExpired?.Invoke();
        }
    }
    
    public bool IsWarningActive => isWarningActive;
    public float RemainingWarningTime => remainingWarningTime;
}
