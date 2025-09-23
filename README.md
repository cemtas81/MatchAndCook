# Match & Cook
A mobile-optimized match-3 cooking game built in Unity.

## Features

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
│   ├── Grid/               # Match-3 grid system
│   ├── Game/               # Game management and scoring
│   ├── UI/                 # User interface and touch input
│   └── SetupHelper.cs      # Automated scene setup
├── Sprites/
│   └── Tiles/              # Placeholder tile sprites
└── UI/
    └── UI_Elements/        # Placeholder UI graphics
```

## Quick Start

1. Open in Unity 6000.0.41f1 or later
2. Run `SetupHelper.cs` to auto-generate scene objects
3. Build and run on mobile device or simulator
4. Replace placeholder sprites in Assets/Sprites/ with final artwork

## Ready for Expansion

The foundation supports easy addition of:
- Recipe system and cooking objectives
- Customer orders and time management
- Power-ups and special tiles
- Progressive difficulty levels
- Social features and leaderboards

