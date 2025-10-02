using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class to create sample pizza orders for testing and demonstration.
/// This would typically be replaced by ScriptableObject assets in the final game.
/// </summary>
public static class SamplePizzaOrders
{
    /// <summary>
    /// Create a simple Margherita pizza order for early levels
    /// DIFFICULTY: 2 tile types (easiest for beginners)
    /// </summary>
    public static PizzaOrder CreateMargheritaPizza()
    {
        var order = ScriptableObject.CreateInstance<PizzaOrder>();
        order.customerName = "Marco";
        order.orderDescription = "I'd like a classic Margherita pizza, please!";
        order.pizzaName = "Margherita Pizza";
        order.timeLimit = 90f;
        order.difficultyLevel = 1;
        order.baseReward = 150;
        order.timeBonus = 75;
        
        order.requiredIngredients = new List<PizzaIngredientRequirement>
        {
            new PizzaIngredientRequirement(Tile.TileType.Tomato, 5, "Fresh Tomatoes"),
            new PizzaIngredientRequirement(Tile.TileType.Cheese, 4, "Mozzarella Cheese")
        };
        
        return order;
    }
    
    /// <summary>
    /// Create a Pepperoni pizza order for mid-level gameplay
    /// DIFFICULTY: 3 tile types (moderate difficulty)
    /// </summary>
    public static PizzaOrder CreatePepperoniPizza()
    {
        var order = ScriptableObject.CreateInstance<PizzaOrder>();
        order.customerName = "Sarah";
        order.orderDescription = "I'm craving a pepperoni pizza with extra cheese!";
        order.pizzaName = "Pepperoni Pizza";
        order.timeLimit = 75f;
        order.difficultyLevel = 2;
        order.baseReward = 200;
        order.timeBonus = 100;
        
        order.requiredIngredients = new List<PizzaIngredientRequirement>
        {
            new PizzaIngredientRequirement(Tile.TileType.Tomato, 4, "Tomato Sauce"),
            new PizzaIngredientRequirement(Tile.TileType.Cheese, 5, "Extra Cheese"),
            new PizzaIngredientRequirement(Tile.TileType.Pepperoni, 6, "Pepperoni Slices")
        };
        
        return order;
    }
    
    /// <summary>
    /// Create a Veggie Supreme pizza order for higher level gameplay
    /// DIFFICULTY: 5 tile types (harder with more variety)
    /// </summary>
    public static PizzaOrder CreateVeggieSupreme()
    {
        var order = ScriptableObject.CreateInstance<PizzaOrder>();
        order.customerName = "Jennifer";
        order.orderDescription = "I want a healthy veggie pizza with lots of fresh ingredients!";
        order.pizzaName = "Veggie Supreme";
        order.timeLimit = 60f;
        order.difficultyLevel = 3;
        order.baseReward = 250;
        order.timeBonus = 125;
        
        order.requiredIngredients = new List<PizzaIngredientRequirement>
        {
            new PizzaIngredientRequirement(Tile.TileType.Tomato, 3, "Tomato Base"),
            new PizzaIngredientRequirement(Tile.TileType.Cheese, 4, "Cheese"),
            new PizzaIngredientRequirement(Tile.TileType.Pepper, 5, "Bell Peppers"),
            new PizzaIngredientRequirement(Tile.TileType.Mushroom, 4, "Fresh Mushrooms"),
            new PizzaIngredientRequirement(Tile.TileType.Onion, 3, "Red Onions")
        };
        
        return order;
    }
    
    /// <summary>
    /// Create a Deluxe pizza order for expert level gameplay
    /// DIFFICULTY: 7 tile types (maximum difficulty with all ingredients)
    /// </summary>
    public static PizzaOrder CreateDeluxePizza()
    {
        var order = ScriptableObject.CreateInstance<PizzaOrder>();
        order.customerName = "Antonio";
        order.orderDescription = "Give me everything! The ultimate deluxe pizza experience!";
        order.pizzaName = "Deluxe Supreme";
        order.timeLimit = 45f;
        order.difficultyLevel = 5;
        order.baseReward = 350;
        order.timeBonus = 175;
        
        order.requiredIngredients = new List<PizzaIngredientRequirement>
        {
            new PizzaIngredientRequirement(Tile.TileType.Tomato, 4, "Tomato Sauce"),
            new PizzaIngredientRequirement(Tile.TileType.Cheese, 6, "Triple Cheese"),
            new PizzaIngredientRequirement(Tile.TileType.Pepperoni, 5, "Pepperoni"),
            new PizzaIngredientRequirement(Tile.TileType.Mushroom, 4, "Mushrooms"),
            new PizzaIngredientRequirement(Tile.TileType.Pepper, 3, "Bell Peppers"),
            new PizzaIngredientRequirement(Tile.TileType.Onion, 3, "Onions"),
            new PizzaIngredientRequirement(Tile.TileType.Olives, 4, "Black Olives")
        };
        
        return order;
    }
    
    /// <summary>
    /// Get a list of all sample pizza orders for testing
    /// </summary>
    public static List<PizzaOrder> GetAllSampleOrders()
    {
        return new List<PizzaOrder>
        {
            CreateMargheritaPizza(),
            CreatePepperoniPizza(),
            CreateVeggieSupreme(),
            CreateDeluxePizza()
        };
    }
    
    /// <summary>
    /// Get orders appropriate for a specific difficulty level
    /// </summary>
    public static List<PizzaOrder> GetOrdersForLevel(int level)
    {
        List<PizzaOrder> appropriateOrders = new List<PizzaOrder>();
        List<PizzaOrder> allOrders = GetAllSampleOrders();
        
        foreach (var order in allOrders)
        {
            if (order.difficultyLevel <= level)
            {
                appropriateOrders.Add(order);
            }
        }
        
        return appropriateOrders.Count > 0 ? appropriateOrders : new List<PizzaOrder> { CreateMargheritaPizza() };
    }
}