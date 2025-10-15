using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tek malzeme butonu - basit ve efektif
/// </summary>
public class MaterialButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI stockText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image materialIcon;

    [Header("Material Settings")]
    [SerializeField] private Tile.TileType materialType;

    // References
    private MaterialStockManager stockManager;
    private PizzaOrderManager pizzaOrderManager;

    void Start()
    {
        stockManager = FindFirstObjectByType<MaterialStockManager>();
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();

        // Button event
        purchaseButton.onClick.AddListener(PurchaseMaterial);

        // Events
        if (stockManager != null)
            stockManager.OnStockChanged += OnStockChanged;

        if (pizzaOrderManager != null)
            pizzaOrderManager.OnMoneyChanged += OnMoneyChanged;

        // Ýlk güncelleme
        UpdateUI();
    }

    private void PurchaseMaterial()
    {
        stockManager?.PurchaseMaterial(materialType);
    }

    private void OnStockChanged(Tile.TileType type, int newStock)
    {
        if (type == materialType)
            UpdateUI();
    }

    private void OnMoneyChanged(int newMoney)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (stockManager == null || pizzaOrderManager == null) return;

        // Stok göster
        int stock = stockManager.GetStock(materialType);
        stockText.text = $"Stok: {stock}";

        // Fiyat göster
        int price = stockManager.GetPrice(materialType);
        priceText.text = $"${price}";

        // Butonu aktif/pasif yap
        int currentMoney = pizzaOrderManager.TotalMoney;
        purchaseButton.interactable = currentMoney >= price;

        // Renk deðiþtir (yeterli para var mý?)
        purchaseButton.image.color = purchaseButton.interactable ? Color.white : Color.gray;
    }

    void OnDestroy()
    {
        if (stockManager != null)
            stockManager.OnStockChanged -= OnStockChanged;

        if (pizzaOrderManager != null)
            pizzaOrderManager.OnMoneyChanged -= OnMoneyChanged;
    }
}