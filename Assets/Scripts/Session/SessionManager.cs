using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages game sessions where each session consists of 10 pizza orders.
/// Each session increases in difficulty with more ingredients and less time.
/// Simple and effective session progression system for mobile publishing.
/// </summary>
public class SessionManager : MonoBehaviour
{
    [Header("Session Settings")]
    [SerializeField] private int pizzasPerSession = 10;
    [SerializeField] private float timeReductionPerSession = 0.9f; // 10% time reduction each session
    
    [Header("Session State")]
    [SerializeField] private int currentSession = 1;
    [SerializeField] private int pizzasCompletedInSession = 0;
    [SerializeField] private int failedOrdersInSession = 0; // Baþarýsýz sipariþlerin sayýsý
    
    // Events
    public System.Action<int> OnSessionStarted;
    public System.Action<int, int> OnSessionProgress; // pizzasCompleted, pizzasTotal
    public System.Action<int> OnSessionCompleted;
    
    // Properties
    public int CurrentSession => currentSession;
    public int PizzasCompletedInSession => pizzasCompletedInSession;
    public int FailedOrdersInSession => failedOrdersInSession;
    public int PizzasPerSession => pizzasPerSession;
    public float CurrentTimeMultiplier => Mathf.Pow(timeReductionPerSession, currentSession - 1);
    
    void Start()
    {
        StartNewSession();
    }
    
    /// <summary>
    /// Start a new session
    /// </summary>
    private void StartNewSession()
    {
        pizzasCompletedInSession = 0;
        failedOrdersInSession = 0; // Reset failed orders
        OnSessionStarted?.Invoke(currentSession);
        
        //Debug.Log($"Session {currentSession} started! Complete {pizzasPerSession} pizzas. Time multiplier: {CurrentTimeMultiplier:F2}");
    }
    
    /// <summary>
    /// Register a completed pizza order
    /// </summary>
    public void RegisterPizzaCompleted(bool success)
    {
        if (success)
        {
            pizzasCompletedInSession++;
            OnSessionProgress?.Invoke(pizzasCompletedInSession, pizzasPerSession);
            
            //Debug.Log($"Pizza {pizzasCompletedInSession}/{pizzasPerSession} completed in session {currentSession}");
            
            // Check if session is complete
            if (pizzasCompletedInSession >= pizzasPerSession)
            {
                CompleteSession();
            }
        }
        else
        {
            // Baþarýsýz sipariþi kaydet
            failedOrdersInSession++;
            //Debug.Log($"Pizza order failed. Total failed in session: {failedOrdersInSession}");
        }
    }
    
    /// <summary>
    /// Complete the current session and start next one
    /// </summary>
    private void CompleteSession()
    {
        // Fire event to show session panel
        OnSessionCompleted?.Invoke(currentSession);
        
        //Debug.Log($"Session {currentSession} completed! Starting session {currentSession + 1}");
        
        // Session number is increased but new session is not started immediately
        // It will start when the player clicks continue
        currentSession++;
        
        // Don't call StartNewSession() here - it will be implicitly called when continuing
    }
    
    /// <summary>
    /// Continue to the next session (called by UI)
    /// </summary>
    public void ContinueToNextSession()
    {
        // Start the new session
        StartNewSession();
    }
    
    /// <summary>
    /// Get session progress as percentage (0-1)
    /// </summary>
    public float GetSessionProgress()
    {
        return (float)pizzasCompletedInSession / pizzasPerSession;
    }
    
    /// <summary>
    /// Reset session to beginning
    /// </summary>
    public void ResetSession()
    {
        currentSession = 1;
        pizzasCompletedInSession = 0;
        failedOrdersInSession = 0;
        StartNewSession();
    }
}
