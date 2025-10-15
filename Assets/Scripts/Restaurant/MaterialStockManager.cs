using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Basit ve efektif malzeme sto�u ve ticaret sistemi
/// Her malzemeden ba�lang��ta 10 birim, 10'ar birim sat�n alma
/// 8 malzeme: Tomato, Cheese, Butter, Pepperoni, Mushroom, Pepper, Onion, Olives
/// </summary>
public class MaterialStockManager : MonoBehaviour
{
    [Header("Stock Settings")]
    [SerializeField] private int initialStock = 10;
    [SerializeField] private int purchaseAmount = 10;

    [Header("Material Prices - 8 Ingredients")]
    [SerializeField] private int tomatoPrice = 50;
    [SerializeField] private int cheesePrice = 75;
    [SerializeField] private int butterPrice = 100;
    [SerializeField] private int pepperoniPrice = 125;
    [SerializeField] private int mushroomPrice = 150;
    [SerializeField] private int pepperPrice = 60;     // YEN�
    [SerializeField] private int onionPrice = 80;      // YEN�  
    [SerializeField] private int olivesPrice = 120;    // YEN�

    // Stok durumu
    private Dictionary<Tile.TileType, int> materialStock = new Dictionary<Tile.TileType, int>();
    private Dictionary<Tile.TileType, int> materialPrices = new Dictionary<Tile.TileType, int>();
    // Sat�n alma takibi i�in
    private Dictionary<Tile.TileType, int> purchaseCount = new Dictionary<Tile.TileType, int>();

    // Events
    public System.Action<Tile.TileType, int> OnStockChanged;
    public System.Action<string> OnPurchaseResult; // Ba�ar�/hata mesaj�

    // References
    private PizzaOrderManager pizzaOrderManager;

    void Awake()
    {
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        InitializeStock();
        InitializePrices();
    }

    /// <summary>
    /// Ba�lang�� sto�unu ayarla - sadece 8 malzeme i�in
    /// </summary>
    private void InitializeStock()
    {
        // Sadece pizza malzemeleri (8 adet)
        materialStock[Tile.TileType.Tomato] = initialStock;
        materialStock[Tile.TileType.Cheese] = initialStock;
        materialStock[Tile.TileType.Butter] = initialStock;
        materialStock[Tile.TileType.Pepperoni] = initialStock;
        materialStock[Tile.TileType.Mushroom] = initialStock;
        materialStock[Tile.TileType.Pepper] = initialStock;
        materialStock[Tile.TileType.Onion] = initialStock;
        materialStock[Tile.TileType.Olives] = initialStock;

        // Sat�n alma saya�lar�n� ba�lat
        purchaseCount[Tile.TileType.Tomato] = 0;
        purchaseCount[Tile.TileType.Cheese] = 0;
        purchaseCount[Tile.TileType.Butter] = 0;
        purchaseCount[Tile.TileType.Pepperoni] = 0;
        purchaseCount[Tile.TileType.Mushroom] = 0;
        purchaseCount[Tile.TileType.Pepper] = 0;
        purchaseCount[Tile.TileType.Onion] = 0;
        purchaseCount[Tile.TileType.Olives] = 0;
    }

    /// <summary>
    /// Malzeme fiyatlar�n� ayarla - 8 malzeme
    /// </summary>
    private void InitializePrices()
    {
        materialPrices[Tile.TileType.Tomato] = tomatoPrice;
        materialPrices[Tile.TileType.Cheese] = cheesePrice;
        materialPrices[Tile.TileType.Butter] = butterPrice;
        materialPrices[Tile.TileType.Pepperoni] = pepperoniPrice;
        materialPrices[Tile.TileType.Mushroom] = mushroomPrice;
        materialPrices[Tile.TileType.Pepper] = pepperPrice;     // YEN�
        materialPrices[Tile.TileType.Onion] = onionPrice;       // YEN�
        materialPrices[Tile.TileType.Olives] = olivesPrice;     // YEN�
    }

    /// <summary>
    /// Malzeme sat�n al
    /// </summary>
    public void PurchaseMaterial(Tile.TileType materialType)
    {
        if (!materialPrices.ContainsKey(materialType)) return;

        int price = materialPrices[materialType];
        int currentMoney = pizzaOrderManager?.TotalMoney ?? 0;

        if (currentMoney >= price)
        {
            // Para d��
            pizzaOrderManager.SpendMoney(price);
            
            // Sat�n alma sayac�n� art�r ve stok ekle
            purchaseCount[materialType]++;
            materialStock[materialType] += purchaseAmount;
            
            OnStockChanged?.Invoke(materialType, materialStock[materialType]);
            OnPurchaseResult?.Invoke($"+{purchaseAmount} {materialType} sat�n al�nd�!");
        }
        else
        {
            OnPurchaseResult?.Invoke("Yetersiz para!");
        }
    }

    /// <summary>
    /// Stok kontrol� - sipari� yap�labilir mi?
    /// </summary>
    public bool CanFulfillOrder(PizzaOrder order)
    {
        if (order?.requiredIngredients == null) return false;

        foreach (var requirement in order.requiredIngredients)
        {
            int currentStock = GetStock(requirement.ingredientType);
            if (currentStock < requirement.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>               
    /// Sipari� i�in malzeme harca
    /// </summary>
    public void ConsumeIngredients(PizzaOrder order)
    {
        if (!CanFulfillOrder(order)) return;

        foreach (var requirement in order.requiredIngredients)
        {
            materialStock[requirement.ingredientType] -= requirement.requiredAmount;
            OnStockChanged?.Invoke(requirement.ingredientType, materialStock[requirement.ingredientType]);
        }
    }

    /// <summary>
    /// Malzeme sto�unu getir
    /// </summary>
    public int GetStock(Tile.TileType materialType)
    {
        return materialStock.ContainsKey(materialType) ? materialStock[materialType] : 0;
    }

    /// <summary>
    /// Malzeme fiyat�n� getir
    /// </summary>
    public int GetPrice(Tile.TileType materialType)
    {
        return materialPrices.ContainsKey(materialType) ? materialPrices[materialType] : 100;
    }

    /// <summary>
    /// T�m stok durumunu getir (sadece pizza malzemeleri)
    /// </summary>
    public Dictionary<Tile.TileType, int> GetAllStock()
    {
        return new Dictionary<Tile.TileType, int>(materialStock);
    }

    /// <summary>
    /// Pizza malzemesi mi kontrol et
    /// </summary>
    public bool IsPizzaIngredient(Tile.TileType materialType)
    {
        return materialStock.ContainsKey(materialType);
    }

    /// <summary>
    /// T�m pizza malzemelerini getir (8 adet)
    /// </summary>
    public List<Tile.TileType> GetAllPizzaIngredients()
    {
        return new List<Tile.TileType>
        {
            Tile.TileType.Tomato,
            Tile.TileType.Cheese,
            Tile.TileType.Butter,
            Tile.TileType.Pepperoni,
            Tile.TileType.Mushroom,
            Tile.TileType.Pepper,
            Tile.TileType.Onion,
            Tile.TileType.Olives
        };
    }

    // Sat�n alma �zetini g�ster
    private void LogPurchaseSummary()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.AppendLine("---- MALZEME SATIN ALMA �ZET� ----");
        
        int totalItems = 0;
        int totalSpent = 0;
        
        foreach (var item in purchaseCount)
        {
            if (item.Value > 0)
            {
                int itemCost = item.Value * materialPrices[item.Key];
                totalSpent += itemCost;
                totalItems += item.Value;
                builder.AppendLine($"{item.Key}: {item.Value} kez sat�n al�nd� (her biri {materialPrices[item.Key]}$ - toplam {itemCost}$)");
            }
        }
        
        builder.AppendLine($"TOPLAM: {totalItems} sat�n al�m, {totalSpent}$ harcand�");
        builder.AppendLine("----------------------------------");
        
        Debug.Log(builder.ToString());
    }

    // Toplam sat�n alma maliyetini hesapla
    public int GetTotalPurchaseCost()
    {
        int total = 0;
        foreach (var type in purchaseCount.Keys)
        {
            total += purchaseCount[type] * materialPrices[type];
        }
        return total;
    }
}