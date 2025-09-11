# Match & Cook - Scripts Documentation

## Core Systems

### Grid System (`/Grid/`)
- **GridManager.cs**: Main grid controller that handles tile creation, matching logic, swapping, and refilling
- **Tile.cs**: Individual tile behavior with animation and type management

### Game Management (`/Game/`)
- **GameManager.cs**: Overall game state management, win/lose conditions, and scene flow
- **ScoreManager.cs**: Score tracking, high scores, and statistics

### User Interface (`/UI/`)
- **UIManager.cs**: In-game UI elements (score, moves, progress bar, game end screen)
- **MenuManager.cs**: Main menu interface and settings management

## Key Features

### Mobile-Optimized
- Touch-based tile swapping
- Large UI elements for finger interaction
- Portrait/landscape friendly layouts
- Performance optimized for mobile devices

### Match-3 Core Mechanics
- 8x8 grid with 5 tile types (cooking themed)
- Horizontal and vertical matching (3+ tiles)
- Cascade matching and refill system
- Smooth animations for swapping and falling

### Game Flow
- Target score system (1000 points default)
- Limited moves (30 moves default)
- Combo multipliers for chain reactions
- Clear win/lose conditions

### Visual Design
- Placeholder colored squares for easy replacement
- Clean, readable UI design
- Progress bar showing score advancement
- Game end screens with restart functionality

## Setup Instructions

1. Create tile prefab with SpriteRenderer and Collider2D
2. Set up GameScene with GridManager, GameManager, and UI Canvas
3. Configure MainMenu scene with MenuManager
4. Add scenes to Build Settings
5. Test on target mobile platform

## Expansion Ready

The code structure supports easy addition of:
- New tile types for cooking ingredients
- Power-ups and special tiles
- Recipe/cooking themed objectives
- Customer orders system
- Additional game modes

All placeholder systems are designed for quick asset replacement without code changes.