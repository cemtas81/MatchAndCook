using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the main menu interface with mobile-optimized buttons and navigation.
/// Provides simple, clean menu design perfect for touch interaction.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI lastScoreText;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button resetHighScoreButton;
    
    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TextMeshProUGUI gameTitle;
    
    [Header("Animation")]
    [SerializeField] private float buttonAnimationScale = 1.1f;
    [SerializeField] private float animationDuration = 0.1f;
    
    // Settings
    private const string SOUND_VOLUME_KEY = "SoundVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string LAST_SCORE_KEY = "LastScore";
    
    void Start()
    {
        InitializeMenu();
        SetupButtons();
        LoadSettings();
        UpdateScoreDisplay();
    }
    
    /// <summary>
    /// Initialize menu elements
    /// </summary>
    private void InitializeMenu()
    {
        // Hide settings panel initially
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Set version text
        if (versionText != null)
            versionText.text = $"v{Application.version}";
            
        // Set game title
        if (gameTitle != null)
            gameTitle.text = "Match & Cook";
    }
    
    /// <summary>
    /// Set up button event listeners
    /// </summary>
    private void SetupButtons()
    {
        // Play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
            AddButtonAnimation(playButton);
        }
        
        // Settings button
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonAnimation(settingsButton);
        }
        
        // Exit button
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
            AddButtonAnimation(exitButton);
        }
        
        // Close settings button
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
            AddButtonAnimation(closeSettingsButton);
        }
        
        // Reset high score button
        if (resetHighScoreButton != null)
        {
            resetHighScoreButton.onClick.AddListener(OnResetHighScoreClicked);
            AddButtonAnimation(resetHighScoreButton);
        }
        
        // Volume sliders
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }
    
    /// <summary>
    /// Add touch animation to button
    /// </summary>
    private void AddButtonAnimation(Button button)
    {
        button.onClick.AddListener(() => AnimateButton(button.transform));
    }
    
    /// <summary>
    /// Start game - load game scene
    /// </summary>
    private void OnPlayClicked()
    {
        Debug.Log("Starting game...");
        
        // Save last played time
        PlayerPrefs.SetString("LastPlayed", System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
        
        // Load game scene
        SceneManager.LoadScene("GameScene");
    }
    
    /// <summary>
    /// Open settings panel
    /// </summary>
    private void OnSettingsClicked()
    {
        Debug.Log("Opening settings...");
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Close settings panel
    /// </summary>
    private void OnCloseSettingsClicked()
    {
        Debug.Log("Closing settings...");
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// Exit application
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Exiting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Reset high score with confirmation
    /// </summary>
    private void OnResetHighScoreClicked()
    {
        // In a full implementation, this would show a confirmation dialog
        PlayerPrefs.DeleteKey("MatchAndCook_HighScore");
        PlayerPrefs.Save();
        
        UpdateScoreDisplay();
        
        Debug.Log("High score reset!");
    }
    
    /// <summary>
    /// Handle sound volume change
    /// </summary>
    private void OnSoundVolumeChanged(float value)
    {
        // Set audio mixer or AudioSource volume
        AudioListener.volume = value;
        Debug.Log($"Sound volume: {value}");
    }
    
    /// <summary>
    /// Handle music volume change
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        // Set background music volume
        // This would typically control a music AudioSource
        Debug.Log($"Music volume: {value}");
    }
    
    /// <summary>
    /// Load saved settings
    /// </summary>
    private void LoadSettings()
    {
        // Load volume settings
        float soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.value = soundVolume;
            OnSoundVolumeChanged(soundVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            OnMusicVolumeChanged(musicVolume);
        }
    }
    
    /// <summary>
    /// Save current settings
    /// </summary>
    private void SaveSettings()
    {
        if (soundVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, soundVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Settings saved");
    }
    
    /// <summary>
    /// Update score display with high score and last score
    /// </summary>
    private void UpdateScoreDisplay()
    {
        int highScore = PlayerPrefs.GetInt("MatchAndCook_HighScore", 0);
        int lastScore = PlayerPrefs.GetInt(LAST_SCORE_KEY, 0);
        
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {FormatScore(highScore)}";
        }
        
        if (lastScoreText != null)
        {
            if (lastScore > 0)
            {
                lastScoreText.text = $"Last: {FormatScore(lastScore)}";
                lastScoreText.gameObject.SetActive(true);
            }
            else
            {
                lastScoreText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Format score for display
    /// </summary>
    private string FormatScore(int score)
    {
        if (score >= 1000000)
        {
            return $"{(score / 1000000f):F1}M";
        }
        else if (score >= 1000)
        {
            return $"{(score / 1000f):F1}K";
        }
        else
        {
            return score.ToString();
        }
    }
    
    /// <summary>
    /// Animate button press
    /// </summary>
    private void AnimateButton(Transform buttonTransform)
    {
        StartCoroutine(ButtonAnimation(buttonTransform));
    }
    
    /// <summary>
    /// Button press animation coroutine
    /// </summary>
    private System.Collections.IEnumerator ButtonAnimation(Transform buttonTransform)
    {
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 targetScale = originalScale * buttonAnimationScale;
        
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale back down
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        buttonTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// Check for updates or news (placeholder for future expansion)
    /// </summary>
    private void CheckForUpdates()
    {
        // This could check for game updates, news, daily challenges, etc.
        Debug.Log("Checking for updates...");
    }
    
    /// <summary>
    /// Show credits or about information
    /// </summary>
    public void ShowCredits()
    {
        Debug.Log("Match & Cook - A simple match-3 cooking game");
    }
}