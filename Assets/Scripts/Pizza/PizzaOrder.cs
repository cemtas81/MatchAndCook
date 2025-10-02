using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a pizza order with specific ingredient requirements.
/// Each order corresponds to one level/customer interaction.
/// </summary>
[CreateAssetMenu(fileName = "New Pizza Order", menuName = "Match & Cook/Pizza Order")]
public class PizzaOrder : ScriptableObject
{
    [Header("Customer Info")]
    public string customerName;
    [TextArea(2, 3)]
    public string orderDescription; // e.g., "I want a delicious Margherita pizza!"
    public Sprite customerAvatar;

    [Header("Pizza Requirements")]
    public string pizzaName; // e.g., "Margherita", "Pepperoni Supreme"
    public List<PizzaIngredientRequirement> requiredIngredients = new List<PizzaIngredientRequirement>();

    [Header("Timing & Difficulty")]
    public float timeLimit = 120f; // Time in seconds to complete the order
    public int difficultyLevel = 1; // Used to determine which orders appear at which levels
    public int baseReward = 200;
    public int timeBonus = 100; // Bonus for completing quickly

    [Header("Money System")]
    public int price = 50; // Money earned when completing this order

    [Header("Customer Personality")]
    public float patienceLevel = 1f; // Multiplier for time limit
    public Color customerMoodColor = Color.white;

    /// <summary>
    /// Calculate the total reward based on completion time
    /// </summary>
    public int CalculateReward(float completionTime)
    {
        if (completionTime > timeLimit) return baseReward / 2; // Penalty for overtime

        float timeRatio = 1f - (completionTime / timeLimit);
        int reward = baseReward + Mathf.RoundToInt(timeBonus * timeRatio);

        return reward;
    }

    /// <summary>
    /// Get completion progress (0-1) based on collected ingredients
    /// </summary>
    public float GetCompletionProgress(Dictionary<Tile.TileType, int> collectedIngredients)
    {
        if (requiredIngredients.Count == 0) return 1f;

        float totalProgress = 0f;
        foreach (var requirement in requiredIngredients)
        {
            int collected = collectedIngredients.ContainsKey(requirement.ingredientType)
                ? collectedIngredients[requirement.ingredientType] : 0;
            float ingredientProgress = Mathf.Clamp01((float)collected / requirement.requiredAmount);
            totalProgress += ingredientProgress;
        }

        return totalProgress / requiredIngredients.Count;
    }

    /// <summary>
    /// Check if all ingredients have been collected
    /// </summary>
    public bool IsCompleted(Dictionary<Tile.TileType, int> collectedIngredients)
    {
        foreach (var requirement in requiredIngredients)
        {
            int collected = collectedIngredients.ContainsKey(requirement.ingredientType)
                ? collectedIngredients[requirement.ingredientType] : 0;
            if (collected < requirement.requiredAmount)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get customer satisfaction based on remaining time (0-1)
    /// </summary>
    public float GetCustomerSatisfaction(float remainingTime)
    {
        return Mathf.Clamp01(remainingTime / timeLimit);
    }
}

/// <summary>
/// Represents an ingredient requirement for a pizza order
/// </summary>
[System.Serializable]
public class PizzaIngredientRequirement
{
    [Header("Ingredient Details")]
    public Tile.TileType ingredientType; // Which pizza ingredient is needed
    public int requiredAmount; // How many matches are needed
    //public string ingredientDisplayName; // User-friendly name (e.g., "Fresh Tomatoes")

    //[Header("Visual Settings")]
    //public Sprite ingredientIcon; // Icon to show in UI
    //public Color ingredientColor = Color.white; // Color for progress indicators

    public PizzaIngredientRequirement(Tile.TileType type, int amount, string displayName)
    {
        ingredientType = type;
        requiredAmount = amount;
        //ingredientDisplayName = displayName;
    }
}