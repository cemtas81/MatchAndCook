using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying leaderboards with different time periods.
/// Shows player rankings, scores, and social comparison features.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Button closeButton;
    
    [Header("Tab Controls")]
    [SerializeField] private Button allTimeButton;
    [SerializeField] private Button weeklyButton;
    [SerializeField] private Button monthlyButton;
    [SerializeField] private Button friendsButton;
    
    [Header("Header Info")]
    [SerializeField] private TextMeshProUGUI leaderboardTitle;
    [SerializeField] private TextMeshProUGUI playerRankText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    
    [Header("Colors")]
    [SerializeField] private Color goldColor = Color.yellow;
    [SerializeField] private Color silverColor = Color.gray;
    [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color playerColor = Color.green;
    
    // Current state
    private EnhancedScoreManager.LeaderboardType currentLeaderboardType = EnhancedScoreManager.LeaderboardType.AllTime;
    private List<LeaderboardEntryUI> entryUIs = new List<LeaderboardEntryUI>();
    private bool showingFriends = false;
    
    // References
    private EnhancedScoreManager scoreManager;
    
    // Properties
    public bool IsVisible => leaderboardPanel != null && leaderboardPanel.activeInHierarchy;
    
    void Start()
    {
        InitializeLeaderboardUI();
    }
    
    /// <summary>
    /// Initialize leaderboard UI and find references
    /// </summary>
    private void InitializeLeaderboardUI()
    {
        scoreManager = FindFirstObjectByType<EnhancedScoreManager>();
        
        // Set up button callbacks
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        if (allTimeButton != null)
        {
            allTimeButton.onClick.AddListener(() => ShowLeaderboard(EnhancedScoreManager.LeaderboardType.AllTime));
        }
        
        if (weeklyButton != null)
        {
            weeklyButton.onClick.AddListener(() => ShowLeaderboard(EnhancedScoreManager.LeaderboardType.Weekly));
        }
        
        if (monthlyButton != null)
        {
            monthlyButton.onClick.AddListener(() => ShowLeaderboard(EnhancedScoreManager.LeaderboardType.Monthly));
        }
        
        if (friendsButton != null)
        {
            friendsButton.onClick.AddListener(ShowFriendsLeaderboard);
        }
        
        // Subscribe to score manager events
        if (scoreManager != null)
        {
            scoreManager.OnLeaderboardUpdated += OnLeaderboardUpdated;
        }
        
        // Hide panel initially
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show the leaderboard UI
    /// </summary>
    public void Show()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            
            // Animate appearance
            leaderboardPanel.transform.localScale = Vector3.zero;
            leaderboardPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        
        // Show default leaderboard
        ShowLeaderboard(currentLeaderboardType);
    }
    
    /// <summary>
    /// Hide the leaderboard UI
    /// </summary>
    public void Hide()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.transform.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => leaderboardPanel.SetActive(false));
        }
    }
    
    /// <summary>
    /// Show a specific leaderboard type
    /// </summary>
    public void ShowLeaderboard(EnhancedScoreManager.LeaderboardType type)
    {
        currentLeaderboardType = type;
        showingFriends = false;
        
        if (scoreManager == null) return;
        
        List<LeaderboardEntry> entries = scoreManager.GetLeaderboard(type, 20);
        DisplayLeaderboard(entries, GetLeaderboardTitle(type));
        
        UpdatePlayerInfo(type);
        UpdateTabButtons();
    }
    
    /// <summary>
    /// Show friends leaderboard
    /// </summary>
    public void ShowFriendsLeaderboard()
    {
        showingFriends = true;
        
        if (scoreManager == null) return;
        
        List<LeaderboardEntry> friendsEntries = scoreManager.GetFriendsScores(currentLeaderboardType);
        DisplayLeaderboard(friendsEntries, "Friends - " + GetLeaderboardTitle(currentLeaderboardType));
        
        UpdateTabButtons();
    }
    
    /// <summary>
    /// Display leaderboard entries
    /// </summary>
    private void DisplayLeaderboard(List<LeaderboardEntry> entries, string title)
    {
        // Update title
        if (leaderboardTitle != null)
        {
            leaderboardTitle.text = title;
        }
        
        // Clear existing entries
        ClearLeaderboardEntries();
        
        // Create new entries
        for (int i = 0; i < entries.Count; i++)
        {
            CreateLeaderboardEntry(entries[i], i + 1);
        }
        
        // Animate entries
        AnimateEntriesAppearance();
    }
    
    /// <summary>
    /// Create a leaderboard entry UI
    /// </summary>
    private void CreateLeaderboardEntry(LeaderboardEntry entry, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null) return;
        
        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
        
        if (entryUI == null)
        {
            entryUI = entryObj.AddComponent<LeaderboardEntryUI>();
        }
        
        // Determine if this is the current player
        bool isCurrentPlayer = scoreManager != null && scoreManager.CurrentPlayer != null &&
            entry.playerId == scoreManager.CurrentPlayer.playerId;
        
        Color entryColor = GetRankColor(rank, isCurrentPlayer);
        entryUI.Initialize(entry, rank, entryColor, isCurrentPlayer);
        
        entryUIs.Add(entryUI);
    }
    
    /// <summary>
    /// Get color for a specific rank
    /// </summary>
    private Color GetRankColor(int rank, bool isCurrentPlayer)
    {
        if (isCurrentPlayer)
            return playerColor;
        
        switch (rank)
        {
            case 1: return goldColor;
            case 2: return silverColor;
            case 3: return bronzeColor;
            default: return normalColor;
        }
    }
    
    /// <summary>
    /// Clear existing leaderboard entries
    /// </summary>
    private void ClearLeaderboardEntries()
    {
        foreach (var entryUI in entryUIs)
        {
            if (entryUI != null && entryUI.gameObject != null)
            {
                Destroy(entryUI.gameObject);
            }
        }
        entryUIs.Clear();
    }
    
    /// <summary>
    /// Animate entries appearance
    /// </summary>
    private void AnimateEntriesAppearance()
    {
        for (int i = 0; i < entryUIs.Count; i++)
        {
            if (entryUIs[i] != null)
            {
                entryUIs[i].PlayAppearAnimation(i * 0.1f);
            }
        }
    }
    
    /// <summary>
    /// Update player info display
    /// </summary>
    private void UpdatePlayerInfo(EnhancedScoreManager.LeaderboardType type)
    {
        if (scoreManager == null) return;
        
        int playerRank = scoreManager.GetPlayerRank(type);
        
        if (playerRankText != null)
        {
            if (playerRank > 0)
            {
                playerRankText.text = $"Your Rank: #{playerRank}";
            }
            else
            {
                playerRankText.text = "Your Rank: Unranked";
            }
        }
        
        if (playerScoreText != null && scoreManager.CurrentPlayer != null)
        {
            int score = 0;
            switch (type)
            {
                case EnhancedScoreManager.LeaderboardType.Weekly:
                    score = scoreManager.CurrentPlayer.weeklyScore;
                    break;
                case EnhancedScoreManager.LeaderboardType.Monthly:
                    score = scoreManager.CurrentPlayer.monthlyScore;
                    break;
                default:
                    score = scoreManager.CurrentPlayer.totalScore;
                    break;
            }
            
            playerScoreText.text = $"Your Score: {score:N0}";
        }
    }
    
    /// <summary>
    /// Update tab button states
    /// </summary>
    private void UpdateTabButtons()
    {
        // Update button colors or states to show which tab is active
        if (allTimeButton != null)
        {
            allTimeButton.interactable = currentLeaderboardType != EnhancedScoreManager.LeaderboardType.AllTime || showingFriends;
        }
        
        if (weeklyButton != null)
        {
            weeklyButton.interactable = currentLeaderboardType != EnhancedScoreManager.LeaderboardType.Weekly || showingFriends;
        }
        
        if (monthlyButton != null)
        {
            monthlyButton.interactable = currentLeaderboardType != EnhancedScoreManager.LeaderboardType.Monthly || showingFriends;
        }
        
        if (friendsButton != null)
        {
            friendsButton.interactable = !showingFriends;
        }
    }
    
    /// <summary>
    /// Get title for leaderboard type
    /// </summary>
    private string GetLeaderboardTitle(EnhancedScoreManager.LeaderboardType type)
    {
        switch (type)
        {
            case EnhancedScoreManager.LeaderboardType.Weekly:
                return "Weekly Leaderboard";
            case EnhancedScoreManager.LeaderboardType.Monthly:
                return "Monthly Leaderboard";
            default:
                return "All-Time Leaderboard";
        }
    }
    
    /// <summary>
    /// Handle leaderboard updates
    /// </summary>
    private void OnLeaderboardUpdated(List<LeaderboardEntry> entries, EnhancedScoreManager.LeaderboardType type)
    {
        // Refresh if currently showing this leaderboard type
        if (IsVisible && type == currentLeaderboardType && !showingFriends)
        {
            ShowLeaderboard(type);
        }
    }
    
    void OnDestroy()
    {
        // Clean up events
        if (scoreManager != null)
        {
            scoreManager.OnLeaderboardUpdated -= OnLeaderboardUpdated;
        }
    }
}

/// <summary>
/// UI component for individual leaderboard entries
/// </summary>
public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("Entry Elements")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankTitleText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image rankIcon;
    
    // State
    private LeaderboardEntry entryData;
    private int rank;
    private bool isCurrentPlayer;
    
    /// <summary>
    /// Initialize leaderboard entry
    /// </summary>
    public void Initialize(LeaderboardEntry entry, int entryRank, Color color, bool isPlayer)
    {
        entryData = entry;
        rank = entryRank;
        isCurrentPlayer = isPlayer;
        
        UpdateDisplay(color);
    }
    
    /// <summary>
    /// Update entry display
    /// </summary>
    private void UpdateDisplay(Color color)
    {
        // Update rank
        if (rankText != null)
        {
            rankText.text = $"#{rank}";
            rankText.color = color;
        }
        
        // Update player name
        if (playerNameText != null)
        {
            playerNameText.text = entryData.playerName;
            
            if (isCurrentPlayer)
            {
                playerNameText.text += " (You)";
                playerNameText.fontStyle = FontStyles.Bold;
            }
        }
        
        // Update score
        if (scoreText != null)
        {
            scoreText.text = entryData.score.ToString("N0");
        }
        
        // Update rank title
        if (rankTitleText != null)
        {
            rankTitleText.text = entryData.rank;
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isCurrentPlayer ? new Color(color.r, color.g, color.b, 0.3f) : Color.clear;
        }
        
        // Update rank icon for top 3
        if (rankIcon != null)
        {
            if (rank <= 3)
            {
                rankIcon.gameObject.SetActive(true);
                rankIcon.color = color;
            }
            else
            {
                rankIcon.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Play appear animation
    /// </summary>
    public void PlayAppearAnimation(float delay)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.3f)
            .SetDelay(delay)
            .SetEase(Ease.OutBack);
    }
}