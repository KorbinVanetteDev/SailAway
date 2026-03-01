using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [Header("Icon Settings")]
    public MinimapIconType iconType = MinimapIconType.Ship;
    public Color iconColor = Color.white;
    public float iconSize = 15f;
    
    [Header("Label Settings")]
    public string iconLabel = "";
    public bool showLabel = true;
    
    [Header("Tracking")]
    public bool trackRotation = true; // Ship rotates, islands don't
    
    [Header("Runtime (Auto-assigned)")]
    public Image iconImage;
    public RectTransform iconRectTransform;
    
    void Start()
    {
        // Register with minimap
        if (MinimapManager.Instance != null)
        {
            MinimapManager.Instance.RegisterIcon(this);
        }
        else
        {
            Debug.LogWarning($"MinimapIcon on {gameObject.name}: MinimapManager not found!");
        }
    }
    
    void OnDestroy()
    {
        // Unregister when destroyed
        if (MinimapManager.Instance != null)
        {
            MinimapManager.Instance.UnregisterIcon(this);
        }
    }
    

    // Get world position for minimap tracking
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    

    // Get rotation for minimap icon
    public float GetRotation()
    {
        if (trackRotation)
        {
            // Return Y rotation (yaw) for top-down view
            return transform.eulerAngles.y;
        }
        return 0f;
    }
}

public enum MinimapIconType
{
    Ship,
    Island
}