# Match & Cook
A mobile-optimized match-3 cooking game built in Unity.

## Features

### Ingredient Warning System ✨ NEW
- **Grace Period**: 30-second countdown when ingredients are insufficient
- **Visual Warning**: Clear warning panel with timer display
- **Smart Resolution**: Order continues if ingredients are gathered in time
- **Fair Gameplay**: Prevents instant order failures

### Session-Based Progression ✨ NEW
- **Session System**: 10 pizzas per session with increasing difficulty
- **Dynamic Difficulty**: Time reduces by 10% each session
- **Session Tracking**: Clear progress indicators for player motivation

### Money System ✨ NEW
- **Earn Money**: Collect cash by completing pizza orders
- **Visual Feedback**: Animated coins fly from customer to cash register
- **Price Variation**: Different pizzas have different prices
- **Persistent Tracking**: Total money displayed in UI

### Dynamic Ingredients ✨ NEW
- **Smart Spawning**: Only ingredients needed for current pizza spawn
- **Early Game**: Simple pizzas with 1-2 ingredients (easier matches)
- **Late Game**: Complex pizzas with 3-4 ingredients (harder matches)
- **Automatic Scaling**: Difficulty increases naturally with complexity

### Core Match-3 Gameplay
- **8x8 Grid**: Classic match-3 mechanics with cooking-themed tiles
- **5 Tile Types**: Red (meat/tomatoes), Blue (water/ice), Green (vegetables), Yellow (grains), Purple (special)
- **Touch Controls**: Mobile-optimized swipe and tap interactions
- **Cascade Matching**: Chain reactions and combo system
- **Smooth Animations**: Professional tile swapping and falling animations

### Game Mechanics
- **Target Score System**: Reach 1000 points to win
- **Limited Moves**: 30 moves per game for quick mobile sessions
- **Combo Multipliers**: Bonus points for consecutive matches
- **Win/Lose Conditions**: Clear objectives for engaging gameplay

### Mobile Optimized
- **Touch Input**: Responsive swipe gestures and tap selection
- **UI Design**: Large buttons and clear visual feedback
- **Performance**: Optimized for mobile devices
- **Screen Scaling**: Works on various mobile screen sizes

### Clean Architecture
- **Modular Scripts**: Separated concerns (Grid, Game, UI, Input)
- **Placeholder Assets**: Easy to replace colored squares with final artwork
- **Commented Code**: Well-documented for maintainability
- **Expandable**: Ready for additional features like recipes and customers

## Project Structure

```
Assets/
├── Scenes/
│   ├── MainMenu.unity      # Main menu with settings
│   └── GameScene.unity     # Core gameplay scene
├── Scripts/
│   ├── Session/            # ✨ NEW: Session management system
│   ├── Grid/               # Match-3 grid system
│   ├── Game/               # Game management and scoring
│   ├── Pizza/              # Pizza order system
│   ├── UI/                 # User interface and touch input (+ CashFlowAnimator)
│   └── SetupHelper.cs      # Automated scene setup
├── Sprites/
│   └── Tiles/              # Placeholder tile sprites
└── UI/
    └── UI_Elements/        # Placeholder UI graphics
```

## Quick Start

1. Open in Unity 6000.0.41f1 or later
2. Run `SetupHelper.cs` to auto-generate scene objects
3. Add `SessionManager` component to GameManager GameObject
4. Add `CashFlowAnimator` component to UI Canvas
5. Add `IngredientWarningTimer` component to GameManager GameObject
6. Add `IngredientWarningPanel` component to UI Canvas
7. Configure PizzaOrder assets with `price` values
8. Build and run on mobile device or simulator
9. Replace placeholder sprites in Assets/Sprites/ with final artwork

## Implementation Details

See documentation for detailed implementation guides:
- [INGREDIENT_WARNING_SYSTEM.md](INGREDIENT_WARNING_SYSTEM.md) - Warning system architecture and setup
- [CODE_OPTIMIZATION_SUMMARY.md](CODE_OPTIMIZATION_SUMMARY.md) - Performance optimizations
- [IMPLEMENTATION_SESSION_MONEY.md](IMPLEMENTATION_SESSION_MONEY.md) - Session and money systems


## Ready for Expansion

The foundation supports easy addition of:
- Recipe system and cooking objectives
- Customer orders and time management
- Power-ups and special tiles
- Progressive difficulty levels
- Social features and leaderboards

