# Input System Documentation

## Overview
The input system has been simplified to use a single controller (TouchInputController) that works with both mobile touch and desktop mouse input.

## Architecture

### TouchInputController.cs
- **Primary responsibility**: Handles all user input (touch/mouse)
- **Features**:
  - Tap to select tiles
  - Swipe to swap adjacent tiles
  - Automatic deselection when touching empty areas
  - Input locking during animations
  - UI collision prevention

### GridManager.cs
- **Primary responsibility**: Manages grid logic and tile operations
- **Public Methods**:
  - `TrySwapTiles(Tile, Tile)`: Attempts to swap two tiles
  - `IsInputLocked()`: Checks if input should be blocked during animations
  - `GetGrid()`: Returns the current grid state

### Key Changes Made

1. **Removed input handling from GridManager**: All input logic moved to TouchInputController
2. **Simplified mobile optimization**: Removed excessive optimization code
3. **Fixed world position conversion**: Proper screen-to-world coordinate transformation
4. **Added input locking**: Prevents input during swap animations
5. **UI collision prevention**: Input is ignored when touching UI elements

## Usage

### For Developers
1. Attach `TouchInputController` to any GameObject in the scene
2. Ensure `GridManager` is present in the scene
3. Tiles must have `BoxCollider2D` components for raycast detection
4. UI elements should have "Raycast Target" disabled if they don't need interaction

### Input Flow
1. User taps/clicks on a tile → Tile gets selected (visual feedback)
2. User taps/clicks on adjacent tile → Swap is attempted
3. User taps/clicks on same tile → Tile gets deselected
4. User swipes from selected tile → Swap with tile in swipe direction
5. User taps/clicks empty space → Current selection is cleared

### Validation
Use the `InputSystemValidator` script to debug input issues:
- Attach to any GameObject
- Check console for validation results
- Use "Test Input Conversion" context menu for manual testing

## Mobile Considerations

- Input works on both mobile and desktop
- Swipe threshold is configurable (default: 50 pixels)
- Touch sensitivity can be adjusted
- UI elements automatically blocked from tile input
- Input is locked during animations to prevent conflicts

## Troubleshooting

### Common Issues
1. **Tiles not responding to touch**: Check if tiles have `BoxCollider2D` components
2. **UI blocking tile input**: Disable "Raycast Target" on non-interactive UI elements
3. **Input during animations**: Input is automatically locked during swaps
4. **World position errors**: Camera setup is automatically detected and configured

### Debug Tools
- Enable `logInputEvents` on TouchInputController for detailed input logging
- Use InputSystemValidator for comprehensive system validation
- Check console for raycast hit information