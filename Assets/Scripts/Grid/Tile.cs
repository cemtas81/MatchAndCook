using DG.Tweening;
using UnityEngine;

/// <summary>
/// Represents a single tile in the match-3 grid.
/// Handles tile properties, visual representation, and basic interactions.
/// </summary>
public class Tile : MonoBehaviour
{
    [Header("Tile Properties")]
    [SerializeField] private TileType tileType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Position")]
    public int gridX;
    public int gridY;
    
    [Header("Animation")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float fallDuration = 0.5f;
    
    // Pizza ingredient tile types
    public enum TileType
    {
        // Basic pizza ingredients
        Tomato,     // Red tomato sauce and fresh tomatoes
        Cheese,     // Yellow cheese (mozzarella, cheddar)
        Pepperoni,  // Red/orange pepperoni slices
        Mushroom,   // Brown mushrooms
        Pepper,     // Green bell peppers
        Onion,      // Purple/white onions
        Olives,     // Black/green olives
        
        // Special tile types (power-ups)
        Bomb,       // Destroys 3x3 area around it
        Rainbow,    // Matches with any ingredient type
        Lightning,  // Destroys entire row or column
        Star        // Destroys all tiles of one ingredient type
    }
    
    // Properties
    public TileType Type => tileType;
    public bool IsMoving { get; private set; }
    public bool IsSpecialTile => IsSpecial(tileType);
    public bool IsIngredient => !IsSpecial(tileType);
    
    /// <summary>
    /// Initialize the tile with a specific type and grid position
    /// </summary>
    public void Initialize(TileType type, int x, int y)
    {
        tileType = type;
        gridX = x;
        gridY = y;
        UpdateVisual();
    }
    
    /// <summary>
    /// Update the tile's visual representation based on its type
    /// </summary>
    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // Set color based on tile type (placeholder visuals)
        Color tileColor = GetTileColor(tileType);
        spriteRenderer.color = tileColor;
    }
    
    /// <summary>
    /// Get the color for a specific pizza ingredient tile type (placeholder visual system)
    /// In production, these would be replaced with actual sprite assets
    /// </summary>
    private Color GetTileColor(TileType type)
    {
        switch (type)
        {
            // Pizza ingredient colors for visual identification
            case TileType.Tomato:    return new Color(0.8f, 0.2f, 0.2f); // Deep red for tomatoes
            case TileType.Cheese:    return new Color(1f, 0.9f, 0.3f);   // Golden yellow for cheese
            case TileType.Pepperoni: return new Color(0.7f, 0.3f, 0.1f); // Dark red-orange for pepperoni
            case TileType.Mushroom:  return new Color(0.6f, 0.4f, 0.2f); // Brown for mushrooms
            case TileType.Pepper:    return new Color(0.2f, 0.7f, 0.2f); // Bright green for peppers
            case TileType.Onion:     return new Color(0.9f, 0.8f, 0.9f); // Light purple-white for onions
            case TileType.Olives:    return new Color(0.1f, 0.1f, 0.2f); // Dark purple-black for olives
            
            // Special power-up tiles with distinct colors
            case TileType.Bomb:      return new Color(0.2f, 0.2f, 0.2f); // Dark gray for bomb
            case TileType.Rainbow:   return Color.white;                  // White with rainbow effect
            case TileType.Lightning: return new Color(0.9f, 0.9f, 0.1f); // Bright yellow for lightning
            case TileType.Star:      return new Color(1f, 0.6f, 0f);     // Orange for star power-up
            
            default: return Color.white;
        }
    }

    /// <summary>
    /// Animate tile moving to a new position
    /// </summary>
    public void MoveTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (IsMoving) return;
        IsMoving = true;
        transform.DOMove(targetPosition, swapDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                IsMoving = false;
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Animate tile falling to a new position
    /// </summary>
    public void FallTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (IsMoving) return;
        IsMoving = true;
        transform.DOMove(targetPosition, fallDuration)
            .SetEase(Ease.InBounce)
            .OnComplete(() => {
                IsMoving = false;
                onComplete?.Invoke();
            });
    }
  
    /// <summary>
    /// Set the grid position of this tile
    /// </summary>
    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }
    
    /// <summary>
    /// Check if this tile can be matched with another tile
    /// </summary>
    public bool CanMatchWith(Tile other)
    {
        if (other == null) return false;
        
        // Rainbow tiles match with anything
        if (tileType == TileType.Rainbow || other.tileType == TileType.Rainbow)
            return true;
            
        // Normal matching
        return other.tileType == this.tileType;
    }
    
    /// <summary>
    /// Check if a tile type is a special tile
    /// </summary>
    public static bool IsSpecial(TileType type)
    {
        return type == TileType.Bomb || type == TileType.Rainbow || 
               type == TileType.Lightning || type == TileType.Star;
    }
    
    /// <summary>
    /// Convert to special tile type
    /// </summary>
    public void ConvertToSpecialTile(TileType specialType)
    {
        if (!IsSpecial(specialType)) return;
        
        tileType = specialType;
        UpdateVisual();
        
        // Add special visual effect
        PlaySpecialCreationEffect();
    }
    
    /// <summary>
    /// Play special tile creation effect
    /// </summary>
    private void PlaySpecialCreationEffect()
    {
        // Pulse effect for special tile creation
        transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 8)
            .SetEase(Ease.OutBounce);
            
        // Color flash
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.DOColor(Color.white, 0.1f)
                .SetLoops(3, LoopType.Yoyo)
                .OnComplete(() => spriteRenderer.color = originalColor);
        }
    }
    
    /// <summary>
    /// Activate special tile effect
    /// </summary>
    public void ActivateSpecialEffect()
    {
        switch (tileType)
        {
            case TileType.Bomb:
                PlayBombEffect();
                break;
            case TileType.Rainbow:
                PlayRainbowEffect();
                break;
            case TileType.Lightning:
                PlayLightningEffect();
                break;
            case TileType.Star:
                PlayStarEffect();
                break;
        }
    }
    
    /// <summary>
    /// Play bomb tile effect
    /// </summary>
    private void PlayBombEffect()
    {
        transform.DOScale(Vector3.one * 1.5f, 0.2f)
            .OnComplete(() => transform.DOScale(Vector3.zero, 0.1f));
    }
    
    /// <summary>
    /// Play rainbow tile effect
    /// </summary>
    private void PlayRainbowEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f)
                .SetLoops(6, LoopType.Yoyo)
                .OnComplete(() => transform.DOScale(Vector3.zero, 0.2f));
        }
    }
    
    /// <summary>
    /// Play lightning tile effect
    /// </summary>
    private void PlayLightningEffect()
    {
        transform.DOShakePosition(0.3f, 5f, 20)
            .OnComplete(() => transform.DOScale(Vector3.zero, 0.1f));
    }
    
    /// <summary>
    /// Play star tile effect
    /// </summary>
    private void PlayStarEffect()
    {
        transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.LocalAxisAdd)
            .OnComplete(() => transform.DOScale(Vector3.zero, 0.2f));
    }
}