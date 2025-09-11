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
    [SerializeField] private bool reduceBatteryUsage = true;
    
    [Header("Quality Settings")]
    [SerializeField] private bool disableShadows = true;
    [SerializeField] private bool optimizeForMobile = true;
    [SerializeField] private bool useLowQualityOnLowEndDevices = true;
    
    [Header("Device Detection")]
    [SerializeField] private bool autoDetectDeviceCapabilities = true;
    
    private bool _isLowEndDevice = false;
    
    void Awake()
    {
        if (autoDetectDeviceCapabilities)
        {
            DetectDeviceCapabilities();
        }
        
        ApplyMobileOptimizations();
    }
    
    /// <summary>
    /// Detects device capabilities to adapt settings
    /// </summary>
    private void DetectDeviceCapabilities()
    {
        // Check for low-end device markers
        _isLowEndDevice = SystemInfo.systemMemorySize < 2000 || 
                          SystemInfo.processorFrequency < 1500 ||
                          SystemInfo.processorCount <= 2;
        
        Debug.Log($"Device detected as: {(_isLowEndDevice ? "Low-end" : "Mid/High-end")}");
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
        
        // Battery saving settings
        if (reduceBatteryUsage)
        {
            Application.targetFrameRate = Mathf.Min(targetFrameRate, 30);
            QualitySettings.asyncUploadTimeSlice = 2;
            QualitySettings.asyncUploadBufferSize = 4;
        }
        
        // Optimize quality settings for mobile
        if (optimizeForMobile)
        {
            // Disable expensive effects
            if (disableShadows)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            
            // Set appropriate quality level based on device
            int qualityLevel = 2; // Medium quality default
            
            if (_isLowEndDevice && useLowQualityOnLowEndDevices)
            {
                qualityLevel = 0; // Lowest quality for low-end devices
                QualitySettings.pixelLightCount = 0;
                QualitySettings.globalTextureMipmapLimit = 1; // Reduce texture resolution
                QualitySettings.particleRaycastBudget = 64;
                QualitySettings.softParticles = false;
            }
            else
            {
                // Optimize rendering for mid/high-end
                QualitySettings.pixelLightCount = 1;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.particleRaycastBudget = 256;
            }
            
            QualitySettings.SetQualityLevel(qualityLevel, true);
            
            // Common optimizations for all devices
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = true;
        }
        
        // Ensure proper screen orientation handling
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
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
        
        // Set accelerometer update interval to save power
        Input.gyro.updateInterval = 0.1f;
    }
    
    /// <summary>
    /// Set up audio settings for mobile
    /// </summary>
    private void ConfigureMobileAudio()
    {
        // Configure audio for mobile performance
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.dspBufferSize = 1024; // Larger buffer for mobile stability
        config.sampleRate = 24000; // Lower sample rate for better performance
        AudioSettings.Reset(config);
        
        //// Ensure audio continues when app loses focus (optional)
        //AudioSettings.Mobile.muteState = AudioSettings.Mobile.MuteState.Never;
        AudioSettings.Mobile.stopAudioOutputOnMute = false;
    }
    
    void Start()
    {
        ConfigureMobileInput();
        ConfigureMobileAudio();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Re-apply critical settings when app resumes
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Apply mobile settings in editor for testing
    /// </summary>
    [ContextMenu("Apply Mobile Settings")]
    public void ApplyMobileSettingsInEditor()
    {
        DetectDeviceCapabilities();
        ApplyMobileOptimizations();
        ConfigureMobileInput();
        ConfigureMobileAudio();
        Debug.Log("Mobile settings applied in editor for testing");
    }
    #endif
}