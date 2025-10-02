using UnityEngine;

/// <summary>
/// Mobile optimization settings for pizza-themed match-3 game.
/// Optimized for smooth pizza ingredient matching and UI interactions.
/// </summary>
public class MobileOptimization : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool optimizeForMobile = true;
    [SerializeField] private bool enablePerformanceMode = false; // For lower-end devices
    
    [Header("Touch Settings")]
    [SerializeField] private bool optimizeTouchInput = true;
    [SerializeField] private float touchSensitivity = 1.0f;
    
    void Awake()
    {
        ApplyBasicOptimizations();
        OptimizeForPizzaGameplay();
    }
    
    /// <summary>
    /// Apply basic mobile optimizations for performance
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
            QualitySettings.antiAliasing = enablePerformanceMode ? 0 : 2; // Slight AA unless performance mode
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;
            
            // Additional performance settings for lower-end devices
            if (enablePerformanceMode)
            {
                QualitySettings.pixelLightCount = 1;
                QualitySettings.globalTextureMipmapLimit = 1; // Reduce texture quality
                QualitySettings.particleRaycastBudget = 64;
            }
        }
        
        // Prevent screen dimming during pizza cooking gameplay
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Enable multitouch for mobile ingredient matching
        Input.multiTouchEnabled = true;
        
        Debug.Log($"Mobile optimizations applied (Performance Mode: {enablePerformanceMode})");
    }
    
    /// <summary>
    /// Apply optimizations specific to pizza match-3 gameplay
    /// </summary>
    private void OptimizeForPizzaGameplay()
    {
        if (!optimizeForMobile) return;
        
        // Optimize memory allocation for pizza ingredient tiles
        System.GC.Collect();
        
        // Set appropriate screen orientation for pizza making
        if (Screen.orientation != ScreenOrientation.Portrait && 
            Screen.orientation != ScreenOrientation.PortraitUpsideDown)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        
        // Optimize touch input for ingredient matching
        if (optimizeTouchInput)
        {
            // Configure input settings for responsive pizza ingredient swapping
            Time.fixedDeltaTime = 1.0f / 60.0f; // 60 FPS physics
        }
        
        Debug.Log("Pizza gameplay optimizations applied");
    }
}