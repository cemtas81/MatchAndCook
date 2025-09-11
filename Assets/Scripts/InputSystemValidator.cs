using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Validation script to ensure the input system is working correctly.
/// Can be attached to a GameObject to help debug input issues.
/// </summary>
public class InputSystemValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private bool logInputEvents = true;
    [SerializeField] private bool validateUIRaycastTargets = true;
    
    private TouchInputController touchController;
    private GridManager gridManager;
    
    void Start()
    {
        ValidateInputSystem();
    }
    
    /// <summary>
    /// Validate the input system setup
    /// </summary>
    private void ValidateInputSystem()
    {
        Debug.Log("=== Input System Validation ===");
        
        // Check for TouchInputController
        touchController = FindObjectOfType<TouchInputController>();
        if (touchController == null)
        {
            Debug.LogError("TouchInputController not found in scene!");
        }
        else
        {
            Debug.Log("✓ TouchInputController found");
        }
        
        // Check for GridManager
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in scene!");
        }
        else
        {
            Debug.Log("✓ GridManager found");
        }
        
        // Validate UI raycast targets
        if (validateUIRaycastTargets)
        {
            ValidateUIRaycastTargets();
        }
        
        // Check camera setup
        ValidateCameraSetup();
        
        Debug.Log("=== Validation Complete ===");
    }
    
    /// <summary>
    /// Check for UI elements that might block touch input
    /// </summary>
    private void ValidateUIRaycastTargets()
    {
        Debug.Log("Checking UI Raycast Targets...");
        
        Graphic[] allGraphics = FindObjectsOfType<Graphic>();
        int blockingElements = 0;
        
        foreach (Graphic graphic in allGraphics)
        {
            if (graphic.raycastTarget && graphic.gameObject.activeInHierarchy)
            {
                // Check if this UI element might overlap with the game area
                RectTransform rectTransform = graphic.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);
                    
                    // If it's a full-screen element, it might block input
                    Vector3 size = corners[2] - corners[0];
                    if (size.magnitude > 500) // Arbitrary threshold for "large" UI elements
                    {
                        Debug.LogWarning($"Large UI element with raycast target: {graphic.name} - Consider disabling raycast target if not needed for interaction");
                        blockingElements++;
                    }
                }
            }
        }
        
        if (blockingElements == 0)
        {
            Debug.Log("✓ No problematic UI raycast targets found");
        }
        else
        {
            Debug.LogWarning($"Found {blockingElements} potentially problematic UI elements");
        }
    }
    
    /// <summary>
    /// Validate camera setup for proper input conversion
    /// </summary>
    private void ValidateCameraSetup()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("No camera found in scene!");
            return;
        }
        
        Debug.Log($"✓ Camera found: {mainCamera.name}");
        Debug.Log($"  - Position: {mainCamera.transform.position}");
        Debug.Log($"  - Orthographic: {mainCamera.orthographic}");
        
        if (mainCamera.orthographic)
        {
            Debug.Log($"  - Orthographic Size: {mainCamera.orthographicSize}");
        }
        else
        {
            Debug.Log($"  - Field of View: {mainCamera.fieldOfView}");
        }
    }
    
    /// <summary>
    /// Test input conversion manually (can be called from inspector)
    /// </summary>
    [ContextMenu("Test Input Conversion")]
    public void TestInputConversion()
    {
        if (touchController == null) return;
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null) return;
        
        // Test center screen conversion
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, Mathf.Abs(mainCamera.transform.position.z)));
        worldCenter.z = 0;
        
        Debug.Log($"Screen center {screenCenter} converts to world position {worldCenter}");
        
        // Test raycast at center
        RaycastHit2D hit = Physics2D.Raycast(worldCenter, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");
        }
        else
        {
            Debug.Log("No raycast hit at screen center");
        }
    }
    
    void Update()
    {
        if (logInputEvents && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
            
            if (mainCamera != null)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(mainCamera.transform.position.z)));
                worldPos.z = 0;
                
                Debug.Log($"Input Event - Screen: {mousePos}, World: {worldPos}");
                
                // Test raycast
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null)
                    {
                        Debug.Log($"Hit tile at grid position ({tile.gridX}, {tile.gridY})");
                    }
                    else
                    {
                        Debug.Log($"Hit object: {hit.collider.name} (not a tile)");
                    }
                }
                else
                {
                    Debug.Log("No object hit by raycast");
                }
            }
        }
    }
}