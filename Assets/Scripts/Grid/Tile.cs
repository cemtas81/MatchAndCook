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
    
    // Tile types for the cooking theme
    public enum TileType
    {
        Red,      // Tomatoes, meat
        Blue,     // Water, ice
        Green,    // Vegetables, herbs
        Yellow,   // Grains, pasta
        Purple    // Special ingredients
    }
    
    // Properties
    public TileType Type => tileType;
    public bool IsMoving { get; private set; }
    
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
    /// Get the color for a specific tile type (placeholder system)
    /// </summary>
    private Color GetTileColor(TileType type)
    {
        switch (type)
        {
            case TileType.Red:    return Color.red;
            case TileType.Blue:   return Color.blue;
            case TileType.Green:  return Color.green;
            case TileType.Yellow: return Color.yellow;
            case TileType.Purple: return new Color(0.5f, 0f, 0.5f); // Purple
            default:              return Color.white;
        }
    }
    
    /// <summary>
    /// Animate tile moving to a new position
    /// </summary>
    public void MoveTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (IsMoving) return;
        
        IsMoving = true;
        StartCoroutine(MoveCoroutine(targetPosition, swapDuration, onComplete));
    }
    
    /// <summary>
    /// Animate tile falling to a new position
    /// </summary>
    public void FallTo(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (IsMoving) return;
        
        IsMoving = true;
        StartCoroutine(MoveCoroutine(targetPosition, fallDuration, onComplete));
    }
    
    /// <summary>
    /// Coroutine for smooth movement animation
    /// </summary>
    private System.Collections.IEnumerator MoveCoroutine(Vector3 targetPosition, float duration, System.Action onComplete)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Smooth easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            
            yield return null;
        }
        
        transform.position = targetPosition;
        IsMoving = false;
        onComplete?.Invoke();
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
        return other != null && other.tileType == this.tileType;
    }
}