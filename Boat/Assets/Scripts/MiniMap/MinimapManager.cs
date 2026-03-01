using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;
    
    [Header("Minimap Settings")]
    public RectTransform minimapContainer;
    public RectTransform minimapContent; // Area where icons are placed
    public Image minimapBackground;
    public Image minimapBorder;
    public float minimapSize = 250f;
    
    [Header("World Settings")]
    public Vector2 worldSize = new Vector2(50f, 50f); // Your ocean scale
    public Vector2 worldCenter = Vector2.zero;
    
    [Header("Icon Prefabs")]
    public GameObject shipIconPrefab;
    public GameObject islandIconPrefab;
    
    [Header("Visual Settings")]
    public Color oceanColor = new Color(0.53f, 0.81f, 0.92f, 0.3f); // Light blue
    public Color parchmentColor = new Color(0.83f, 0.77f, 0.66f, 1f); // Tan
    public Color borderColor = new Color(0.3f, 0.2f, 0.1f, 1f); // Dark brown
    
    [Header("Toggle Settings")]
    public KeyCode toggleKey = KeyCode.M;
    public bool startVisible = true;
    public float fadeDuration = 0.3f;
    
    [Header("Optional Features")]
    public bool showCompassRose = true;
    public GameObject compassRoseObject;
    
    // Internal tracking
    private List<MinimapIcon> registeredIcons = new List<MinimapIcon>();
    private Dictionary<MinimapIcon, GameObject> iconUIObjects = new Dictionary<MinimapIcon, GameObject>();
    
    private bool isVisible = true;
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        
        // Get or add CanvasGroup for fading
        canvasGroup = minimapContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = minimapContainer.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set initial visibility
        isVisible = startVisible;
        canvasGroup.alpha = isVisible ? 1f : 0f;
        
        // Apply visual styling
        ApplyVisualStyling();
    }
    
    void Update()
    {
        // Toggle minimap with M key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMinimap();
        }
        
        // Update all icon positions
        UpdateIconPositions();
    }
    

    // Apply parchment/nautical visual styling
    void ApplyVisualStyling()
    {
        if (minimapBackground != null)
        {
            minimapBackground.color = parchmentColor;
        }
        
        if (minimapBorder != null)
        {
            minimapBorder.color = borderColor;
        }
        
        if (compassRoseObject != null)
        {
            compassRoseObject.SetActive(showCompassRose);
        }
    }
    

    // Register an icon to be tracked on minimap
    public void RegisterIcon(MinimapIcon icon)
    {
        if (registeredIcons.Contains(icon))
        {
            Debug.LogWarning($"Icon {icon.gameObject.name} already registered!");
            return;
        }
        
        registeredIcons.Add(icon);
        CreateIconUI(icon);
    }
    

    // Unregister an icon (when destroyed)
    public void UnregisterIcon(MinimapIcon icon)
    {
        if (registeredIcons.Contains(icon))
        {
            registeredIcons.Remove(icon);
            
            if (iconUIObjects.ContainsKey(icon))
            {
                Destroy(iconUIObjects[icon]);
                iconUIObjects.Remove(icon);
            }
        }
    }
    

    // Create UI representation of an icon
    void CreateIconUI(MinimapIcon icon)
    {
        GameObject iconUI = null;
        
        // Create appropriate icon based on type
        switch (icon.iconType)
        {
            case MinimapIconType.Ship:
                iconUI = CreateShipIcon();
                break;
                
            case MinimapIconType.Island:
                iconUI = CreateIslandIcon(icon.iconLabel);
                break;
                
            default:
                iconUI = CreateGenericIcon();
                break;
        }
        
        if (iconUI != null)
        {
            iconUI.transform.SetParent(minimapContent, false);
            iconUI.name = $"Icon_{icon.gameObject.name}";
            
            // Store reference
            icon.iconImage = iconUI.GetComponent<Image>();
            icon.iconRectTransform = iconUI.GetComponent<RectTransform>();
            
            // Apply color and size
            if (icon.iconImage != null)
            {
                icon.iconImage.color = icon.iconColor;
            }
            
            icon.iconRectTransform.sizeDelta = new Vector2(icon.iconSize, icon.iconSize);
            
            iconUIObjects[icon] = iconUI;
        }
    }
    

    // Create ship icon (triangle pointing up)
    GameObject CreateShipIcon()
    {
        GameObject iconObj = new GameObject("ShipIcon");
        
        Image img = iconObj.AddComponent<Image>();
        img.sprite = CreateTriangleSprite();
        img.color = Color.white;
        
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(15f, 15f);
        
        return iconObj;
    }

    // Create island icon (circle with label)
    GameObject CreateIslandIcon(string label)
    {
        GameObject iconObj = new GameObject("IslandIcon");
        
        // Circle icon
        Image img = iconObj.AddComponent<Image>();
        img.sprite = CreateCircleSprite();
        img.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green
        
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10f, 10f);
        
        // Label (if provided)
        if (!string.IsNullOrEmpty(label))
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(iconObj.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 10;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
            
            // Add outline for readability
            labelText.outlineWidth = 0.2f;
            labelText.outlineColor = Color.black;
            
            RectTransform labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.sizeDelta = new Vector2(60f, 20f);
            labelRT.anchoredPosition = new Vector2(0f, -15f); // Below icon
        }
        
        return iconObj;
    }
    
    // Create generic icon
    GameObject CreateGenericIcon()
    {
        GameObject iconObj = new GameObject("GenericIcon");
        
        Image img = iconObj.AddComponent<Image>();
        img.sprite = CreateCircleSprite();
        img.color = Color.yellow;
        
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(8f, 8f);
        
        return iconObj;
    }
    

    // Update positions of all icons on minimap
    void UpdateIconPositions()
    {
        foreach (MinimapIcon icon in registeredIcons)
        {
            if (icon == null || icon.iconRectTransform == null) continue;
            
            // Get world position
            Vector3 worldPos = icon.GetWorldPosition();
            
            // Convert to minimap position
            Vector2 minimapPos = WorldToMinimapPosition(worldPos);
            
            // Update icon position
            icon.iconRectTransform.anchoredPosition = minimapPos;
            
            // Update rotation (for ship)
            if (icon.trackRotation)
            {
                float rotation = icon.GetRotation();
                icon.iconRectTransform.localRotation = Quaternion.Euler(0f, 0f, -rotation);
            }
        }
    }
    
    // Convert world position to minimap UI position
    public Vector2 WorldToMinimapPosition(Vector3 worldPosition)
    {
        // Extract X and Z (horizontal plane)
        float worldX = worldPosition.x;
        float worldZ = worldPosition.z;
        
        // Normalize to 0-1 range based on world size
        float normalizedX = (worldX - worldCenter.x + worldSize.x / 2f) / worldSize.x;
        float normalizedZ = (worldZ - worldCenter.y + worldSize.y / 2f) / worldSize.y;
        
        // Clamp to keep within minimap bounds
        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedZ = Mathf.Clamp01(normalizedZ);
        
        // Convert to minimap pixel coordinates (centered at 0,0)
        float minimapX = (normalizedX - 0.5f) * minimapSize;
        float minimapY = (normalizedZ - 0.5f) * minimapSize;
        
        return new Vector2(minimapX, minimapY);
    }
    
    // Toggle minimap visibility
    public void ToggleMinimap()
    {
        isVisible = !isVisible;
        
        StopAllCoroutines();
        StartCoroutine(FadeMinimap(isVisible));
    }
    

    // Fade minimap in/out
    System.Collections.IEnumerator FadeMinimap(bool fadeIn)
    {
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
 
    // HELPER METHODS: CREATE SIMPLE SPRITES
    

    // Create a triangle sprite for ship icon
    Sprite CreateTriangleSprite()
    {
        // Create simple triangle texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Draw triangle (pointing up)
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Triangle shape
                float centerX = size / 2f;
                float topY = size * 0.8f;
                float bottomY = size * 0.2f;
                float width = (topY - y) / (topY - bottomY) * (size * 0.4f);
                
                if (y > bottomY && y < topY)
                {
                    if (Mathf.Abs(x - centerX) < width)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    

    // Create a circle sprite for island icons
    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (distance < radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}