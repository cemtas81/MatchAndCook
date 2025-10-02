using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    [SerializeField] private GameObject powerUpTilePrefab; // Power-up tile prefab
    [SerializeField] private Transform gridParent;

    [Header("Animation Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float matchClearDuration = 0.2f;
    [SerializeField] private float delayBetweenActions = 0.3f;

    [Header("Power-Up Settings")]
    [SerializeField] private int combo4Threshold = 4; // Minimum tiles for special power-ups
    [SerializeField] private int combo5Threshold = 5; // Threshold for more powerful combos
    [SerializeField] private int combo6Threshold = 6; // Threshold for most powerful combos

    [Header("Performance Settings")]
    [SerializeField] private int maxLargeMatchThreshold = 15; // Büyük eþleþme limiti
    [SerializeField] private int maxFrameProcessingTiles = 8; // Frame baþýna iþlenecek tile
    [SerializeField] private bool useOptimizedLargeMatches = true; // Optimizasyon açýk/kapalý

    // Grid data
    private Tile[,] grid;
    private bool isSwapping = false;

    // Performance tracking
    private bool isProcessingLargeMatch = false;
    private Coroutine currentMatchProcessing = null;

    // Events
    public System.Action<int> OnTilesCleared;
    public System.Action OnGridRefilled;

    // Pizza order integration
    private PizzaOrderManager pizzaOrderManager;
    // Power-up manager integration
    private PowerUpManager powerUpManager;
    private void Awake()
    {
        // Find pizza order manager for ingredient coordination
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        // Find power-up manager for special tile creation
        powerUpManager = FindFirstObjectByType<PowerUpManager>();
    }

    void Start()
    {
        // Pizza sipariþinin baþlatýlmasýný bekle
        StartCoroutine(InitializeGridWhenReady());
    }

    /// <summary>
    /// Pizza sipariþi hazýr olana kadar bekle, sonra grid'i baþlat
    /// </summary>
    private IEnumerator InitializeGridWhenReady()
    {
        // PizzaOrderManager'ýn hazýr olmasýný bekle
        while (pizzaOrderManager == null || !pizzaOrderManager.IsOrderActive)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Þimdi grid'i baþlat
        InitializeGrid();
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
    /// Ensure no matches exist at game start - simple solution for 2 ingredients
    /// </summary>
    private void EnsureNoInitialMatches()
    {
        // Get required ingredients for current order
        List<Tile.TileType> requiredIngredients = new List<Tile.TileType>();
        if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
        {
            requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
        }

        // If we have exactly 2 ingredients, use simple alternating pattern
        if (requiredIngredients.Count == 2)
        {
            PlaceAlternatingPattern(requiredIngredients);
            return;
        }

        // For other cases, use original method but limit to required ingredients only
        EnsureNoMatchesWithLimitedTypes(requiredIngredients);
    }

    /// <summary>
    /// Place tiles in alternating pattern - works perfectly for 2 types
    /// </summary>
    private void PlaceAlternatingPattern(List<Tile.TileType> ingredients)
    {
        Tile.TileType type1 = ingredients[0];
        Tile.TileType type2 = ingredients[1];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    // Simple alternating: even positions = type1, odd positions = type2
                    bool useType1 = (x + y) % 2 == 0;
                    Tile.TileType selectedType = useType1 ? type1 : type2;
                    grid[x, y].Initialize(selectedType, x, y);
                }
            }
        }
    }

    /// <summary>
    /// Original method but limited to required ingredients only
    /// </summary>
    private void EnsureNoMatchesWithLimitedTypes(List<Tile.TileType> allowedTypes)
    {
        if (allowedTypes.Count == 0) return;

        bool hasMatches = true;
        int attempts = 0;

        while (hasMatches && attempts < 50) // Reduced attempts since we have fewer types
        {
            hasMatches = false;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (IsPartOfMatch(x, y))
                    {
                        // Pick different type from allowed types
                        Tile.TileType currentType = grid[x, y].Type;
                        Tile.TileType newType = allowedTypes.Find(t => t != currentType);
                        if (newType == default(Tile.TileType)) newType = allowedTypes[0]; // Fallback

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
    /// </summary>
    private Tile.TileType GetRandomPizzaIngredient()
    {
        // DEBUG EKLEME
        string currentOrderName = pizzaOrderManager?.CurrentOrder?.pizzaName ?? "NULL";
        bool isActive = pizzaOrderManager?.IsOrderActive ?? false;
        
        Debug.Log($"[GridManager] GetRandomPizzaIngredient - Order: {currentOrderName}, Active: {isActive}");
        
        List<Tile.TileType> requiredIngredients = null;
        if (pizzaOrderManager != null && pizzaOrderManager.IsOrderActive)
        {
            requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
            Debug.Log($"[GridManager] Required ingredients: {string.Join(", ", requiredIngredients)}");
        }
        else
        {
            Debug.Log("[GridManager] NO ACTIVE ORDER - Using fallback!");
            
            // YENÝ: Fallback durumunda da son sipariþ malzemelerini kullan
            if (pizzaOrderManager != null && pizzaOrderManager.CurrentOrder != null)
            {
                requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
                Debug.Log($"[GridManager] FALLBACK using last order ingredients: {string.Join(", ", requiredIngredients)}");
            }
        }

        if (requiredIngredients != null && requiredIngredients.Count > 0)
        {
            Tile.TileType selected = requiredIngredients[Random.Range(0, requiredIngredients.Count)];
            Debug.Log($"[GridManager] Selected ingredient: {selected}");
            return selected;
        }

        // Fallback durumda hangi malzemeleri kullandýðýný da görelim
        Debug.Log("[GridManager] FINAL FALLBACK MODE - Using all ingredients!");
        
        // Geri kalan kod ayný...
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
            .OnComplete(() =>
            {
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
                        .OnComplete(() =>
                        {
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

        // Eðer zaten büyük bir eþleþme iþleniyorsa, bekle
        if (isProcessingLargeMatch)
        {
            return;
        }

        List<Tile> tilesToClear = FindMatchingTiles();
        
        // PERFORMANCE CHECK - Büyük eþleþmeleri tespit et
        if (useOptimizedLargeMatches && tilesToClear.Count > maxLargeMatchThreshold)
        {
            Debug.Log($"[PERFORMANCE] Large match detected: {tilesToClear.Count} tiles. Using optimized processing.");
            if (currentMatchProcessing != null)
            {
                StopCoroutine(currentMatchProcessing);
            }
            currentMatchProcessing = StartCoroutine(ProcessLargeMatchOptimized(tilesToClear));
            return;
        }

        // Normal processing for small matches
        ProcessNormalMatch(tilesToClear);
    }

    /// <summary>
    /// Process normal match (for small matches) - separate method for clarity
    /// </summary>
    private void ProcessNormalMatch(List<Tile> tilesToClear)
    {
        // Find matches
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

        // POWER-UP SYSTEM: Detect combos and determine power-up spawn location
        Vector2Int? powerUpSpawnPosition = null;
        Tile.TileType powerUpType = Tile.TileType.Bomb;

        if (tilesToClear.Count >= combo4Threshold)
        {
            // Calculate pivot position (center of match)
            int sumX = 0, sumY = 0;
            foreach (Tile tile in tilesToClear)
            {
                sumX += tile.gridX;
                sumY += tile.gridY;
            }
            int pivotX = sumX / tilesToClear.Count;
            int pivotY = sumY / tilesToClear.Count;
            powerUpSpawnPosition = new Vector2Int(pivotX, pivotY);

            // Determine power-up type based on combo size
            if (tilesToClear.Count >= combo6Threshold)
            {
                powerUpType = Tile.TileType.Star; // Star: destroys all of one type
            }
            else if (tilesToClear.Count >= combo5Threshold)
            {
                powerUpType = Tile.TileType.Rainbow; // Rainbow: matches with anything
            }
            else
            {
                // Check if match is horizontal or vertical for Lightning
                bool isHorizontal = CheckIfHorizontalMatch(tilesToClear);
                if (isHorizontal)
                {
                    powerUpType = Tile.TileType.Lightning; // Lightning: destroys row/column
                }
                else
                {
                    powerUpType = Tile.TileType.Bomb; // Bomb: destroys 3x3
                }
            }
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
        Vector2Int? finalPowerUpPosition = powerUpSpawnPosition;
        Tile.TileType finalPowerUpType = powerUpType;

        clearSequence.OnComplete(() =>
        {
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

            // POWER-UP SYSTEM: Spawn power-up tile at pivot position
            if (finalPowerUpPosition.HasValue && IsValidPosition(finalPowerUpPosition.Value.x, finalPowerUpPosition.Value.y))
            {
                CreatePowerUpTile(finalPowerUpPosition.Value.x, finalPowerUpPosition.Value.y, finalPowerUpType);
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
                    // Move this tile down to fill the gap
                    int newY = y - emptySpaces;

                    // Update grid
                    grid[x, newY] = grid[x, y];
                    grid[x, y] = null;

                    // Update tile position in grid
                    grid[x, newY].SetGridPosition(x, newY);

                    // Animate the fall
                    Vector3 targetPos = GetWorldPosition(x, newY);
                    // Animate the fall - YUMUÞAK VE SMOOTH
                    dropSequence.Join(grid[x, newY].transform.DOMove(targetPos, fallDuration)
                        .SetEase(Ease.InOutQuad)); // Çok yumuþak geçiþ

                    tilesDropped = true;
                }
            }
        }

        // After all drops are complete, fill empty spaces
        dropSequence.OnComplete(() =>
        {
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

                    // Create new tile - BURADA DEÐÝÞÝKLÝK
                    CreateTile(x, y, gridCenter);

                    // Position above screen and animate down
                    Vector3 startPos = GetWorldPosition(x, gridHeight + emptyCount);
                    Vector3 endPos = GetWorldPosition(x, y);

                    grid[x, y].transform.position = startPos;

                    // Animate new tile falling in
                    fillSequence.Join(grid[x, y].transform.DOMove(endPos, fallDuration).SetEase(Ease.OutCubic));
                    tilesFilled = true;
                }
            }
        }

        // After filling, check for matches again
        fillSequence.AppendInterval(delayBetweenActions);
        fillSequence.OnComplete(() =>
        {
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

    /// <summary>
    /// Check if a position is valid on the grid
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    /// <summary>
    /// Check if a match is primarily horizontal (for determining power-up type)
    /// </summary>
    private bool CheckIfHorizontalMatch(List<Tile> tiles)
    {
        if (tiles.Count < 2) return false;

        // Check if most tiles share the same Y coordinate (horizontal)
        Dictionary<int, int> yCoordCounts = new Dictionary<int, int>();
        foreach (Tile tile in tiles)
        {
            if (!yCoordCounts.ContainsKey(tile.gridY))
                yCoordCounts[tile.gridY] = 0;
            yCoordCounts[tile.gridY]++;
        }

        int maxYCount = 0;
        foreach (var count in yCoordCounts.Values)
        {
            if (count > maxYCount)
                maxYCount = count;
        }

        // If more than half tiles are on the same row, it's horizontal
        return maxYCount > tiles.Count / 2;
    }

    /// <summary>
    /// Create a power-up tile at specified position
    /// </summary>
    private void CreatePowerUpTile(int x, int y, Tile.TileType powerUpType)
    {
        if (grid[x, y] != null) return; // Position not empty

        Vector3 gridCenter = new Vector3((gridWidth - 1) * tileSpacing * 0.5f, (gridHeight - 1) * tileSpacing * 0.5f, 0);
        Vector3 position = new Vector3(x * tileSpacing, y * tileSpacing, 0) - gridCenter;

        // Use power-up prefab if available, otherwise use regular tile prefab
        GameObject prefabToUse = powerUpTilePrefab != null ? powerUpTilePrefab : tilePrefab;
        GameObject tileObject = Instantiate(prefabToUse, position, Quaternion.identity, gridParent);

        Tile tile = tileObject.GetComponent<Tile>();
        if (tile == null)
        {
            tile = tileObject.AddComponent<Tile>();
        }

        // Ensure components exist
        if (tileObject.GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = tileObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
        }

        if (tileObject.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = tileObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }

        // Initialize as power-up tile
        tile.Initialize(powerUpType, x, y);
        grid[x, y] = tile;

        // Play special creation effect
        tile.transform.localScale = Vector3.zero;
        tile.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Add pulsing animation to power-up tiles
                tile.transform.DOScale(Vector3.one * 1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });

        Debug.Log($"Created {powerUpType} power-up tile at ({x}, {y})");
    }

    /// <summary>
    /// Activate power-up tile effect (called when power-up is clicked)
    /// </summary>
    public void ActivatePowerUpTile(Tile powerUpTile)
    {
        if (powerUpTile == null || !powerUpTile.IsSpecialTile) return;

        int x = powerUpTile.gridX;
        int y = powerUpTile.gridY;

        // Play activation animation
        powerUpTile.ActivateSpecialEffect();

        // Determine which tiles to destroy based on power-up type
        List<Tile> tilesToDestroy = new List<Tile>();

        switch (powerUpTile.Type)
        {
            case Tile.TileType.Bomb:
                // 3x3 area around the bomb
                tilesToDestroy = GetTilesInRadius(x, y, 1);
                break;

            case Tile.TileType.Lightning:
                // Entire row or column
                if (Random.value > 0.5f)
                    tilesToDestroy = GetTilesInRow(y);
                else
                    tilesToDestroy = GetTilesInColumn(x);
                break;

            case Tile.TileType.Star:
                // Entire column (changed from original spec for variety)
                tilesToDestroy = GetTilesInColumn(x);
                break;

            case Tile.TileType.Rainbow:
                // All tiles of the same type as a random tile
                Tile.TileType targetType = GetRandomIngredientTypeOnGrid();
                if (targetType != Tile.TileType.Tomato || HasTileType(targetType))
                {
                    tilesToDestroy = GetAllTilesOfType(targetType);
                }
                break;
        }

        // Destroy the power-up tile itself
        if (!tilesToDestroy.Contains(powerUpTile))
            tilesToDestroy.Add(powerUpTile);

        // Animate and destroy tiles
        DestroyTilesWithAnimation(tilesToDestroy);
    }

    /// <summary>
    /// Get all tiles in a radius around position
    /// </summary>
    private List<Tile> GetTilesInRadius(int centerX, int centerY, int radius)
    {
        List<Tile> tiles = new List<Tile>();

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsValidPosition(x, y) && grid[x, y] != null)
                {
                    tiles.Add(grid[x, y]);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Get all tiles in a row
    /// </summary>
    private List<Tile> GetTilesInRow(int row)
    {
        List<Tile> tiles = new List<Tile>();

        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] != null)
            {
                tiles.Add(grid[x, row]);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Get all tiles in a column
    /// </summary>
    private List<Tile> GetTilesInColumn(int column)
    {
        List<Tile> tiles = new List<Tile>();

        for (int y = 0; y < gridHeight; y++)
        {
            if (grid[column, y] != null)
            {
                tiles.Add(grid[column, y]);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Get all tiles of a specific type
    /// </summary>
    private List<Tile> GetAllTilesOfType(Tile.TileType type)
    {
        List<Tile> tiles = new List<Tile>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null && grid[x, y].Type == type && !grid[x, y].IsSpecialTile)
                {
                    tiles.Add(grid[x, y]);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Get a random ingredient type currently on the grid
    /// </summary>
    private Tile.TileType GetRandomIngredientTypeOnGrid()
    {
        List<Tile.TileType> typesOnGrid = new List<Tile.TileType>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null && !grid[x, y].IsSpecialTile)
                {
                    Tile.TileType type = grid[x, y].Type;
                    if (!typesOnGrid.Contains(type))
                    {
                        typesOnGrid.Add(type);
                    }
                }
            }
        }

        return typesOnGrid.Count > 0 ? typesOnGrid[Random.Range(0, typesOnGrid.Count)] : Tile.TileType.Tomato;
    }

    /// <summary>
    /// Check if grid has at least one tile of given type
    /// </summary>
    private bool HasTileType(Tile.TileType type)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null && grid[x, y].Type == type && !grid[x, y].IsSpecialTile)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Destroy tiles with animation
    /// </summary>
    private void DestroyTilesWithAnimation(List<Tile> tilesToDestroy)
    {
        if (tilesToDestroy.Count == 0) return;

        Sequence destroySequence = DOTween.Sequence();

        // Animate each tile
        foreach (Tile tile in tilesToDestroy)
        {
            if (tile != null)
            {
                destroySequence.Join(tile.transform.DOScale(0.1f, matchClearDuration).SetEase(Ease.InBack));
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    destroySequence.Join(sr.DOFade(0, matchClearDuration).SetEase(Ease.InQuad));
                }
            }
        }

        // After animation, destroy and refill
        destroySequence.OnComplete(() =>
        {
            foreach (Tile tile in tilesToDestroy)
            {
                if (tile != null)
                {
                    int x = tile.gridX;
                    int y = tile.gridY;

                    if (IsValidPosition(x, y) && grid[x, y] == tile)
                    {
                        Destroy(tile.gameObject);
                        grid[x, y] = null;
                    }
                }
            }

            // Notify and refill
            OnTilesCleared?.Invoke(tilesToDestroy.Count);

            DOTween.Sequence()
                .AppendInterval(delayBetweenActions)
                .OnComplete(() => DropTilesWithAnimation());
        });

        destroySequence.Play();
    }

    /// <summary>
    /// Coroutine to process large matches with performance optimization
    /// </summary>
    private IEnumerator ProcessLargeMatchOptimized(List<Tile> tilesToClear)
    {
        isProcessingLargeMatch = true;

        // Only process a limited number of tiles per frame
        int totalTiles = tilesToClear.Count;
        int processedTiles = 0;

        // Create lists for organizing tiles by column
        List<Tile>[] columnTiles = new List<Tile>[gridWidth];
        for (int i = 0; i < gridWidth; i++)
        {
            columnTiles[i] = new List<Tile>();
        }

        // Distribute tiles to corresponding column lists
        foreach (Tile tile in tilesToClear)
        {
            if (tile != null)
            {
                columnTiles[tile.gridX].Add(tile);
            }
        }

        // Animate and clear tiles in columns
        for (int x = 0; x < gridWidth; x++)
        {
            if (columnTiles[x].Count > 0)
            {
                // Sort tiles in this column by Y position (descending)
                columnTiles[x].Sort((a, b) => b.gridY.CompareTo(a.gridY));

                // PROCESS TILES IN BATCHES - FRAMERATE FRIENDLY
                for (int i = 0; i < columnTiles[x].Count; i++)
                {
                    Tile tile = columnTiles[x][i];

                    // Animate tile clearing (scale down and fade out)
                    tile.transform.DOScale(0.1f, matchClearDuration).SetEase(Ease.InBack);
                    tile.GetComponent<SpriteRenderer>().DOFade(0, matchClearDuration).SetEase(Ease.InQuad);

                    processedTiles++;

                    // Yield return null to spread out processing over multiple frames
                    if (processedTiles % maxFrameProcessingTiles == 0)
                    {
                        // Wait until the end of the frame to continue
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }

        // Wait a short moment after animations before destroying tiles
        yield return new WaitForSeconds(matchClearDuration);

        // Actually destroy the tiles and update grid
        for (int x = 0; x < gridWidth; x++)
        {
            foreach (Tile tile in columnTiles[x])
            {
                if (tile != null && grid[tile.gridX, tile.gridY] == tile)
                {
                    Destroy(tile.gameObject);
                    grid[tile.gridX, tile.gridY] = null;
                }
            }
        }

        // Reset the flag and invoke events
        isProcessingLargeMatch = false;
        OnTilesCleared?.Invoke(totalTiles);

        // Start dropping tiles after a delay
        DOTween.Sequence()
            .AppendInterval(delayBetweenActions)
            .OnComplete(() => DropTilesWithAnimation());
    }
}