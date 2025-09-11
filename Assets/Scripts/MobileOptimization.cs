using UnityEngine;

/// <summary>
/// Configures Unity project settings for optimal mobile match-3 performance.
/// Sets up rendering, quality, and input settings for mobile deployment.
/// </summary>
public class MobileOptimization : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = false;
    
    [Header("Quality Settings")]
    [SerializeField] private bool disableShadows = true;
    [SerializeField] private bool optimizeForMobile = true;
    
    void Awake()
    {
        ApplyMobileOptimizations();
    }
    
    /// <summary>
    /// Apply mobile-specific optimizations
    /// </summary>
    private void ApplyMobileOptimizations()
    {
        // Set target frame rate for consistent performance
        Application.targetFrameRate = targetFrameRate;
        
        // Disable VSync for mobile (handled by target frame rate)
        if (!enableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }
        
        // Optimize quality settings for mobile
        if (optimizeForMobile)
        {
            // Disable expensive effects
            if (disableShadows)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            
            // Set appropriate quality level
            QualitySettings.SetQualityLevel(2, true); // Medium quality
            
            // Optimize rendering
            QualitySettings.pixelLightCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;
        }
        
        // Ensure proper screen orientation handling
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        
        Debug.Log("Mobile optimizations applied successfully!");
    }
    
    /// <summary>
    /// Configure input settings for mobile
    /// </summary>
    private void ConfigureMobileInput()
    {
        // Enable multitouch (useful for future features)
        Input.multiTouchEnabled = true;
        
        // Set input sensitivity for mobile
        Input.compensateSensors = true;
    }
    
    /// <summary>
    /// Set up audio settings for mobile
    /// </summary>
    private void ConfigureMobileAudio()
    {
        // Configure audio for mobile performance
        AudioSettings.GetConfiguration(out var config);
        config.dspBufferSize = 1024; // Larger buffer for mobile stability
        AudioSettings.Reset(config);
        
        // Ensure audio continues when app loses focus (optional)
        AudioSettings.Mobile.muteState = AudioSettings.Mobile.MuteState.Never;
    }
    
    void Start()
    {
        ConfigureMobileInput();
        ConfigureMobileAudio();
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Apply mobile settings in editor for testing
    /// </summary>
    [ContextMenu("Apply Mobile Settings")]
    public void ApplyMobileSettingsInEditor()
    {
        ApplyMobileOptimizations();
        Debug.Log("Mobile settings applied in editor for testing");
    }
    #endif
}