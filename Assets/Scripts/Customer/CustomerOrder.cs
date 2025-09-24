using UnityEngine;

/// <summary>
/// Represents a customer order with recipe requirements and time limits.
/// Used to create time-based challenges in the cooking game.
/// </summary>
[CreateAssetMenu(fileName = "New Customer Order", menuName = "Match & Cook/Customer Order")]
public class CustomerOrder : ScriptableObject
{
    [Header("Customer Info")]
    public string customerName;
    [TextArea(2, 3)]
    public string orderDescription;
    public Sprite customerAvatar;
    
    [Header("Order Details")]
    public Recipe requestedRecipe;
    public float timeLimit = 60f; // Time in seconds
    public int baseReward = 150;
    public int timeBonus = 50; // Bonus for completing quickly
    
    [Header("Customer Patience")]
    public float patienceDecayRate = 1f; // How fast patience decreases
    public Color happyColor = Color.green;
    public Color neutralColor = Color.yellow;
    public Color angryColor = Color.red;
    
    [Header("Difficulty")]
    public OrderDifficulty difficulty = OrderDifficulty.Easy;
    public int minimumLevel = 1;
    
    public enum OrderDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    /// <summary>
    /// Calculate reward based on completion time
    /// </summary>
    public int CalculateReward(float completionTime)
    {
        if (completionTime > timeLimit) return 0; // Late completion = no reward
        
        float timeRatio = 1f - (completionTime / timeLimit);
        int reward = baseReward + Mathf.RoundToInt(timeBonus * timeRatio);
        
        return reward;
    }
    
    /// <summary>
    /// Get customer satisfaction level based on remaining time
    /// </summary>
    public float GetSatisfactionLevel(float remainingTime)
    {
        return Mathf.Clamp01(remainingTime / timeLimit);
    }
    
    /// <summary>
    /// Get customer mood color based on remaining time
    /// </summary>
    public Color GetCustomerMoodColor(float remainingTime)
    {
        float satisfaction = GetSatisfactionLevel(remainingTime);
        
        if (satisfaction > 0.7f)
            return happyColor;
        else if (satisfaction > 0.3f)
            return Color.Lerp(angryColor, neutralColor, (satisfaction - 0.3f) / 0.4f);
        else
            return angryColor;
    }
}