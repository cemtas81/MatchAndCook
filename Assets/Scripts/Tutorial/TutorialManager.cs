using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Manages tutorial sequences and guided learning for new players.
/// Provides step-by-step instructions for all game mechanics.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Image highlightOverlay;
    [SerializeField] private Transform pointer;
    
    [Header("Tutorial Settings")]
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private bool canSkip = true;
    
    // Current state
    private int currentStepIndex = 0;
    private bool tutorialActive = false;
    private bool tutorialCompleted = false;
    private Coroutine currentTutorialCoroutine;
    
    // Events
    public System.Action OnTutorialStarted;
    public System.Action OnTutorialCompleted;
    public System.Action OnTutorialSkipped;
    public System.Action<int> OnStepCompleted;
    
    // References
    private GameManager gameManager;
    private GridManager gridManager;
    private UIManager uiManager;
    
    // Properties
    public bool IsTutorialActive => tutorialActive;
    public bool IsTutorialCompleted => tutorialCompleted;
    public int CurrentStep => currentStepIndex;
    
    // Constants for PlayerPrefs
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";
    
    void Start()
    {
        InitializeTutorialManager();
    }
    
    /// <summary>
    /// Initialize tutorial manager and check if tutorial should run
    /// </summary>
    private void InitializeTutorialManager()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        gridManager = FindFirstObjectByType<GridManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        
        // Set up button callbacks
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
            skipButton.gameObject.SetActive(canSkip);
        }
        
        // Hide tutorial panel initially
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // Check if tutorial should run
        CheckForTutorial();
    }
    
    /// <summary>
    /// Check if tutorial should run for new players
    /// </summary>
    private void CheckForTutorial()
    {
        tutorialCompleted = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
        
        // Start tutorial for new players or if specifically requested
        if (!tutorialCompleted && tutorialSteps.Count > 0)
        {
            StartTutorial();
        }
    }
    
    /// <summary>
    /// Start the tutorial sequence
    /// </summary>
    public void StartTutorial()
    {
        if (tutorialActive || tutorialSteps.Count == 0) return;
        
        tutorialActive = true;
        currentStepIndex = 0;
        
        // Pause the game during tutorial
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
        
        // Show tutorial panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        
        OnTutorialStarted?.Invoke();
        
        // Start tutorial sequence
        currentTutorialCoroutine = StartCoroutine(TutorialSequence());
        
        Debug.Log("Tutorial started!");
    }
    
    /// <summary>
    /// Tutorial sequence coroutine
    /// </summary>
    private IEnumerator TutorialSequence()
    {
        while (currentStepIndex < tutorialSteps.Count && tutorialActive)
        {
            yield return StartCoroutine(ExecuteTutorialStep(tutorialSteps[currentStepIndex]));
            
            if (autoAdvance)
            {
                yield return new WaitForSeconds(stepDelay);
                NextStep();
            }
            else
            {
                // Wait for manual advancement
                yield return new WaitUntil(() => !tutorialActive || currentStepIndex >= tutorialSteps.Count);
            }
        }
        
        if (tutorialActive)
        {
            CompleteTutorial();
        }
    }
    
    /// <summary>
    /// Execute a single tutorial step
    /// </summary>
    private IEnumerator ExecuteTutorialStep(TutorialStep step)
    {
        // Update tutorial text
        if (tutorialText != null)
        {
            tutorialText.text = step.instructionText;
        }
        
        // Handle step-specific actions
        switch (step.stepType)
        {
            case TutorialStepType.ShowText:
                yield return ShowTextStep(step);
                break;
            case TutorialStepType.HighlightElement:
                yield return HighlightElementStep(step);
                break;
            case TutorialStepType.WaitForAction:
                yield return WaitForActionStep(step);
                break;
            case TutorialStepType.PlayAnimation:
                yield return PlayAnimationStep(step);
                break;
        }
        
        OnStepCompleted?.Invoke(currentStepIndex);
    }
    
    /// <summary>
    /// Execute text display step
    /// </summary>
    private IEnumerator ShowTextStep(TutorialStep step)
    {
        // Animate text appearance
        if (tutorialText != null)
        {
            tutorialText.transform.localScale = Vector3.zero;
            tutorialText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce);
        }
        
        yield return new WaitForSeconds(step.duration);
    }
    
    /// <summary>
    /// Execute element highlighting step
    /// </summary>
    private IEnumerator HighlightElementStep(TutorialStep step)
    {
        if (step.targetElement != null)
        {
            HighlightElement(step.targetElement);
            
            // Point to the element
            if (pointer != null)
            {
                Vector3 targetPos = step.targetElement.position;
                pointer.position = targetPos;
                pointer.gameObject.SetActive(true);
                
                // Animate pointer
                pointer.DOPunchScale(Vector3.one * 0.2f, 0.5f).SetLoops(-1, LoopType.Restart);
            }
        }
        
        yield return new WaitForSeconds(step.duration);
    }
    
    /// <summary>
    /// Execute wait for action step
    /// </summary>
    private IEnumerator WaitForActionStep(TutorialStep step)
    {
        // This would wait for specific player actions
        // For now, just wait for the specified duration
        yield return new WaitForSeconds(step.duration);
    }
    
    /// <summary>
    /// Execute animation step
    /// </summary>
    private IEnumerator PlayAnimationStep(TutorialStep step)
    {
        // Play custom animations if needed
        yield return new WaitForSeconds(step.duration);
    }
    
    /// <summary>
    /// Highlight a UI element
    /// </summary>
    private void HighlightElement(Transform element)
    {
        if (highlightOverlay == null || element == null) return;
        
        // Position overlay over the target element
        RectTransform elementRect = element.GetComponent<RectTransform>();
        if (elementRect != null)
        {
            RectTransform overlayRect = highlightOverlay.GetComponent<RectTransform>();
            if (overlayRect != null)
            {
                overlayRect.position = elementRect.position;
                overlayRect.sizeDelta = elementRect.sizeDelta * 1.2f; // Slightly larger
            }
        }
        
        highlightOverlay.gameObject.SetActive(true);
        
        // Animate highlight
        highlightOverlay.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
    
    /// <summary>
    /// Clear current highlights
    /// </summary>
    private void ClearHighlights()
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(false);
            highlightOverlay.DOKill();
        }
        
        if (pointer != null)
        {
            pointer.gameObject.SetActive(false);
            pointer.DOKill();
        }
    }
    
    /// <summary>
    /// Advance to next tutorial step
    /// </summary>
    public void NextStep()
    {
        if (!tutorialActive) return;
        
        ClearHighlights();
        currentStepIndex++;
        
        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
        }
    }
    
    /// <summary>
    /// Skip the entire tutorial
    /// </summary>
    public void SkipTutorial()
    {
        if (!tutorialActive || !canSkip) return;
        
        OnTutorialSkipped?.Invoke();
        CompleteTutorial();
    }
    
    /// <summary>
    /// Complete the tutorial
    /// </summary>
    private void CompleteTutorial()
    {
        tutorialActive = false;
        tutorialCompleted = true;
        
        // Stop tutorial coroutine
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        // Clear highlights
        ClearHighlights();
        
        // Hide tutorial panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // Resume the game
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
        
        // Save completion status
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
        
        OnTutorialCompleted?.Invoke();
        
        Debug.Log("Tutorial completed!");
    }
    
    /// <summary>
    /// Reset tutorial (for testing)
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        tutorialCompleted = false;
        currentStepIndex = 0;
    }
    
    /// <summary>
    /// Add a tutorial step
    /// </summary>
    public void AddTutorialStep(TutorialStep step)
    {
        tutorialSteps.Add(step);
    }
    
    /// <summary>
    /// Remove a tutorial step
    /// </summary>
    public void RemoveTutorialStep(int index)
    {
        if (index >= 0 && index < tutorialSteps.Count)
        {
            tutorialSteps.RemoveAt(index);
        }
    }
    
    /// <summary>
    /// Get tutorial progress (0-1)
    /// </summary>
    public float GetTutorialProgress()
    {
        if (tutorialSteps.Count == 0) return 1f;
        return (float)currentStepIndex / tutorialSteps.Count;
    }
    
    void OnDestroy()
    {
        // Clean up any running coroutines
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        // Clean up tweens
        if (highlightOverlay != null)
        {
            highlightOverlay.DOKill();
        }
        
        if (pointer != null)
        {
            pointer.DOKill();
        }
    }
}

/// <summary>
/// Represents a single tutorial step
/// </summary>
[System.Serializable]
public class TutorialStep
{
    [Header("Step Info")]
    public string stepName;
    [TextArea(3, 5)]
    public string instructionText;
    public TutorialStepType stepType;
    public float duration = 3f;
    
    [Header("Target Element")]
    public Transform targetElement;
    
    [Header("Custom Settings")]
    public bool waitForUserInput = true;
    public string triggerEvent = "";
}

/// <summary>
/// Types of tutorial steps
/// </summary>
public enum TutorialStepType
{
    ShowText,        // Just show instruction text
    HighlightElement, // Highlight a UI element
    WaitForAction,   // Wait for player to perform an action
    PlayAnimation    // Play a custom animation
}