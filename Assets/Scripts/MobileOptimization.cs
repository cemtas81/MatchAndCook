using UnityEngine;

/// <summary>
/// Simple mobile optimization settings for match-3 game.
/// Minimal configuration for better performance without excessive complexity.
/// </summary>
public class MobileOptimization : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool optimizeForMobile = true;
    
    void Awake()
    {
        ApplyBasicOptimizations();
    }
    
    /// <summary>
    /// Apply basic mobile optimizations
    /// </summary>
    private void ApplyBasicOptimizations()
    {
        // Set target frame rate for consistent performance
        Application.targetFrameRate = targetFrameRate;
        
        // Disable VSync for mobile (handled by target frame rate)
        QualitySettings.vSyncCount = 0;
        
        if (optimizeForMobile)
        {
            // Basic quality optimizations for mobile
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;
        }
        
        // Prevent screen dimming during gameplay
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Enable multitouch for mobile
        Input.multiTouchEnabled = true;
        
        Debug.Log("Basic mobile optimizations applied");
    }
}