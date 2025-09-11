using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to quickly set up Match & Cook scenes with proper GameObjects and components.
/// This automates the tedious process of creating the game hierarchy manually.
/// </summary>
public class SetupHelper : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool setupGameScene = true;
    [SerializeField] private bool setupMainMenu = false;
    [SerializeField] private bool createTilePrefab = true;
    
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float tileSpacing = 1.1f;
    
    #if UNITY_EDITOR
    /// <summary>
    /// Set up the current scene with game objects and components
    /// </summary>
    [ContextMenu("Setup Current Scene")]
    public void SetupCurrentScene()
    {
        if (setupGameScene)
        {
            SetupGameSceneObjects();
        }
        
        if (setupMainMenu)
        {
            SetupMainMenuObjects();
        }
        
        if (createTilePrefab)
        {
            CreateTilePrefab();
        }
        
        Debug.Log("Scene setup completed!");
    }
    
    /// <summary>
    /// Set up game scene with grid, managers, and UI
    /// </summary>
    private void SetupGameSceneObjects()
    {
        // Create Grid System
        GameObject gridManager = new GameObject("GridManager");
        gridManager.AddComponent<GridManager>();
        
        GameObject gridParent = new GameObject("GridParent");
        gridParent.transform.SetParent(gridManager.transform);
        
        // Create Game Management
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        
        GameObject scoreManager = new GameObject("ScoreManager");
        scoreManager.AddComponent<ScoreManager>();
        
        // Set up camera for 2D
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6f;
            mainCam.transform.position = new Vector3(0, 0, -10);
        }
        
        // Create UI Canvas
        SetupGameUI();
        
        Debug.Log("Game scene objects created successfully!");
    }
    
    /// <summary>
    /// Set up game UI canvas and elements
    /// </summary>
    private void SetupGameUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<UIManager>();
        
        // Create basic UI elements (placeholder text objects)
        CreateUIText(canvasGO.transform, "Score Text", new Vector3(-400, 800, 0));
        CreateUIText(canvasGO.transform, "Moves Text", new Vector3(0, 800, 0));
        CreateUIText(canvasGO.transform, "Target Text", new Vector3(400, 800, 0));
        
        // Create game end panel (inactive)
        GameObject gameEndPanel = new GameObject("Game End Panel");
        gameEndPanel.transform.SetParent(canvasGO.transform, false);
        gameEndPanel.AddComponent<UnityEngine.UI.Image>();
        gameEndPanel.SetActive(false);
        
        CreateUIText(gameEndPanel.transform, "Game End Title", Vector3.zero);
        CreateUIText(gameEndPanel.transform, "Game End Score", new Vector3(0, -100, 0));
    }
    
    /// <summary>
    /// Create a simple UI text element
    /// </summary>
    private GameObject CreateUIText(Transform parent, string name, Vector3 position)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(300, 100);
        
        UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = name;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        return textGO;
    }
    
    /// <summary>
    /// Set up main menu scene objects
    /// </summary>
    private void SetupMainMenuObjects()
    {
        // Create Menu Canvas
        GameObject canvasGO = new GameObject("Menu Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<MenuManager>();
        
        // Create menu elements
        CreateUIText(canvasGO.transform, "Game Title", new Vector3(0, 400, 0));
        CreateUIText(canvasGO.transform, "High Score", new Vector3(0, 200, 0));
        CreateUIText(canvasGO.transform, "Play Button", new Vector3(0, 0, 0));
        CreateUIText(canvasGO.transform, "Settings Button", new Vector3(0, -100, 0));
        
        Debug.Log("Main menu objects created successfully!");
    }
    
    /// <summary>
    /// Create a basic tile prefab
    /// </summary>
    private void CreateTilePrefab()
    {
        // Create tile GameObject
        GameObject tilePrefab = new GameObject("TilePrefab");
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = tilePrefab.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        
        // Add Collider
        BoxCollider2D collider = tilePrefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        
        // Add Tile script
        tilePrefab.AddComponent<Tile>();
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/TilePrefab.prefab";
        
        // Ensure Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        PrefabUtility.SaveAsPrefabAsset(tilePrefab, prefabPath);
        
        // Clean up scene
        DestroyImmediate(tilePrefab);
        
        Debug.Log($"Tile prefab created at: {prefabPath}");
    }
    
    /// <summary>
    /// Create Event System for UI input
    /// </summary>
    private void CreateEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
    
    void Start()
    {
        // Auto-setup on start if this script is in the scene
        if (Application.isEditor)
        {
            SetupCurrentScene();
            // Destroy this helper after setup
            Destroy(gameObject);
        }
    }
    
    #endif
}