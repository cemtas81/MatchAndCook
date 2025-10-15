# Ingredient Warning System

## Overview
The Ingredient Warning System provides a grace period when a pizza order starts but the required ingredients are not in stock. This gives players time to purchase the needed ingredients before the order fails.

## Components

### IngredientWarningTimer
Located in: `Assets/Scripts/UI/IngredientWarningTimer.cs`

**Purpose**: Manages the countdown timer during the warning period.

**Settings**:
- `warningDuration`: Grace period in seconds (default: 30 seconds)

**Events**:
- `OnWarningTimeChanged`: Fired every frame with remaining time
- `OnWarningExpired`: Fired when the warning timer reaches zero
- `OnWarningResolved`: Fired when ingredients are gathered and warning is resolved

### IngredientWarningPanel
Located in: `Assets/Scripts/UI/IngredientWarningPanel.cs`

**Purpose**: Visual UI component that displays the warning message and countdown timer.

**UI Elements**:
- Warning panel container
- Warning message text (default: "Malzemeler eksik! Lütfen topla!")
- Warning timer display (MM:SS format)
- Warning background with color-coded urgency

**Visual Settings**:
- `warningColor`: Normal warning color (default: orange)
- `urgentColor`: Urgent warning color when time is low (default: red)
- `urgentThreshold`: Seconds when color becomes urgent (default: 10 seconds)
- `enablePulseAnimation`: Enables pulsing background animation

## How It Works

### Flow
1. **Order Start**: When `PizzaOrderManager.StartPizzaOrder()` is called with an order
2. **Ingredient Check**: System checks if all required ingredients are in stock via `MaterialStockManager.CanFulfillOrder()`
3. **Warning Activation**: If ingredients are insufficient:
   - Sets `isWaitingForIngredients` flag to true
   - Starts `IngredientWarningTimer`
   - Shows `IngredientWarningPanel`
   - Main order timer is paused
4. **During Warning Period**:
   - System continuously checks if ingredients become available
   - Warning panel displays countdown timer
   - Background pulses and color changes based on urgency
5. **Resolution**:
   - **Success**: If ingredients are gathered in time:
     - `OnIngredientWarningResolved` event is fired
     - Warning panel is hidden
     - Order proceeds normally
     - Main timer starts counting down
   - **Failure**: If timer expires:
     - `OnIngredientWarningExpired` event is fired
     - Order fails immediately
     - Warning panel is hidden

### Integration with PizzaOrderManager

The warning system is fully integrated into the order flow:

```csharp
// In StartPizzaOrder()
if (stockManager != null && !stockManager.CanFulfillOrder(order))
{
    // Start warning timer instead of failing immediately
    isWaitingForIngredients = true;
    warningTimer?.StartWarningTimer();
    warningPanel?.ShowWarning("Malzemeler eksik! Lütfen topla!");
}
else
{
    // Normal order start
    OnOrderStarted?.Invoke(currentOrder);
}

// In Update()
void CheckAndResolveWarning()
{
    if (!isWaitingForIngredients || currentOrder == null) return;
    
    if (stockManager != null && stockManager.CanFulfillOrder(currentOrder))
    {
        warningTimer?.ResolveWarning();
    }
}
```

## Setup Instructions

1. Add `IngredientWarningTimer` component to your scene (typically to GameManager or a dedicated manager object)
2. Add `IngredientWarningPanel` component to your UI Canvas
3. Configure the UI elements in the IngredientWarningPanel inspector:
   - Assign the warning panel GameObject
   - Assign warning message TextMeshProUGUI
   - Assign warning timer TextMeshProUGUI
   - Assign warning background Image
4. Adjust warning duration and visual settings as needed
5. The system will automatically integrate with `PizzaOrderManager`

## Performance Optimizations

The warning system is designed to be lightweight:
- Only runs checks when `isWaitingForIngredients` is true
- Minimal UI updates (only timer text changes each frame)
- Efficient ingredient availability checks
- No continuous coroutines or heavy operations

## Benefits

1. **Better Player Experience**: Players have time to react to missing ingredients
2. **Clear Communication**: Visual warning makes the situation obvious
3. **Fair Gameplay**: Grace period prevents instant failures
4. **Seamless Integration**: Works with existing order and stock systems
5. **Performance Focused**: Minimal overhead, only active when needed
