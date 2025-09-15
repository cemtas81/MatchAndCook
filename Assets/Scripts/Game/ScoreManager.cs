using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Manages score tracking, high scores, and score-related functionality.
/// Provides simple score persistence for mobile gaming sessions.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private bool saveHighScore = true;

    [Header("Combo Popup Settings")]
    [SerializeField] private GameObject comboPopupPrefab;
    [SerializeField] private Transform comboPopupParent;
    [SerializeField] private float popupDuration = 1.5f;
    [SerializeField] private float popupMoveDistance = 100f;

    [Header("Combo Messages")]
    [SerializeField]
    private string[] comboMessages = new string[] {
        "Good!",
        "Nice!",
        "Great!",
        "Awesome!",
        "Fantastic!",
        "Incredible!",
        "Unstoppable!",
        "Godlike!"
    };

    [Header("Combo Colors")]
    [SerializeField]
    private Color[] comboColors = new Color[] {
        new Color(0.2f, 0.8f, 0.2f),       // Light green
        new Color(0.0f, 1.0f, 0.0f),       // Green
        new Color(0.0f, 1.0f, 0.5f),       // Teal
        new Color(0.0f, 0.5f, 1.0f),       // Sky blue
        new Color(0.0f, 0.0f, 1.0f),       // Blue
        new Color(0.5f, 0.0f, 1.0f),       // Purple
        new Color(1.0f, 0.0f, 1.0f),       // Magenta
        new Color(1.0f, 0.0f, 0.5f)        // Dark pink
    };

    // Score tracking
    private int currentScore = 0;
    private int highScore = 0;
    private int sessionBestScore = 0;

    // Score breakdown
    private int totalTilesCleared = 0;
    private int totalCombos = 0;
    private int longestCombo = 0;

    // Events
    public System.Action<int> OnScoreUpdated;
    public System.Action<int> OnHighScoreBeaten;

    // Cache
    private Camera mainCamera;
    private List<GameObject> activePopups = new List<GameObject>();

    // Properties
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int SessionBestScore => sessionBestScore;
    public int TotalTilesCleared => totalTilesCleared;
    public int TotalCombos => totalCombos;
    public int LongestCombo => longestCombo;

    void Start()
    {
        LoadHighScore();
        mainCamera = Camera.main;

        // Eðer popup parent belirtilmemiþse canvas'ta ara
        if (comboPopupParent == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                comboPopupParent = canvas.transform;
            }
            else
            {
                comboPopupParent = transform;
            }
        }
    }

    /// <summary>
    /// Update the current score
    /// </summary>
    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        OnScoreUpdated?.Invoke(currentScore);

        // Check for session best
        if (currentScore > sessionBestScore)
        {
            sessionBestScore = currentScore;
        }

        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            OnHighScoreBeaten?.Invoke(highScore);

            if (saveHighScore)
            {
                SaveHighScore();
            }

            Debug.Log($"NEW HIGH SCORE: {highScore}!");
        }
    }

    /// <summary>
    /// Add points to the current score
    /// </summary>
    public void AddScore(int points)
    {
        UpdateScore(currentScore + points);
    }

    /// <summary>
    /// Record tiles cleared for statistics
    /// </summary>
    public void RecordTilesCleared(int tilesCleared)
    {
        totalTilesCleared += tilesCleared;
    }

    /// <summary>
    /// Record a combo for statistics and show popup
    /// </summary>
    public void RecordCombo(int comboLength)
    {
        totalCombos++;

        if (comboLength > longestCombo)
        {
            longestCombo = comboLength;
        }

        // Eðer combo 2'den büyükse popup göster (combo 1 ilk eþleþme olduðu için deðil)
        if (comboLength > 1)
        {
            ShowComboPopup(comboLength);
        }
    }

    /// <summary>
    /// Shows a combo popup with appropriate message based on combo length
    /// </summary>
    private void ShowComboPopup(int comboLength)
    {
        if (comboPopupPrefab == null) return;

        // Kaç popupýn gösterileceðini sýnýrla
        if (activePopups.Count > 3)
        {
            Destroy(activePopups[0]);
            activePopups.RemoveAt(0);
        }

        // Combo indeksini sýnýrla
        int messageIndex = Mathf.Min(comboLength - 2, comboMessages.Length - 1);
        int colorIndex = Mathf.Min(comboLength - 2, comboColors.Length - 1);

        // Popup metni ve rengi
        string message = comboMessages[messageIndex];
        Color color = comboColors[colorIndex];

        // Ekran merkezinde popup oluþtur
        GameObject popup = Instantiate(comboPopupPrefab, comboPopupParent);
        activePopups.Add(popup);

        // Ekran merkezi
        popup.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        // TextMeshProUGUI bileþenini bul
        TextMeshProUGUI textComponent = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"{message}\nCombo x{comboLength}";
            textComponent.color = color;
        }

        // Animasyon oluþtur
        Sequence comboAnimation = DOTween.Sequence();

        // Baþlangýçta büyüt
        comboAnimation.Append(popup.transform.DOScale(1.2f, 0.2f).From(0.5f).SetEase(Ease.OutBack));

        // Yukarý doðru animasyon
        comboAnimation.Append(popup.transform.DOLocalMoveY(popup.transform.localPosition.y + popupMoveDistance, popupDuration).SetEase(Ease.OutQuint));

        // Solma animasyonu
        comboAnimation.Join(popup.GetComponent<CanvasGroup>()?.DOFade(0, popupDuration * 0.5f).SetDelay(popupDuration * 0.5f));

        // Animasyon bittiðinde temizle
        comboAnimation.OnComplete(() => {
            activePopups.Remove(popup);
            Destroy(popup);
        });

        // Animasyonu baþlat
        comboAnimation.Play();
    }

    /// <summary>
    /// Reset current session scores
    /// </summary>
    public void ResetCurrentScore()
    {
        currentScore = 0;
        totalTilesCleared = 0;
        totalCombos = 0;
        longestCombo = 0;

        OnScoreUpdated?.Invoke(currentScore);

        // Temizle
        ClearActivePopups();
    }

    /// <summary>
    /// Clear all active combo popups
    /// </summary>
    private void ClearActivePopups()
    {
        foreach (GameObject popup in activePopups)
        {
            Destroy(popup);
        }
        activePopups.Clear();
    }

    /// <summary>
    /// Get formatted score string
    /// </summary>
    public string GetFormattedScore()
    {
        return FormatScore(currentScore);
    }

    /// <summary>
    /// Get formatted high score string
    /// </summary>
    public string GetFormattedHighScore()
    {
        return FormatScore(highScore);
    }

    /// <summary>
    /// Format score with proper number formatting
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
    /// Calculate score multiplier based on performance
    /// </summary>
    public float GetScoreMultiplier(int comboLength, int tilesCleared)
    {
        float baseMultiplier = 1f;

        // Combo bonus
        if (comboLength > 1)
        {
            baseMultiplier += (comboLength - 1) * 0.5f;
        }

        // Large clear bonus
        if (tilesCleared >= 5)
        {
            baseMultiplier += (tilesCleared - 4) * 0.2f;
        }

        return Mathf.Min(baseMultiplier, 5f); // Cap at 5x multiplier
    }

    /// <summary>
    /// Get game statistics summary
    /// </summary>
    public string GetStatsString()
    {
        return $"Current: {GetFormattedScore()} | Best: {GetFormattedHighScore()}\n" +
               $"Tiles Cleared: {totalTilesCleared} | Combos: {totalCombos} | Longest: {longestCombo}";
    }

    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("MatchAndCook_HighScore", highScore);
        PlayerPrefs.Save();
        Debug.Log($"High score saved: {highScore}");
    }

    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("MatchAndCook_HighScore", 0);
        Debug.Log($"High score loaded: {highScore}");
    }

    /// <summary>
    /// Reset high score (for testing or player request)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.DeleteKey("MatchAndCook_HighScore");
        PlayerPrefs.Save();
        Debug.Log("High score reset");
    }

    /// <summary>
    /// Check if current score is a new record
    /// </summary>
    public bool IsNewHighScore()
    {
        return currentScore > 0 && currentScore == highScore;
    }

    /// <summary>
    /// Get percentage progress towards next milestone
    /// </summary>
    public float GetMilestoneProgress(int milestone)
    {
        return Mathf.Clamp01((float)currentScore / milestone);
    }

    /// <summary>
    /// Award bonus points for special achievements
    /// </summary>
    public void AwardBonus(int bonusPoints, string reason)
    {
        AddScore(bonusPoints);
        Debug.Log($"Bonus awarded: +{bonusPoints} points for {reason}");
    }

    /// <summary>
    /// Clean up active popups when destroyed
    /// </summary>
    private void OnDestroy()
    {
        ClearActivePopups();
    }
}