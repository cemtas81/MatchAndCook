using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Basit ve efektif malzeme stoðu ve ticaret sistemi
/// Her malzemeden baþlangýçta 10 birim, 10'ar birim satýn alma
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
    [SerializeField] private int pepperPrice = 60;     // YENÝ
    [SerializeField] private int onionPrice = 80;      // YENÝ  
    [SerializeField] private int olivesPrice = 120;    // YENÝ

    // Stok durumu
    private Dictionary<Tile.TileType, int> materialStock = new Dictionary<Tile.TileType, int>();
    private Dictionary<Tile.TileType, int> materialPrices = new Dictionary<Tile.TileType, int>();
    // Satýn alma takibi için
    private Dictionary<Tile.TileType, int> purchaseCount = new Dictionary<Tile.TileType, int>();

    // Events
    public System.Action<Tile.TileType, int> OnStockChanged;
    public System.Action<string> OnPurchaseResult; // Baþarý/hata mesajý

    // References
    private PizzaOrderManager pizzaOrderManager;

    void Awake()
    {
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        InitializeStock();
        InitializePrices();
    }

    /// <summary>
    /// Baþlangýç stoðunu ayarla - sadece 8 malzeme için
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

        // Satýn alma sayaçlarýný baþlat
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
    /// Malzeme fiyatlarýný ayarla - 8 malzeme
    /// </summary>
    private void InitializePrices()
    {
        materialPrices[Tile.TileType.Tomato] = tomatoPrice;
        materialPrices[Tile.TileType.Cheese] = cheesePrice;
        materialPrices[Tile.TileType.Butter] = butterPrice;
        materialPrices[Tile.TileType.Pepperoni] = pepperoniPrice;
        materialPrices[Tile.TileType.Mushroom] = mushroomPrice;
        materialPrices[Tile.TileType.Pepper] = pepperPrice;     // YENÝ
        materialPrices[Tile.TileType.Onion] = onionPrice;       // YENÝ
        materialPrices[Tile.TileType.Olives] = olivesPrice;     // YENÝ
    }

    /// <summary>
    /// Malzeme satýn al
    /// </summary>
    public void PurchaseMaterial(Tile.TileType materialType)
    {
        if (!materialPrices.ContainsKey(materialType)) return;

        int price = materialPrices[materialType];
        int currentMoney = pizzaOrderManager?.TotalMoney ?? 0;
        
        Debug.Log($"Satýn alýnýyor: {materialType}, Fiyat: {price}, Mevcut Para: {currentMoney}");

        if (currentMoney >= price)
        {
            int previousMoney = currentMoney;
            
            // Para düþ
            pizzaOrderManager.SpendMoney(price);
            int moneySpent = previousMoney - pizzaOrderManager.TotalMoney;
            
            Debug.Log($"Para düþüldü. Fark: {moneySpent} (Beklenen: {price}), Yeni para: {pizzaOrderManager.TotalMoney}");
            
            if (moneySpent != price) {
                Debug.LogWarning($"UYARI: {materialType} için harcanmasý gereken para ({price}) ile düþülen para ({moneySpent}) farklý!");
            }
            
            // Satýn alma sayacýný artýr
            purchaseCount[materialType]++;
            
            // Stok ekle
            materialStock[materialType] += purchaseAmount;
            OnStockChanged?.Invoke(materialType, materialStock[materialType]);
            OnPurchaseResult?.Invoke($"+{purchaseAmount} {materialType} satýn alýndý!");
            
            // Satýn alma özeti
            LogPurchaseSummary();
        }
        else
        {
            OnPurchaseResult?.Invoke("Yetersiz para!");
        }
    }

    /// <summary>
    /// Stok kontrolü - sipariþ yapýlabilir mi?
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
    /// Sipariþ için malzeme harca
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
    /// Malzeme stoðunu getir
    /// </summary>
    public int GetStock(Tile.TileType materialType)
    {
        return materialStock.ContainsKey(materialType) ? materialStock[materialType] : 0;
    }

    /// <summary>
    /// Malzeme fiyatýný getir
    /// </summary>
    public int GetPrice(Tile.TileType materialType)
    {
        return materialPrices.ContainsKey(materialType) ? materialPrices[materialType] : 100;
    }

    /// <summary>
    /// Tüm stok durumunu getir (sadece pizza malzemeleri)
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
    /// Tüm pizza malzemelerini getir (8 adet)
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

    // Satýn alma özetini göster
    private void LogPurchaseSummary()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.AppendLine("---- MALZEME SATIN ALMA ÖZETÝ ----");
        
        foreach (var item in purchaseCount)
        {
            if (item.Value > 0)
            {
                int totalCost = item.Value * materialPrices[item.Key];
                builder.AppendLine($"{item.Key}: {item.Value} kez satýn alýndý (her biri {materialPrices[item.Key]}$ - toplam {totalCost}$)");
            }
        }
        
        int totalItems = purchaseCount.Values.Sum();
        int totalSpent = 0;
        
        foreach (var type in purchaseCount.Keys)
        {
            totalSpent += purchaseCount[type] * materialPrices[type];
        }
        
        builder.AppendLine($"TOPLAM: {totalItems} satýn alým, {totalSpent}$ harcandý");
        builder.AppendLine("----------------------------------");
        
        Debug.Log(builder.ToString());
    }

    // Toplam satýn alma maliyetini hesapla
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