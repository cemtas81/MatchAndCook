# Match & Cook - Unity Setup Guide

## Scene Setup Instructions

### GameScene Setup
1. **Create Grid System**
   - Empty GameObject named "GridManager" with GridManager.cs script
   - Empty GameObject named "GridParent" as child for organizing tiles
   - Set TilePrefab reference in GridManager

2. **Create Game Management**
   - Empty GameObject named "GameManager" with GameManager.cs script
   - Empty GameObject named "ScoreManager" with ScoreManager.cs script
   - Link references: GameManager → GridManager, ScoreManager, UIManager

3. **Create UI Canvas**
   - Canvas (Screen Space - Overlay, UI Scale Mode: Scale With Screen Size, Reference Resolution: 1080x1920)
   - Add UIManager.cs script to Canvas
   - Create UI hierarchy:
     ```
     Canvas (UIManager)
     ├── HUD Panel
     │   ├── Score Text (TextMeshPro)
     │   ├── Moves Text (TextMeshPro)
     │   ├── Target Text (TextMeshPro)
     │   ├── Progress Bar (Slider)
     │   └── Pause Button
     ├── Game End Panel (initially inactive)
     │   ├── Title Text (TextMeshPro)
     │   ├── Score Text (TextMeshPro)
     │   ├── Restart Button
     │   └── Menu Button
     └── Pause Panel (initially inactive)
         ├── Resume Button
         └── Menu Button
     ```

4. **Camera Setup**
   - Main Camera positioned to show full grid (Z: -10)
   - Orthographic projection
   - Size: 6-8 depending on grid size

### MainMenu Setup
1. **Create Menu Canvas**
   - Canvas with MenuManager.cs script
   - Create UI hierarchy:
     ```
     Canvas (MenuManager)
     ├── Title Text
     ├── High Score Text
     ├── Last Score Text
     ├── Play Button
     ├── Settings Button
     ├── Exit Button
     └── Settings Panel (initially inactive)
         ├── Volume Sliders
         ├── Reset High Score Button
         └── Close Button
     ```

### Tile Prefab Creation
1. **Basic Tile Prefab**
   - GameObject with Tile.cs script
   - SpriteRenderer (default Unity knob sprite)
   - BoxCollider2D (size 1x1)
   - Layer: Default or custom "Tiles" layer

### Mobile Optimization Settings
1. **Project Settings**
   - Default Orientation: Portrait or Auto Rotation
   - Target Device: Mobile
   - Graphics: Use Mobile-optimized settings

2. **Quality Settings**
   - Create mobile-optimized quality preset
   - Disable expensive effects (shadows, realtime reflections)

3. **Build Settings**
   - Add MainMenu and GameScene
   - Set MainMenu as first scene

## Quick Setup Script
The SetupHelper.cs script can automate basic scene setup for faster development.