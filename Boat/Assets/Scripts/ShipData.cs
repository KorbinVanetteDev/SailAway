using UnityEngine;

[CreateAssetMenu(fileName = "NewShip", menuName = "Game/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Ship Info")]
    public string shipName = "Ship";
    
    [TextArea(3, 5)]
    public string description = "A ship for sailing the seas.";
    
    public Sprite shipImage; // Ship icon/image for UI
    public Sprite shipIcon; // Alternative name (same thing)
    
    [Header("Ship Costs")]
    public float cost = 100f; // Total cost (used by save system)
    
    // Individual resource costs (for detailed display)
    public float coinCost = 100f;
    public int woodCost = 0;
    public int ironCost = 0;
    public int clothCost = 0;
    
    [Header("Ship Stats")]
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float turnSpeed = 50f;
    public int cargoCapacity = 100;
    
    [Header("Ship Models")]
    public GameObject shipPrefab; // The ship used when sailing
    public GameObject dockedShipModel; // The ship shown when docked (optional)
    
    [Header("Unlock Status (Runtime - Don't Edit!)")]
    [Tooltip("This is managed by ShipManager at runtime")]
    public bool isUnlocked = false; // NOT USED - kept for backwards compatibility
    
    void OnValidate()
    {
        // Keep shipIcon in sync with shipImage
        if (shipImage != null && shipIcon == null)
        {
            shipIcon = shipImage;
        }
        else if (shipIcon != null && shipImage == null)
        {
            shipImage = shipIcon;
        }
        
        // Keep cost in sync with coinCost (for save system compatibility)
        cost = coinCost;
    }
}