using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Malzeme ma�azas� ana kontrolc� - basit ve efektif
/// </summary>
public class MaterialShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI moneyText;

    // References
    private MaterialStockManager stockManager;
    private PizzaOrderManager pizzaOrderManager;

    void Start()
    {
        stockManager = FindFirstObjectByType<MaterialStockManager>();
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();

        if (stockManager != null)
            stockManager.OnPurchaseResult += ShowMessage;

        if (pizzaOrderManager != null)
            pizzaOrderManager.OnMoneyChanged += UpdateMoneyDisplay;

        // �lk para g�sterimi
        UpdateMoneyDisplay(pizzaOrderManager?.TotalMoney ?? 0);

        // Mesaj alan�n� gizle
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
            moneyText.text = $"Para: ${money}";
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = message;

            // Mesaj� 2 saniye g�ster
            messageText.DOFade(1f, 0.2f).OnComplete(() =>
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    messageText.DOFade(0f, 0.3f).OnComplete(() =>
                        messageText.gameObject.SetActive(false));
                });
            });
        }
    }

    public void ToggleShop()
    {
        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);

            if (!isActive) // A��l�yorsa
            {
                shopPanel.transform.localScale = Vector3.zero;
                shopPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        }
    }

    void OnDestroy()
    {
        if (stockManager != null)
            stockManager.OnPurchaseResult -= ShowMessage;

        if (pizzaOrderManager != null)
            pizzaOrderManager.OnMoneyChanged -= UpdateMoneyDisplay;
    }
}