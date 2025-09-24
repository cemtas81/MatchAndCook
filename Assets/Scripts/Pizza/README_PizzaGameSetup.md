# Pizza Match-3 Game Setup Guide

This guide explains how to set up the pizza-themed Match-3 game with the new components.

## Quick Setup Steps

1. **Create a new scene or use existing Match-3 scene**

2. **Add core game objects:**
   - Empty GameObject named "GameManager" with `GameManager.cs`
   - Empty GameObject named "PizzaOrderManager" with `PizzaOrderManager.cs`
   - Empty GameObject named "GridManager" with `GridManager.cs`
   - Empty GameObject named "PowerUpManager" with `PowerUpManager.cs`
   - Canvas with "PizzaSliderUI" object containing `PizzaSliderUI.cs`

3. **Add the master controller:**
   - Empty GameObject named "PizzaGameController" with `PizzaGameController.cs`
   - Enable "Auto Find Components" and "Initialize With Sample Orders"

4. **Mobile optimization:**
   - Add `MobileOptimization.cs` to any GameObject (or GameManager)

## Component Configuration

### GridManager
- **Tile Prefab**: Create a prefab with a GameObject containing:
  - `Tile.cs` script
  - `SpriteRenderer` component
  - `BoxCollider2D` for touch input
- **Grid Size**: Recommended 8x8 for mobile
- **Tile Spacing**: 1.1f for proper spacing

### PizzaOrderManager
- Will automatically use sample pizza orders if none are configured
- Sample orders include: Margherita, Pepperoni, Veggie Supreme, Deluxe

### PizzaSliderUI (Canvas UI)
- **Pizza Progress Ring**: Image with "Filled" type for circular progress
- **Pizza Icon**: Center image showing current pizza
- **Customer Avatar**: Image in top-right corner for customer
- **Timer Text**: TextMeshPro for remaining time display
- **Progress Text**: TextMeshPro showing percentage completion

### Power-ups Available
- **Pizza Oven Bomb**: Destroys 3x3 area
- **Magic Spatula**: Converts tiles to needed ingredients  
- **Extra Time**: Adds 20 seconds to current order
- **Rainbow Ingredient**: Matches with any ingredient
- **Chef's Special**: Doubles score for next moves
- **Extra Energy**: Adds extra moves

## Pizza Ingredients

The game uses 7 pizza ingredient types:
- **Tomato**: Red tomatoes and sauce
- **Cheese**: Yellow mozzarella and cheddar
- **Pepperoni**: Red-orange pepperoni slices
- **Mushroom**: Brown mushrooms
- **Pepper**: Green bell peppers
- **Onion**: Purple-white onions
- **Olives**: Dark black/green olives

## Game Flow

1. **Level Start**: Customer appears with pizza order
2. **Ingredient Matching**: Player matches ingredients on grid
3. **Progress Tracking**: Circular progress bar fills as ingredients are collected
4. **Order Completion**: When all ingredients collected, pizza completes
5. **Level Advance**: New customer appears with more complex order
6. **Difficulty Scaling**: Time limits decrease and ingredient requirements increase

## Testing

Use the `PizzaGameController` context menu options:
- **"Quick Setup for Testing"**: Auto-configure for immediate testing
- **"Log Game Status"**: Check current game state in console

## Mobile Optimization

The `MobileOptimization.cs` script automatically:
- Sets 60 FPS target frame rate
- Disables shadows and expensive effects
- Enables multi-touch for ingredient swapping
- Prevents screen dimming during gameplay
- Optimizes for portrait and landscape orientations

## Troubleshooting

**No pizza orders appearing?**
- Check that `PizzaOrderManager` has "Initialize With Sample Orders" enabled
- Verify `PizzaGameController` is in the scene

**Ingredients not spawning correctly?**
- Ensure `GridManager` has reference to `PizzaOrderManager`
- Check that tile prefab has `Tile.cs` script attached

**UI not updating?**
- Verify `PizzaSliderUI` components are properly assigned
- Check that UI Canvas is set to "Screen Space - Overlay"

**Performance issues?**
- Enable "Performance Mode" in `MobileOptimization`
- Reduce grid size to 6x6 for lower-end devices
- Check that DOTween is properly imported for animations

## Integration with Existing Systems

The pizza system is designed to work alongside existing components:
- Maintains compatibility with original `CustomerManager` and `RecipeManager`
- Extends `GameManager` with pizza-specific scoring
- Enhances `PowerUpManager` with pizza-themed effects
- Works with existing `UIManager` for general UI elements

This modular approach allows gradual migration or hybrid gameplay modes.