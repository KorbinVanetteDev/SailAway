using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipSelectionUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject shipSelectionPanel;
    
    [Header("Ship Display")]
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipDescriptionText;
    public Image shipImageDisplay;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI statsText;
    
    [Header("Buttons")]
    public Button selectButton;
    public Button purchaseButton;
    public Button nextButton;
    public Button previousButton;
    public Button closeButton;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.H;
    
    private int currentShipIndex = 0;
    private bool isOpen = false;
    
    void Start()
    {
        // Make sure panel is hidden at start
        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("shipSelectionPanel is not assigned in Inspector!");
        }
        
        // Connect button events
        ConnectButtons();
    }
    
    void Update()
    {
        // Toggle UI with H key (or configured key)
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleShipSelection();
        }
    }
    

    // Connect button click events
    void ConnectButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextShip);
        }
        
        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(PreviousShip);
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectShip);
        }
        
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchaseShip);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShipSelection);
        }
    }
    

    // Toggle ship selection UI open/closed
    public void ToggleShipSelection()
    {
        
        if (shipSelectionPanel == null)
        {
            Debug.LogError("shipSelectionPanel is NULL!");
            return;
        }
        
        isOpen = !isOpen;
        shipSelectionPanel.SetActive(isOpen);
        
        
        if (isOpen)
        {
            // Show cursor when UI opens
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Set to current ship when opening
            if (ShipManager.Instance != null)
            {
                currentShipIndex = ShipManager.Instance.currentShipIndex;
            }
            
            // Update display
            UpdateShipDisplay();
        }
        else
        {
            // Hide cursor when UI closes (if in game)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    

    // Close ship selection UI
    public void CloseShipSelection()
    {
        if (shipSelectionPanel != null)
        {
            isOpen = false;
            shipSelectionPanel.SetActive(false);
            
            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // Update the ship display UI
    void UpdateShipDisplay()
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogError("ShipManager.Instance is NULL!");
            return;
        }
        
        if (ShipManager.Instance.allShips == null || ShipManager.Instance.allShips.Count == 0)
        {
            Debug.LogError("ShipManager.allShips is null or empty!");
            return;
        }
        
        if (currentShipIndex < 0 || currentShipIndex >= ShipManager.Instance.allShips.Count)
        {
            Debug.LogWarning($"Invalid currentShipIndex: {currentShipIndex}, clamping to 0");
            currentShipIndex = 0;
        }
        
        ShipData ship = ShipManager.Instance.allShips[currentShipIndex];
        
        if (ship == null)
        {
            Debug.LogError($"Ship at index {currentShipIndex} is NULL!");
            return;
        }
        
        
        // Update ship name
        if (shipNameText != null)
        {
            shipNameText.text = ship.shipName;
        }
        else
        {
            Debug.LogWarning("shipNameText is not assigned!");
        }
        
        // Update ship image
        if (shipImageDisplay != null)
        {
            if (ship.shipImage != null)
            {
                shipImageDisplay.sprite = ship.shipImage;
                shipImageDisplay.enabled = true;
                shipImageDisplay.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"Ship {ship.shipName} has no shipImage assigned");
                shipImageDisplay.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("shipImageDisplay is not assigned!");
        }
        
        // Update description
        if (shipDescriptionText != null)
        {
            shipDescriptionText.text = ship.description;
        }
        else
        {
            Debug.LogWarning("shipDescriptionText is not assigned!");
        }
        
        // Update cost display
        if (costText != null)
        {
            string costString = "";
            
            //if (ship.coinCost > 0) costString += $"{ship.coinCost} Coins";
            if (ship.woodCost > 0) costString += (costString.Length > 0 ? "\n" : "") + $"{ship.woodCost} Wood";
            if (ship.ironCost > 0) costString += (costString.Length > 0 ? "\n" : "") + $"{ship.ironCost} Iron";
            if (ship.clothCost > 0) costString += (costString.Length > 0 ? "\n" : "") + $"{ship.clothCost} Cloth";
            
            if (costString.Length == 0) costString = "FREE";
            
            costText.text = costString;
        }
        else
        {
            Debug.LogWarning("costText is not assigned!");
        }
        
        // Update stats (optional)
        if (statsText != null)
        {
            statsText.text = $"Speed: {ship.maxSpeed}";
        }
        
        // Update buttons
        UpdateButtons();
    }
    

    // Update button states (enabled/disabled, text)
    void UpdateButtons()
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogWarning("ShipManager.Instance is NULL in UpdateButtons");
            return;
        }
        
        if (currentShipIndex < 0 || currentShipIndex >= ShipManager.Instance.allShips.Count)
        {
            Debug.LogWarning($"Invalid currentShipIndex in UpdateButtons: {currentShipIndex}");
            return;
        }
        
        ShipData ship = ShipManager.Instance.allShips[currentShipIndex];
        
        bool isUnlocked = ShipManager.Instance.IsShipUnlocked(currentShipIndex);
        bool isCurrent = (ShipManager.Instance.currentShipIndex == currentShipIndex);
        bool canAfford = CanAffordCurrentShip();
        
        
        // Select button (only show if unlocked and not current)
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(isUnlocked && !isCurrent);
            selectButton.interactable = true;
            
            TextMeshProUGUI selectText = selectButton.GetComponentInChildren<TextMeshProUGUI>();
            if (selectText != null)
            {
                selectText.text = "SELECT";
            }
        }
        
        // Purchase button (only show if locked)
        if (purchaseButton != null)
        {
            purchaseButton.gameObject.SetActive(!isUnlocked);
            purchaseButton.interactable = canAfford;
            
            TextMeshProUGUI purchaseText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (purchaseText != null)
            {
                if (canAfford)
                {
                    purchaseText.text = "PURCHASE";
                }
                else
                {
                    purchaseText.text = "CAN'T AFFORD";
                }
            }
        }
        
        // Show current indicator in name
        if (isCurrent && shipNameText != null)
        {
            shipNameText.text = ship.shipName + " [CURRENT]";
        }
    }
    

    // Check if player can afford current ship
    bool CanAffordCurrentShip()
    {
        if (ShipManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogWarning("ShipManager or GameManager is NULL");
            return false;
        }
        
        return ShipManager.Instance.CanAffordShip(currentShipIndex);
    }
    

    // Navigate to next ship
    public void NextShip()
    {
        if (ShipManager.Instance == null || ShipManager.Instance.allShips == null)
        {
            Debug.LogError("Cannot navigate - ShipManager or allShips is NULL");
            return;
        }
        
        currentShipIndex++;
        if (currentShipIndex >= ShipManager.Instance.allShips.Count)
        {
            currentShipIndex = 0;
        }
        
        UpdateShipDisplay();
    }
    

    // Navigate to previous ship
    public void PreviousShip()
    {
        if (ShipManager.Instance == null || ShipManager.Instance.allShips == null)
        {
            Debug.LogError("Cannot navigate - ShipManager or allShips is NULL");
            return;
        }
        
        currentShipIndex--;
        if (currentShipIndex < 0)
        {
            currentShipIndex = ShipManager.Instance.allShips.Count - 1;
        }
        
        UpdateShipDisplay();
    }
    

    // Select the current ship (set as active)
    public void OnSelectShip()
    {
        
        if (ShipManager.Instance == null)
        {
            Debug.LogError("ShipManager.Instance is NULL!");
            return;
        }
        
        if (!ShipManager.Instance.IsShipUnlocked(currentShipIndex))
        {
            Debug.LogWarning("Cannot select - ship is not unlocked!");
            return;
        }
        
        ShipManager.Instance.SetCurrentShip(currentShipIndex);
        
        UpdateShipDisplay();
    }
    

    // Purchase the current ship
    public void OnPurchaseShip()
    {
        
        if (ShipManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("ShipManager or GameManager is NULL!");
            return;
        }
        
        if (currentShipIndex < 0 || currentShipIndex >= ShipManager.Instance.allShips.Count)
        {
            Debug.LogError($"Invalid currentShipIndex: {currentShipIndex}");
            return;
        }
        
        ShipData ship = ShipManager.Instance.allShips[currentShipIndex];
        
        // Check if already unlocked
        if (ShipManager.Instance.IsShipUnlocked(currentShipIndex))
        {
            Debug.LogWarning("Ship is already unlocked!");
            return;
        }
        
        // Check if can afford
        if (!CanAffordCurrentShip())
        {
            Debug.LogWarning("Cannot afford this ship!");
            return;
        }
        
        // Purchase the ship (ShipManager handles coin deduction)
        bool success = ShipManager.Instance.PurchaseShip(currentShipIndex);
        
        if (success)
        {
            // Deduct other resources
            GameManager.Instance.wood -= ship.woodCost;
            GameManager.Instance.iron -= ship.ironCost;
            GameManager.Instance.cloth -= ship.clothCost;
            
            
            UpdateShipDisplay();
        }
        else
        {
            Debug.LogError("Purchase failed!");
        }
    }
}