using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mobile-optimized touch input for the match-3 game.
/// Provides smooth touch interaction with visual feedback and proper mobile scaling.
/// </summary>
public class TouchInputController : MonoBehaviour
{
    [Header("Touch Settings")]
    [SerializeField] private float touchSensitivity = 1f;
    [SerializeField] private float swipeThreshold = 50f; // pixels
    [SerializeField] private LayerMask touchLayerMask = 1;
    
    [Header("Visual Feedback")]
    [SerializeField] private float touchScaleEffect = 1.1f;
    [SerializeField] private float feedbackDuration = 0.1f;
    
    // Touch tracking
    private Vector2 touchStartPos;
    private bool isTouching = false;
    private Tile selectedTile;
    private Camera mainCamera;
    private GridManager gridManager;
    
    // Events - removed for simplification as per requirements
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("TouchInputController: GridManager not found!");
        }
    }
    
    void Update()
    {
        HandleTouchInput();
    }
    
    /// <summary>
    /// Handle touch input for both mobile and desktop testing
    /// </summary>
    private void HandleTouchInput()
    {
        // Check if input is locked during animations
        if (gridManager != null && gridManager.IsInputLocked()) return;
        
        // Don't handle input if touching UI elements
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        
        // Use Unity's Input system that works for both touch and mouse
        if (Input.GetMouseButtonDown(0))
        {
            StartTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isTouching)
        {
            UpdateTouch(Input.mousePosition);
        }
    }
    
    /// <summary>
    /// Start touch interaction
    /// </summary>
    private void StartTouch(Vector2 screenPosition)
    {
        touchStartPos = screenPosition;
        isTouching = true;
        
        Vector3 worldPoint = ScreenToWorldPoint(screenPosition);
        Tile touchedTile = GetTileAtPosition(worldPoint);
        
        if (touchedTile != null)
        {
            if (selectedTile == null)
            {
                // First tile selection
                SelectTile(touchedTile);
            }
            else if (selectedTile == touchedTile)
            {
                // Deselect same tile
                DeselectTile();
            }
            else
            {
                // Attempt swap with second tile
                AttemptSwap(selectedTile, touchedTile);
            }
        }
        else
        {
            // Touched empty space - deselect current tile
            if (selectedTile != null)
            {
                DeselectTile();
            }
        }
    }
    
    /// <summary>
    /// Update touch during drag
    /// </summary>
    private void UpdateTouch(Vector2 screenPosition)
    {
        if (!isTouching || selectedTile == null) return;
        
        Vector2 touchDelta = screenPosition - touchStartPos;
        float swipeDistance = touchDelta.magnitude;
        
        // Check for swipe gesture
        if (swipeDistance >= swipeThreshold)
        {
            Vector2 swipeDirection = touchDelta.normalized;
            Tile targetTile = GetTileInDirection(selectedTile, swipeDirection);
            
            if (targetTile != null)
            {
                AttemptSwap(selectedTile, targetTile);
                isTouching = false; // Prevent multiple swipes
            }
        }
    }
    
    /// <summary>
    /// End touch interaction
    /// </summary>
    private void EndTouch(Vector2 screenPosition)
    {
        isTouching = false;
        // Touch end logic is already handled in StartTouch and UpdateTouch
    }
    
    /// <summary>
    /// Select a tile with visual feedback
    /// </summary>
    private void SelectTile(Tile tile)
    {
        selectedTile = tile;
        ApplySelectionEffect(tile, true);
        
        //Debug.Log($"Selected tile at ({tile.gridX}, {tile.gridY})");
    }
    
    /// <summary>
    /// Deselect current tile
    /// </summary>
    private void DeselectTile()
    {
        if (selectedTile != null)
        {
            ApplySelectionEffect(selectedTile, false);
            selectedTile = null;
        }
    }
    
    /// <summary>
    /// Attempt to swap two tiles via GridManager
    /// </summary>
    private void AttemptSwap(Tile tile1, Tile tile2)
    {
        if (tile1 != null && tile2 != null && AreAdjacent(tile1, tile2) && gridManager != null)
        {
            gridManager.TrySwapTiles(tile1, tile2);
            DeselectTile(); // Clear selection after swap attempt
            
            Debug.Log($"Swapping tiles ({tile1.gridX}, {tile1.gridY}) and ({tile2.gridX}, {tile2.gridY})");
        }
        else
        {
            // Invalid swap - provide feedback
            if (selectedTile != null)
            {
                InvalidSwapFeedback(selectedTile); // StartCoroutine kald�r�ld�
            }
        }
    }

    /// <summary>
    /// Apply visual selection effect to tile
    /// </summary>
    private void ApplySelectionEffect(Tile tile, bool selected)
    {
        if (tile == null) return;
        Transform t = tile.transform;

        // DOTween ile scale animasyonu
        if (selected)
        {
            t.DOScale(Vector3.one * touchScaleEffect, 0.2f)
             .SetEase(Ease.OutBack);
        }
        else
        {
            t.DOScale(Vector3.one, 0.15f)
             .SetEase(Ease.InBack);
        }
    }

    /// <summary>
    /// Convert screen position to world position - fixed for both mobile and desktop
    /// </summary>
    private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
    {
        // For 2D games, set the z distance to match the camera's distance from the game plane
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0; // Force to 2D plane
        return worldPoint;
    }

    /// <summary>
    /// Get tile at world position using raycast
    /// </summary>
    private Tile GetTileAtPosition(Vector3 worldPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, touchLayerMask);
        
        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                Debug.Log($"Hit tile at ({tile.gridX}, {tile.gridY}) - World position: {worldPosition}");
            }
            return tile;
        }
        
        Debug.Log($"No tile hit at world position: {worldPosition}");
        return null;
    }
    
    /// <summary>
    /// Get adjacent tile in swipe direction
    /// </summary>
    private Tile GetTileInDirection(Tile fromTile, Vector2 direction)
    {
        if (fromTile == null || gridManager == null) return null;
        
        int targetX = fromTile.gridX;
        int targetY = fromTile.gridY;
        
        // Determine direction (prioritize strongest axis)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal swipe
            targetX += direction.x > 0 ? 1 : -1;
        }
        else
        {
            // Vertical swipe
            targetY += direction.y > 0 ? 1 : -1;
        }
        
        // Get tile from GridManager
        Tile[,] grid = gridManager.GetGrid();
        if (targetX >= 0 && targetX < grid.GetLength(0) && 
            targetY >= 0 && targetY < grid.GetLength(1))
        {
            return grid[targetX, targetY];
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if two tiles are adjacent
    /// </summary>
    private bool AreAdjacent(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return false;
        
        int deltaX = Mathf.Abs(tile1.gridX - tile2.gridX);
        int deltaY = Mathf.Abs(tile1.gridY - tile2.gridY);
        
        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }

    /// <summary>
    /// Provide feedback for invalid swap attempt
    /// </summary>
    private void InvalidSwapFeedback(Tile tile)
    {
        ShakeTile(tile);
    }

    /// <summary>
    /// Enable or disable touch input
    /// </summary>
    public void SetTouchEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            DeselectTile();
            isTouching = false;
        }
    }
    
    /// <summary>
    /// Get currently selected tile
    /// </summary>
    public Tile GetSelectedTile()
    {
        return selectedTile;
    }
    private void ShakeTile(Tile tile)
    {
        if (tile == null) return;
        
        // Mevcut t�m shake animasyonlar�n� durdur
        DOTween.Kill(tile.transform, false);
        
        // Karonun grid pozisyonuna g�re d�nya pozisyonunu al
        Vector3 originalPosition = gridManager.GetWorldPosition(tile.gridX, tile.gridY);
        
        // Karoyu orijinal pozisyonuna yerle�tir (kayma olmu�sa d�zelt)
        tile.transform.position = originalPosition;
        
        // Basit ve g�venli bir shake animasyonu
        tile.transform.DOShakePosition(0.2f, 0.1f, 6, 90, false)
            .SetTarget(tile.transform)
            .OnComplete(() => {
                // Animasyon sonunda orijinal pozisyona d�nd�r
                tile.transform.position = originalPosition;
            });
    }

}