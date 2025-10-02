using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Manages the match-3 grid system including tile creation, swapping, matching, and refilling.
/// Core logic for the match-3 gameplay mechanics.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float tileSpacing = 1.1f;
    
    [Header("Tile Setup")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gridParent;

    [Header("Animation Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float matchClearDuration = 0.2f;
    [SerializeField] private float delayBetweenActions = 0.3f;
    
    // Grid data
    private Tile[,] grid;
    private bool isSwapping = false;
    
    // Events
    public System.Action<int> OnTilesCleared;
    public System.Action OnGridRefilled;
    
    // Pizza order integration
    private PizzaOrderManager pizzaOrderManager;
    
    void Start()
    {
        InitializeGrid();
        
        // Find pizza order manager for ingredient coordination
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
    }
    
    /// <summary>
    /// Initialize the grid with random tiles
    /// </summary>
    private void InitializeGrid()
    {
        grid = new Tile[gridWidth, gridHeight];
        
        // Calculate grid center offset for proper positioning
        Vector3 gridCenter = new Vector3((gridWidth - 1) * tileSpacing * 0.5f, (gridHeight - 1) * tileSpacing * 0.5f, 0);
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateTile(x, y, gridCenter);
            }
        }
        
        // Ensure no initial matches
        EnsureNoInitialMatches();
    }
    
    /// <summary>
    /// Create a single tile at the specified grid position
    /// </summary>
    private void CreateTile(int x, int y, Vector3 offset = default)
    {
        Vector3 position = new Vector3(x * tileSpacing, y * tileSpacing, 0) - offset;
        GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, gridParent);
        
        Tile tile = tileObject.GetComponent<Tile>();
        if (tile == null)
        {
            tile = tileObject.AddComponent<Tile>();
        }
        
        // Add SpriteRenderer if missing
        if (tileObject.GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = tileObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
        }
        
        // Add Collider2D for input
        if (tileObject.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = tileObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
        
        // Random tile type prioritizing pizza order ingredients
        Tile.TileType randomType = GetRandomPizzaIngredient();
        tile.Initialize(randomType, x, y);
        
        grid[x, y] = tile;
    }
    
    /// <summary>
    /// Create a default white sprite for tiles
    /// </summary>
    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
    }
    
    /// <summary>
    /// Ensure no matches exist at game start
    /// </summary>
    private void EnsureNoInitialMatches()
    {
        bool hasMatches = true;
        int attempts = 0;
        
        while (hasMatches && attempts < 100)
        {
            hasMatches = false;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (IsPartOfMatch(x, y))
                    {
                        // Change this tile to a different type
                        Tile.TileType newType = GetSafeRandomType(x, y);
                        grid[x, y].Initialize(newType, x, y);
                        hasMatches = true;
                    }
                }
            }
            
            attempts++;
        }
    }
    
    /// <summary>
    /// Get a random pizza ingredient type that won't create matches at this position
    /// </summary>
    private Tile.TileType GetSafeRandomType(int x, int y)
    {
        List<Tile.TileType> availableTypes = new List<Tile.TileType>();
        
        // Only consider pizza ingredients (not special tiles) for safe placement
        foreach (Tile.TileType type in System.Enum.GetValues(typeof(Tile.TileType)))
        {
            if (Tile.IsSpecial(type)) continue; // Skip special tiles for regular placement
            
            // Temporarily set this type and check for matches
            Tile.TileType originalType = grid[x, y].Type;
            grid[x, y].Initialize(type, x, y);
            
            if (!IsPartOfMatch(x, y))
            {
                availableTypes.Add(type);
            }
            
            // Restore original type
            grid[x, y].Initialize(originalType, x, y);
        }
        
        if (availableTypes.Count > 0)
        {
            // Prioritize current order ingredients if available
            if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
            {
                List<Tile.TileType> requiredTypes = pizzaOrderManager.GetRequiredIngredientTypes();
                List<Tile.TileType> priorityTypes = new List<Tile.TileType>();
                
                foreach (var type in availableTypes)
                {
                    if (requiredTypes.Contains(type))
                    {
                        priorityTypes.Add(type);
                    }
                }
                
                if (priorityTypes.Count > 0 && Random.Range(0f, 1f) < 0.7f)
                {
                    return priorityTypes[Random.Range(0, priorityTypes.Count)];
                }
            }
            
            return availableTypes[Random.Range(0, availableTypes.Count)];
        }
        
        // Fallback to any pizza ingredient
        return GetRandomPizzaIngredient();
    }
    
    /// <summary>
    /// Get a random pizza ingredient type, prioritizing current order requirements
    /// DIFFICULTY SYSTEM: Tile variety limited to ingredients in current pizza order
    /// </summary>
    private Tile.TileType GetRandomPizzaIngredient()
    {
        // Get required ingredients from current pizza order
        List<Tile.TileType> requiredIngredients = null;
        if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
        {
            requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
        }
        
        // DIFFICULTY SYSTEM: If we have a current order, limit tile variety to order ingredients only
        // This makes early levels easier (fewer tile types) and later levels harder (more tile types)
        if (requiredIngredients != null && requiredIngredients.Count > 0)
        {
            return requiredIngredients[Random.Range(0, requiredIngredients.Count)];
        }
        
        // Fallback: If no order is active, use all available pizza ingredients
        List<Tile.TileType> availableIngredients = new List<Tile.TileType>();
        foreach (Tile.TileType ingredient in System.Enum.GetValues(typeof(Tile.TileType)))
        {
            if (!Tile.IsSpecial(ingredient))
            {
                availableIngredients.Add(ingredient);
            }
        }
        
        if (availableIngredients.Count > 0)
        {
            return availableIngredients[Random.Range(0, availableIngredients.Count)];
        }
        
        // Final fallback to Tomato if something goes wrong
        return Tile.TileType.Tomato;
    }
    
    /// <summary>
    /// Public method to attempt tile swap - called from TouchInputController
    /// </summary>
    public void TrySwapTiles(Tile tile1, Tile tile2)
    {
        if (isSwapping || tile1 == null || tile2 == null) return;
        
        if (AreAdjacent(tile1, tile2))
        {
            TrySwapTilesWithAnimation(tile1, tile2);
        }
    }
    
    /// <summary>
    /// Check if input is currently locked (during animations)
    /// </summary>
    public bool IsInputLocked()
    {
        return isSwapping;
    }
    
    /// <summary>
    /// Check if two tiles are adjacent (horizontally or vertically)
    /// </summary>
    private bool AreAdjacent(Tile tile1, Tile tile2)
    {
        int deltaX = Mathf.Abs(tile1.gridX - tile2.gridX);
        int deltaY = Mathf.Abs(tile1.gridY - tile2.gridY);
        
        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }
    
    /// <summary>
    /// Try to swap two tiles and check for matches using DOTween
    /// </summary>
    private void TrySwapTilesWithAnimation(Tile tile1, Tile tile2)
    {
        isSwapping = true;
        
        // Swap positions in grid
        Vector3 tile1Pos = tile1.transform.position;
        Vector3 tile2Pos = tile2.transform.position;
        
        int temp1X = tile1.gridX, temp1Y = tile1.gridY;
        int temp2X = tile2.gridX, temp2Y = tile2.gridY;
        
        // Update grid array
        grid[temp1X, temp1Y] = tile2;
        grid[temp2X, temp2Y] = tile1;
        
        // Update tile positions
        tile1.SetGridPosition(temp2X, temp2Y);
        tile2.SetGridPosition(temp1X, temp1Y);
        
        // Animate swap with DOTween
        DOTween.Sequence()
            .Join(tile1.transform.DOMove(tile2Pos, swapDuration).SetEase(Ease.InOutQuad))
            .Join(tile2.transform.DOMove(tile1Pos, swapDuration).SetEase(Ease.InOutQuad))
            .OnComplete(() => {
                // Check for matches
                bool hasMatches = CheckForMatches();
                
                if (hasMatches)
                {
                    // Valid move - process matches
                    ProcessMatchesWithAnimation();
                }
                else
                {
                    // Invalid move - swap back
                    grid[temp1X, temp1Y] = tile1;
                    grid[temp2X, temp2Y] = tile2;
                    
                    tile1.SetGridPosition(temp1X, temp1Y);
                    tile2.SetGridPosition(temp2X, temp2Y);
                    
                    // Animate swap back
                    DOTween.Sequence()
                        .Join(tile1.transform.DOMove(tile1Pos, swapDuration).SetEase(Ease.InOutQuad))
                        .Join(tile2.transform.DOMove(tile2Pos, swapDuration).SetEase(Ease.InOutQuad))
                        .OnComplete(() => {
                            isSwapping = false;
                        });
                }
            });
    }
    
    /// <summary>
    /// Check if there are any matches on the grid
    /// </summary>
    private bool CheckForMatches()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsPartOfMatch(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Check if a tile at given position is part of a match
    /// </summary>
    private bool IsPartOfMatch(int x, int y)
    {
        if (grid[x, y] == null) return false;
        
        Tile.TileType tileType = grid[x, y].Type;
        
        // Check horizontal match
        int horizontalCount = 1;
        
        // Count left
        for (int i = x - 1; i >= 0 && grid[i, y] != null && grid[i, y].Type == tileType; i--)
        {
            horizontalCount++;
        }
        
        // Count right
        for (int i = x + 1; i < gridWidth && grid[i, y] != null && grid[i, y].Type == tileType; i++)
        {
            horizontalCount++;
        }
        
        // Check vertical match
        int verticalCount = 1;
        
        // Count down
        for (int j = y - 1; j >= 0 && grid[x, j] != null && grid[x, j].Type == tileType; j--)
        {
            verticalCount++;
        }
        
        // Count up
        for (int j = y + 1; j < gridHeight && grid[x, j] != null && grid[x, j].Type == tileType; j++)
        {
            verticalCount++;
        }
        
        return horizontalCount >= 3 || verticalCount >= 3;
    }
    
    /// <summary>
    /// Process all matches and handle tile clearing/refilling with DOTween animations
    /// </summary>
    private void ProcessMatchesWithAnimation()
    {
        if (!CheckForMatches())
        {
            isSwapping = false;
            OnGridRefilled?.Invoke();
            return;
        }
        
        // Find matches
        List<Tile> tilesToClear = FindMatchingTiles();
        Dictionary<int, List<int>> clearPositions = new Dictionary<int, List<int>>();
        
        // Organize positions for clearance
        foreach (Tile tile in tilesToClear)
        {
            if (!clearPositions.ContainsKey(tile.gridX))
            {
                clearPositions.Add(tile.gridX, new List<int>());
            }
            clearPositions[tile.gridX].Add(tile.gridY);
        }
        
        // Create clear animation sequence
        Sequence clearSequence = DOTween.Sequence();
        
        // Animate and clear matched tiles
        foreach (Tile tile in tilesToClear)
        {
            if (tile != null)
            {
                // Store the positions before clearing
                int x = tile.gridX;
                int y = tile.gridY;
                
                // Animate tile clearing (scale down and fade out)
                clearSequence.Join(tile.transform.DOScale(0.1f, matchClearDuration).SetEase(Ease.InBack));
                clearSequence.Join(tile.GetComponent<SpriteRenderer>().DOFade(0, matchClearDuration).SetEase(Ease.InQuad));
            }
        }
        
        // After all clear animations finish, destroy tiles and mark grid slots as empty
        clearSequence.OnComplete(() => {
            // Actually destroy the tiles and update grid
            foreach (Tile tile in tilesToClear)
            {
                if (tile != null)
                {
                    int x = tile.gridX;
                    int y = tile.gridY;
                    
                    if (grid[x, y] == tile) // Double check it's still the same tile
                    {
                        Destroy(tile.gameObject);
                        grid[x, y] = null;
                    }
                }
            }
            
            // Notify about cleared tiles
            OnTilesCleared?.Invoke(tilesToClear.Count);
            
            // Start dropping tiles with a small delay
            DOTween.Sequence()
                .AppendInterval(delayBetweenActions)
                .OnComplete(() => DropTilesWithAnimation());
        });
        
        // Play the clear sequence
        clearSequence.Play();
    }
    
    /// <summary>
    /// Find all tiles that are part of matches
    /// </summary>
    private List<Tile> FindMatchingTiles()
    {
        HashSet<Tile> matchingTiles = new HashSet<Tile>();
        
        // Find horizontal matches
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth - 2; x++)
            {
                if (grid[x, y] != null && grid[x + 1, y] != null && grid[x + 2, y] != null)
                {
                    if (grid[x, y].Type == grid[x + 1, y].Type && grid[x, y].Type == grid[x + 2, y].Type)
                    {
                        // Found a horizontal match of at least 3
                        matchingTiles.Add(grid[x, y]);
                        matchingTiles.Add(grid[x + 1, y]);
                        matchingTiles.Add(grid[x + 2, y]);
                        
                        // Check for more in this match
                        for (int i = x + 3; i < gridWidth; i++)
                        {
                            if (grid[i, y] != null && grid[i, y].Type == grid[x, y].Type)
                            {
                                matchingTiles.Add(grid[i, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        // Find vertical matches
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight - 2; y++)
            {
                if (grid[x, y] != null && grid[x, y + 1] != null && grid[x, y + 2] != null)
                {
                    if (grid[x, y].Type == grid[x, y + 1].Type && grid[x, y].Type == grid[x, y + 2].Type)
                    {
                        // Found a vertical match of at least 3
                        matchingTiles.Add(grid[x, y]);
                        matchingTiles.Add(grid[x, y + 1]);
                        matchingTiles.Add(grid[x, y + 2]);
                        
                        // Check for more in this match
                        for (int i = y + 3; i < gridHeight; i++)
                        {
                            if (grid[x, i] != null && grid[x, i].Type == grid[x, y].Type)
                            {
                                matchingTiles.Add(grid[x, i]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        return new List<Tile>(matchingTiles);
    }
    
    /// <summary>
    /// Drop existing tiles down to fill gaps with DOTween animations
    /// </summary>
    private void DropTilesWithAnimation()
    {
        Sequence dropSequence = DOTween.Sequence();
        bool tilesDropped = false;
        
        // Process each column bottom to top
        for (int x = 0; x < gridWidth; x++)
        {
            int emptySpaces = 0;
            
            // Count empty spaces and drop tiles
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    // Move this tile down by emptySpaces
                    int newY = y - emptySpaces;
                    
                    // Update grid
                    grid[x, newY] = grid[x, y];
                    grid[x, y] = null;
                    
                    // Update tile position in grid
                    grid[x, newY].SetGridPosition(x, newY);
                    
                    // Animate the fall
                    Vector3 targetPos = GetWorldPosition(x, newY);
                    dropSequence.Join(grid[x, newY].transform.DOMove(targetPos, fallDuration).SetEase(Ease.InBounce));
                    
                    tilesDropped = true;
                }
            }
        }
        
        // After all drops are complete, fill empty spaces
        dropSequence.OnComplete(() => {
            FillEmptySpacesWithAnimation();
        });
        
        // If no tiles were dropped, directly proceed to filling
        if (!tilesDropped)
        {
            FillEmptySpacesWithAnimation();
        }
        else
        {
            dropSequence.Play();
        }
    }
    
    /// <summary>
    /// Fill empty grid spaces with new tiles using DOTween animations
    /// </summary>
    private void FillEmptySpacesWithAnimation()
    {
        Sequence fillSequence = DOTween.Sequence();
        Vector3 gridCenter = new Vector3((gridWidth - 1) * tileSpacing * 0.5f, (gridHeight - 1) * tileSpacing * 0.5f, 0);
        bool tilesFilled = false;
        
        for (int x = 0; x < gridWidth; x++)
        {
            int emptyCount = 0;
            
            // Count empty spaces in this column
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    emptyCount++;
                    
                    // Create new tile
                    CreateTile(x, y, gridCenter);
                    
                    // Position above screen and animate down
                    Vector3 startPos = GetWorldPosition(x, gridHeight + emptyCount);
                    Vector3 endPos = GetWorldPosition(x, y);
                    
                    grid[x, y].transform.position = startPos;
                    
                    // Animate new tile falling in
                    fillSequence.Join(grid[x, y].transform.DOMove(endPos, fallDuration).SetEase(Ease.InBounce));
                    tilesFilled = true;
                }
            }
        }
        
        // After filling, check for matches again
        fillSequence.AppendInterval(delayBetweenActions);
        fillSequence.OnComplete(() => {
            // Check if we have new matches after filling
            if (CheckForMatches())
            {
                ProcessMatchesWithAnimation();
            }
            else
            {
                // No more matches, we're done
                isSwapping = false;
                OnGridRefilled?.Invoke();
            }
        });
        
        if (tilesFilled)
        {
            fillSequence.Play();
        }
        else
        {
            // No tiles filled, check for matches directly
            if (CheckForMatches())
            {
                ProcessMatchesWithAnimation();
            }
            else
            {
                isSwapping = false;
                OnGridRefilled?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Get world position for grid coordinates
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 gridCenter = new Vector3((gridWidth - 1) * tileSpacing * 0.5f, (gridHeight - 1) * tileSpacing * 0.5f, 0);
        return new Vector3(x * tileSpacing, y * tileSpacing, 0) - gridCenter;
    }
    
    /// <summary>
    /// Get the current grid state (for debugging)
    /// </summary>
    public Tile[,] GetGrid()
    {
        return grid;
    }
}