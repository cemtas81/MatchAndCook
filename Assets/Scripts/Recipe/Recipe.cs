using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a cooking recipe with required ingredients and rewards.
/// Used to create cooking objectives in the match-3 game.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Match & Cook/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    [TextArea(2, 4)]
    public string description;
    public Sprite recipeIcon;
    
    [Header("Required Ingredients")]
    public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();
    
    [Header("Rewards")]
    public int baseScore = 100;
    public int bonusScore = 50;
    public float cookingAnimationDuration = 2f;
    
    [Header("Difficulty")]
    public RecipeDifficulty difficulty = RecipeDifficulty.Easy;
    public int recommendedLevel = 1;
    
    public enum RecipeDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    /// <summary>
    /// Check if recipe is completed based on collected ingredients
    /// </summary>
    public bool IsCompleted(Dictionary<Tile.TileType, int> collectedIngredients)
    {
        foreach (var requirement in requiredIngredients)
        {
            if (!collectedIngredients.ContainsKey(requirement.ingredientType) ||
                collectedIngredients[requirement.ingredientType] < requirement.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get completion progress as percentage
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
    /// Get total score for completing this recipe
    /// </summary>
    public int GetTotalReward()
    {
        return baseScore + bonusScore;
    }
}

/// <summary>
/// Represents an ingredient requirement for a recipe
/// </summary>
[System.Serializable]
public class IngredientRequirement
{
    [Header("Ingredient")]
    public Tile.TileType ingredientType;
    public int requiredAmount;
    public string ingredientName;
    
    [Header("Visual")]
    public Sprite ingredientIcon;
    public Color ingredientColor = Color.white;
    
    public IngredientRequirement(Tile.TileType type, int amount, string name)
    {
        ingredientType = type;
        requiredAmount = amount;
        ingredientName = name;
    }
}