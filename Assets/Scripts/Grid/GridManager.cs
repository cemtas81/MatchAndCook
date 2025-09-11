using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    [Header("Input")]
    [SerializeField] private LayerMask tileLayerMask = 1;
    
    // Grid data
    private Tile[,] grid;
    private Camera mainCamera;
    private bool isInputEnabled = true;
    private Tile selectedTile;
    
    // Events
    public System.Action<int> OnTilesCleared;
    public System.Action OnGridRefilled;
    
    void Start()
    {
        mainCamera = Camera.main;
        InitializeGrid();
    }
    
    void Update()
    {
        if (isInputEnabled)
        {
            HandleInput();
        }
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
        StartCoroutine(EnsureNoInitialMatches());
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
        
        // Random tile type
        Tile.TileType randomType = (Tile.TileType)Random.Range(0, System.Enum.GetValues(typeof(Tile.TileType)).Length);
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
    private IEnumerator EnsureNoInitialMatches()
    {
        yield return new WaitForEndOfFrame();
        
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
            yield return null;
        }
    }
    
    /// <summary>
    /// Get a random tile type that won't create matches at this position
    /// </summary>
    private Tile.TileType GetSafeRandomType(int x, int y)
    {
        List<Tile.TileType> availableTypes = new List<Tile.TileType>();
        
        foreach (Tile.TileType type in System.Enum.GetValues(typeof(Tile.TileType)))
        {
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
            return availableTypes[Random.Range(0, availableTypes.Count)];
        }
        
        // Fallback to random type
        return (Tile.TileType)Random.Range(0, System.Enum.GetValues(typeof(Tile.TileType)).Length);
    }
    
    /// <summary>
    /// Handle touch/mouse input for tile selection and swapping
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0;
            
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, tileLayerMask);
            
            if (hit.collider != null)
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile != null)
                {
                    HandleTileClick(clickedTile);
                }
            }
        }
    }
    
    /// <summary>
    /// Handle tile click/tap for selection and swapping
    /// </summary>
    private void HandleTileClick(Tile clickedTile)
    {
        if (selectedTile == null)
        {
            // First tile selection
            selectedTile = clickedTile;
            HighlightTile(selectedTile, true);
        }
        else if (selectedTile == clickedTile)
        {
            // Deselect current tile
            HighlightTile(selectedTile, false);
            selectedTile = null;
        }
        else if (AreAdjacent(selectedTile, clickedTile))
        {
            // Attempt to swap adjacent tiles
            HighlightTile(selectedTile, false);
            StartCoroutine(TrySwapTiles(selectedTile, clickedTile));
            selectedTile = null;
        }
        else
        {
            // Select new tile
            HighlightTile(selectedTile, false);
            selectedTile = clickedTile;
            HighlightTile(selectedTile, true);
        }
    }
    
    /// <summary>
    /// Highlight a tile to show it's selected
    /// </summary>
    private void HighlightTile(Tile tile, bool highlight)
    {
        if (tile != null)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = highlight ? Color.white : sr.color;
                transform.localScale = highlight ? Vector3.one * 1.1f : Vector3.one;
            }
        }
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
    /// Try to swap two tiles and check for matches
    /// </summary>
    private IEnumerator TrySwapTiles(Tile tile1, Tile tile2)
    {
        isInputEnabled = false;
        
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
        
        // Animate swap
        tile1.MoveTo(tile2Pos);
        tile2.MoveTo(tile1Pos);
        
        yield return new WaitForSeconds(0.3f);
        
        // Check for matches
        bool hasMatches = CheckForMatches();
        
        if (hasMatches)
        {
            // Valid move - process matches
            yield return StartCoroutine(ProcessMatches());
        }
        else
        {
            // Invalid move - swap back
            grid[temp1X, temp1Y] = tile1;
            grid[temp2X, temp2Y] = tile2;
            
            tile1.SetGridPosition(temp1X, temp1Y);
            tile2.SetGridPosition(temp2X, temp2Y);
            
            tile1.MoveTo(tile1Pos);
            tile2.MoveTo(tile2Pos);
            
            yield return new WaitForSeconds(0.3f);
        }
        
        isInputEnabled = true;
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
    /// Process all matches and handle tile clearing/refilling
    /// </summary>
    private IEnumerator ProcessMatches()
    {
        while (CheckForMatches())
        {
            List<Tile> tilesToClear = FindMatchingTiles();
            
            // Clear matched tiles
            foreach (Tile tile in tilesToClear)
            {
                if (tile != null)
                {
                    grid[tile.gridX, tile.gridY] = null;
                    Destroy(tile.gameObject);
                }
            }
            
            OnTilesCleared?.Invoke(tilesToClear.Count);
            
            yield return new WaitForSeconds(0.2f);
            
            // Drop tiles down
            yield return StartCoroutine(DropTiles());
            
            // Fill empty spaces
            yield return StartCoroutine(FillGrid());
            
            yield return new WaitForSeconds(0.3f);
        }
        
        OnGridRefilled?.Invoke();
    }
    
    /// <summary>
    /// Find all tiles that are part of matches
    /// </summary>
    private List<Tile> FindMatchingTiles()
    {
        List<Tile> matchingTiles = new List<Tile>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsPartOfMatch(x, y))
                {
                    matchingTiles.Add(grid[x, y]);
                }
            }
        }
        
        return matchingTiles;
    }
    
    /// <summary>
    /// Drop existing tiles down to fill gaps
    /// </summary>
    private IEnumerator DropTiles()
    {
        bool tilesDropped = false;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    // Find tile above to drop down
                    for (int above = y + 1; above < gridHeight; above++)
                    {
                        if (grid[x, above] != null)
                        {
                            grid[x, y] = grid[x, above];
                            grid[x, above] = null;
                            
                            grid[x, y].SetGridPosition(x, y);
                            
                            Vector3 newPosition = GetWorldPosition(x, y);
                            grid[x, y].FallTo(newPosition);
                            
                            tilesDropped = true;
                            break;
                        }
                    }
                }
            }
        }
        
        if (tilesDropped)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    /// <summary>
    /// Fill empty grid spaces with new tiles
    /// </summary>
    private IEnumerator FillGrid()
    {
        Vector3 gridCenter = new Vector3((gridWidth - 1) * tileSpacing * 0.5f, (gridHeight - 1) * tileSpacing * 0.5f, 0);
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    CreateTile(x, y, gridCenter);
                    
                    // Start tile above screen and animate down
                    Vector3 startPos = GetWorldPosition(x, gridHeight + 1);
                    Vector3 endPos = GetWorldPosition(x, y);
                    
                    grid[x, y].transform.position = startPos;
                    grid[x, y].FallTo(endPos);
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// Get world position for grid coordinates
    /// </summary>
    private Vector3 GetWorldPosition(int x, int y)
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
    
    /// <summary>
    /// Enable or disable input
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
    }
}