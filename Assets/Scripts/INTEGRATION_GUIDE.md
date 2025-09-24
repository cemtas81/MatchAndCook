# Match & Cook - Integration Guide

This guide explains how to integrate the new Match & Cook features into your Unity project.

## 🎮 New Features Implemented

### 1. Recipe System and Cooking Tasks
- **Recipe.cs** - ScriptableObject for creating recipes
- **RecipeManager.cs** - Manages active recipes and ingredient collection
- **RecipeCardUI.cs** - Displays current recipe at top of screen

### 2. Customer Orders and Time Management
- **CustomerOrder.cs** - ScriptableObject for customer orders
- **CustomerManager.cs** - Manages order generation and timing
- **CustomerOrderUI.cs** - Displays orders in screen corner

### 3. Power-ups and Special Tiles
- **PowerUpManager.cs** - Manages power-up effects and special tiles
- **PowerUpUI.cs** - UI for power-up display and activation
- Extended **Tile.cs** with special tile types (Bomb, Rainbow, Lightning, Star)

### 4. Progressive Difficulty Levels
- **Level.cs** - ScriptableObject for level configuration
- **LevelManager.cs** - Manages level progression and objectives

### 5. Social Features and Leaderboards
- **PlayerProfile.cs** - Player statistics and progression
- **EnhancedScoreManager.cs** - Advanced scoring with leaderboards
- **LeaderboardUI.cs** - UI for displaying rankings

### 6. Tutorial System
- **TutorialManager.cs** - Guided learning for new players

### 7. System Integration
- **GameSystemsIntegrator.cs** - Connects all systems together

## 🚀 Quick Setup Steps

### Step 1: Add Core Systems to Scene
1. Create empty GameObjects in your GameScene:
   - `RecipeManager` (add RecipeManager.cs)
   - `CustomerManager` (add CustomerManager.cs)
   - `PowerUpManager` (add PowerUpManager.cs)
   - `LevelManager` (add LevelManager.cs)
   - `EnhancedScoreManager` (add EnhancedScoreManager.cs)
   - `TutorialManager` (add TutorialManager.cs)
   - `GameSystemsIntegrator` (add GameSystemsIntegrator.cs)

### Step 2: Create UI Elements
1. Add UI elements to your Canvas:
   - **Recipe Card** - Top of screen (add RecipeCardUI.cs)
   - **Customer Orders** - Corner of screen (add CustomerOrderUI.cs)
   - **Power-ups** - Side panel (add PowerUpUI.cs)
   - **Leaderboard** - Full screen overlay (add LeaderboardUI.cs)

### Step 3: Create ScriptableObjects
1. Right-click in Project window → Create → Match & Cook:
   - Create sample **Recipes** (e.g., "Tomato Soup", "Pasta")
   - Create sample **Customer Orders**
   - Create sample **Levels**

### Step 4: Configure Systems
1. **RecipeManager**: Assign available recipes in inspector
2. **CustomerManager**: Assign available orders in inspector
3. **LevelManager**: Assign available levels in inspector
4. **GameSystemsIntegrator**: Enable "Auto Find References" and "Enable All Systems"

## 📋 Recipe Configuration Example

```csharp
// Example recipe setup
Recipe tomatoSoup = CreateInstance<Recipe>();
tomatoSoup.recipeName = "Tomato Soup";
tomatoSoup.description = "A warm and delicious tomato soup";
tomatoSoup.requiredIngredients.Add(new IngredientRequirement(
    Tile.TileType.Red, 5, "Tomatoes"
));
tomatoSoup.baseScore = 100;
```

## 🎯 Customer Order Configuration Example

```csharp
// Example customer order setup
CustomerOrder order = CreateInstance<CustomerOrder>();
order.customerName = "Chef Mario";
order.orderDescription = "I need tomato soup quickly!";
order.requestedRecipe = tomatoSoupRecipe;
order.timeLimit = 60f; // 60 seconds
order.baseReward = 150;
```

## 🏆 Level Configuration Example

```csharp
// Example level setup
Level level1 = CreateInstance<Level>();
level1.levelNumber = 1;
level1.levelName = "Kitchen Basics";
level1.targetScore = 1000;
level1.movesLimit = 30;
level1.maxConcurrentOrders = 1;
level1.difficulty = Level.LevelDifficulty.Easy;
```

## 🔧 Integration Notes

### Existing System Compatibility
- All new systems work with existing GameManager, GridManager, and UIManager
- No breaking changes to existing code
- Backwards compatible with current match-3 mechanics

### Event System Integration
- Systems communicate via C# events for loose coupling
- Easy to extend and modify individual systems
- Debug-friendly with comprehensive logging

### Mobile Optimization
- All UI elements are touch-friendly
- DOTween animations for smooth performance
- Efficient memory usage with object pooling concepts

## 🎨 Visual Integration

### Tile Visual Updates
The Tile.cs system now supports:
- Special tile visual effects
- Color coding for different ingredient types
- Animation effects for special tile creation and activation

### UI Theme Consistency
- All new UI follows the existing Match & Cook style
- Consistent color schemes and typography
- Mobile-first responsive design principles

## 🐛 Testing and Debugging

### Debug Features
- `GameSystemsIntegrator` provides system status logging
- Each manager has comprehensive debug output
- Easy to test individual systems in isolation

### Testing Checklist
- [ ] Recipes appear and track ingredient collection
- [ ] Customer orders spawn and countdown properly
- [ ] Power-ups activate with visual effects
- [ ] Level progression works correctly
- [ ] Leaderboards save and display scores
- [ ] Tutorial guides new players effectively

## 🚀 Performance Considerations

- Systems use efficient event-based communication
- UI updates only when necessary
- Object pooling ready for high-frequency operations
- Mobile-optimized rendering and animations

## 📱 Mobile Features

- Touch-optimized UI sizing
- Proper landscape/portrait support
- Performance monitoring integration points
- Social sharing integration ready

---

## 🎮 Ready to Play!

Once integrated, your Match & Cook game will have:
- ✅ Complete recipe cooking system
- ✅ Time-pressured customer orders  
- ✅ 6 different power-up types
- ✅ Progressive level system
- ✅ Social leaderboards
- ✅ Tutorial for new players
- ✅ Modern mobile game features

Enjoy your enhanced Match & Cook experience! 🍳✨