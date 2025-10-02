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
- **Power-Up Tile Prefab**: (Optional) Separate prefab for power-up tiles with distinct visuals
  - Same components as regular tile prefab
  - Can use different sprites or materials for visual distinction
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

## Increasing Difficulty System

The game now features a progressive difficulty system:

### Tile Variety Scaling
- **Level 1-2**: 2-3 tile types (Margherita: Tomato + Cheese)
- **Level 3-4**: 3-4 tile types (Pepperoni: Tomato + Cheese + Pepperoni)
- **Level 5-6**: 5 tile types (Veggie Supreme)
- **Level 7+**: Up to 7 tile types (Deluxe Supreme with all ingredients)

The grid automatically limits tile variety to match the current pizza order's required ingredients, making early levels easier and later levels progressively harder.

### Time Pressure
- Each level applies time scaling: `timeLimit * (0.9 ^ (level - 1))`
- Minimum time limit: 30 seconds
- Early levels: 90 seconds
- Later levels: As low as 45 seconds

## Real Power-Up System

### Power-Up Creation
Power-ups are created automatically during match-3 combos:
- **4-tile match**: Creates Bomb or Lightning power-up at pivot position
- **5-tile match**: Creates Rainbow power-up
- **6+ tile match**: Creates Star power-up

### Power-Up Activation
Click/tap on a power-up tile to activate its special effect:

1. **Bomb Power-Up** (Dark Gray)
   - Destroys all tiles in a 3x3 area around the bomb
   - Created from 4-tile matches

2. **Lightning Power-Up** (Bright Yellow)
   - Destroys an entire row OR column
   - Created from 4-tile horizontal/vertical matches
   - Direction determined randomly on activation

3. **Star Power-Up** (Orange)
   - Destroys an entire column
   - Created from 6+ tile matches

4. **Rainbow Power-Up** (White/Rainbow)
   - Destroys all tiles of one random ingredient type on the grid
   - Created from 5-tile matches
   - Most powerful for clearing specific ingredients

### Visual Effects
- Power-up creation: Scale animation with elastic bounce
- Idle state: Continuous pulsing animation (scale 1.0-1.1)
- Activation: DOTween animations + particle effects
- Destruction: Scale down + fade out animations

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
3. **Combo Power-Ups**: 4+ matches create special power-up tiles
4. **Power-Up Usage**: Click power-ups to activate special effects
5. **Progress Tracking**: Circular progress bar fills as ingredients are collected
6. **Order Completion**: When all ingredients collected, pizza completes
7. **Level Advance**: New customer appears with more complex order
8. **Difficulty Scaling**: 
   - Time limits decrease (min 30s)
   - Ingredient variety increases (2 to 7 types)
   - More complex combinations required

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
- Verify grid only spawns ingredients from current order

**Power-ups not appearing?**
- Make 4+ tile matches to trigger power-up creation
- Check that `PowerUpManager` is in the scene
- Verify `GridManager` has optional `powerUpTilePrefab` assigned (uses regular tile prefab if not set)

**Power-ups not activating?**
- Ensure `TouchInputController` is detecting clicks on power-up tiles
- Check that tiles have `BoxCollider2D` component
- Verify power-up tiles are on the correct physics layer

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

## Technical Implementation Details

### Difficulty System Architecture
- `GridManager.GetRandomPizzaIngredient()`: Limits tile variety to current order ingredients
- `PizzaOrderManager.StartPizzaOrder()`: Applies level-based time scaling
- `SamplePizzaOrders`: Progressive difficulty from 2 to 7 ingredient types

### Power-Up System Architecture
- `GridManager.ProcessMatchesWithAnimation()`: Detects combos and spawns power-ups at pivot
- `GridManager.CreatePowerUpTile()`: Instantiates power-up with special visuals
- `GridManager.ActivatePowerUpTile()`: Handles power-up logic (Bomb/Lightning/Star/Rainbow)
- `TouchInputController.StartTouch()`: Detects clicks on power-up tiles
- `Tile.cs`: Special tile types (Bomb, Lightning, Star, Rainbow) with visual effects