using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI successfulOrdersText;
    [SerializeField] private TextMeshProUGUI failedOrdersText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject panel;

    private SessionManager sessionManager;
   

    void Awake()
    {
        panel.SetActive(false);
    }

    void Start()
    {
        sessionManager = FindFirstObjectByType<SessionManager>();

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (sessionManager != null)
        {
            sessionManager.OnSessionCompleted += ShowSessionResults;
        }
    }

    private void ShowSessionResults(int sessionNumber)
    {
        
        if (titleText != null)
            titleText.text = $"Session {sessionNumber} Tamamlandý!";

        if (successfulOrdersText != null && sessionManager != null)
            successfulOrdersText.text = $"Baþarýlý Sipariþler: {sessionManager.PizzasCompletedInSession}";

        if (failedOrdersText != null && sessionManager != null)
            failedOrdersText.text = $"Baþarýsýz Sipariþler: {sessionManager.FailedOrdersInSession}";

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnContinueClicked()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        
        if (sessionManager != null)
        {
            sessionManager.ContinueToNextSession();
        }
    }

    void OnDestroy()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionCompleted -= ShowSessionResults;
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}