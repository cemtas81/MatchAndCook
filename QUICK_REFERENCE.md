# Session, Money & Dynamic Ingredient System - Quick Reference

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        GAME FLOW                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  SessionManager                PizzaOrderManager                │
│  ┌──────────────┐             ┌──────────────────┐             │
│  │ Session: 1   │────────────▶│ Pizza Order 1    │             │
│  │ Progress:0/10│             │ Time: 120s       │             │
│  │ Time: 1.0x   │             │ Price: $50       │             │
│  └──────────────┘             └──────────────────┘             │
│         │                              │                         │
│         │                              │                         │
│         │                      ┌───────▼────────┐               │
│         │                      │  GridManager   │               │
│         │                      │ Spawns ONLY:   │               │
│         │                      │ • Tomato       │               │
│         │                      │ • Cheese       │               │
│         │                      └────────────────┘               │
│         │                              │                         │
│         │                              │ Player matches          │
│         │                              │ ingredients             │
│         │                              │                         │
│         │                      ┌───────▼────────┐               │
│         │                      │ Order Complete!│               │
│         │                      │ Money: +$50    │               │
│         │                      └────────────────┘               │
│         │                              │                         │
│         │ registerPizzaCompleted()     │                         │
│         │◀─────────────────────────────┘                         │
│         │                              │                         │
│  ┌──────▼──────────┐           ┌──────▼────────────┐           │
│  │ Session: 1      │           │ CashFlowAnimator  │           │
│  │ Progress: 1/10  │           │ Coins fly!        │           │
│  │ (9 more to go)  │           │ 💰💰💰 → Cash     │           │
│  └─────────────────┘           └───────────────────┘           │
│         │                              │                         │
│         │                      ┌───────▼────────┐               │
│         │                      │   UIManager    │               │
│         │                      │ Money: $50     │               │
│         │                      └────────────────┘               │
│         │                                                        │
│  After 10 pizzas...                                             │
│         │                                                        │
│  ┌──────▼──────────┐                                            │
│  │ Session: 2      │                                            │
│  │ Progress: 0/10  │                                            │
│  │ Time: 0.9x ⚡   │  (10% faster!)                             │
│  └─────────────────┘                                            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

### SessionManager
- ✅ Tracks session number (1, 2, 3...)
- ✅ Tracks pizza completion (0-10 per session)
- ✅ Calculates time multiplier (0.9^session)
- ✅ Fires events on session start/complete

### PizzaOrderManager
- ✅ Manages current pizza order
- ✅ Applies session time multiplier
- ✅ Awards money on completion
- ✅ Registers completion with SessionManager
- ✅ Triggers CashFlowAnimator

### GridManager
- ✅ Gets required ingredients from PizzaOrderManager
- ✅ Spawns ONLY those ingredient types
- ✅ Early levels: 1-2 ingredients = easier
- ✅ Late levels: 3-4 ingredients = harder

### CashFlowAnimator
- ✅ Animates coins from customer to register
- ✅ Uses DOTween for smooth animations
- ✅ Plays particle effects
- ✅ Plays sound effects

### UIManager
- ✅ Displays total money: $XXX
- ✅ Subscribes to money change events
- ✅ Updates display automatically

## Event Flow

```
OrderComplete
     │
     ├──▶ AddMoney()
     │      │
     │      ├──▶ totalMoney += price
     │      │
     │      ├──▶ OnMoneyChanged.Invoke() ──▶ UIManager.UpdateMoney()
     │      │
     │      └──▶ CashFlowAnimator.AnimateMoneyCollection()
     │
     └──▶ SessionManager.RegisterPizzaCompleted()
            │
            └──▶ OnSessionProgress.Invoke()
                 │
                 └──▶ Check if 10 pizzas done
                      │
                      └──▶ CompleteSession() ──▶ Start next session
```

## Setup Checklist

### In Unity Editor:
- [ ] Add SessionManager component to GameManager GameObject
- [ ] Add CashFlowAnimator component to UI Canvas
- [ ] Create coin icon prefab (UI Image with sprite)
- [ ] Assign coin prefab to CashFlowAnimator
- [ ] Create cash register position GameObject
- [ ] Assign cash register to CashFlowAnimator
- [ ] Add TextMeshProUGUI for money display
- [ ] Assign money text to UIManager
- [ ] Update PizzaOrder assets with price values
- [ ] Test with SessionMoneyIntegrationTest script

### PizzaOrder Assets:
```
Margherita Pizza:
- Ingredients: Tomato, Cheese (2 types)
- Price: $50
- Easy difficulty

Pepperoni Supreme:
- Ingredients: Tomato, Cheese, Pepperoni, Basil (4 types)
- Price: $150
- Hard difficulty
```

## Testing

### Manual Test Flow:
1. Start game
2. Complete 1 pizza order
3. Verify:
   - Money increases by pizza price
   - Coin animation plays
   - Session progress: 1/10
   - UI displays correct money
4. Complete 9 more pizzas
5. Verify:
   - Session advances to 2
   - Time becomes shorter (0.9x)
   - Session progress resets to 0/10
6. Check GridManager:
   - Only spawns ingredient types from current order
   - Early pizzas easier (fewer types)
   - Later pizzas harder (more types)

### Automated Test:
```csharp
// Attach SessionMoneyIntegrationTest to GameObject
// Use context menu: Right-click → Run All Tests
// Check console for test results
```

## Performance Notes

### Mobile Optimization:
- ✅ Minimal animations (DOTween is efficient)
- ✅ Events prevent polling
- ✅ Simple calculations (no heavy math)
- ✅ Clean architecture (no circular references)

### Memory:
- ✅ No memory leaks (events unsubscribed in OnDestroy)
- ✅ Minimal allocations (reuses lists where possible)
- ✅ Efficient particle effects

## Common Issues & Solutions

### Issue: Money not updating in UI
**Solution:** Check that UIManager.moneyText is assigned in Inspector

### Issue: Coin animation not playing
**Solution:** 
1. Verify CashFlowAnimator.coinIconPrefab is assigned
2. Verify CashFlowAnimator.cashRegisterPosition is assigned
3. Check that DOTween is imported

### Issue: Grid spawning all tile types
**Solution:** Ensure PizzaOrderManager is active before GridManager creates tiles

### Issue: Session not advancing
**Solution:** Verify SessionManager.RegisterPizzaCompleted(true) is being called

## Code Examples

### Accessing Money from Code:
```csharp
PizzaOrderManager pizzaManager = FindFirstObjectByType<PizzaOrderManager>();
int currentMoney = pizzaManager.TotalMoney;
Debug.Log($"Player has ${currentMoney}");
```

### Listening to Money Changes:
```csharp
void Start() {
    PizzaOrderManager pizzaManager = FindFirstObjectByType<PizzaOrderManager>();
    pizzaManager.OnMoneyChanged += OnMoneyChanged;
}

void OnMoneyChanged(int newAmount) {
    Debug.Log($"Money updated: ${newAmount}");
}
```

### Checking Session Progress:
```csharp
SessionManager session = FindFirstObjectByType<SessionManager>();
int current = session.PizzasCompletedInSession;
int total = session.PizzasPerSession;
float progress = session.GetSessionProgress(); // 0.0 to 1.0
Debug.Log($"Session {session.CurrentSession}: {current}/{total} ({progress:P0})");
```

## Summary

✅ **Simple** - Each component has one job
✅ **Clean** - Event-driven architecture
✅ **Scalable** - Easy to add features
✅ **Mobile-Ready** - Performance optimized
✅ **Documented** - Clear code and comments
✅ **Tested** - Integration test included
✅ **Publish-Ready** - Production quality code
