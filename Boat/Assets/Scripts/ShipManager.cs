using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance;
    
    [Header("Ship Data")]
    public List<ShipData> allShips; // Your ship ScriptableObjects
    public int currentShipIndex = 0;
    
    [Header("Ship Spawning")]
    public Transform shipSpawnPoint; // Optional spawn location
    public GameObject currentShipInstance; // The currently spawned ship
    
    [Header("Scene Settings")]
    public string mainGameSceneName = "MainGame"; // Scene where ship spawns
    
    // Runtime unlock state (NOT in ScriptableObject)
    private bool[] shipUnlockStates;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize unlock states array
            if (allShips != null && allShips.Count > 0)
            {
                shipUnlockStates = new bool[allShips.Count];
                
                // Default: Only first ship unlocked
                shipUnlockStates[0] = true;
                for (int i = 1; i < shipUnlockStates.Length; i++)
                {
                    shipUnlockStates[i] = false;
                }
            }
            else
            {
                Debug.LogError("allShips list is empty or null!");
            }
            
            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Only spawn if we're in the MainGame scene
        if (IsInMainGameScene())
        {
            SpawnCurrentShip();
        }
        else
        {
            Debug.Log($"Not in MainGame scene - ship will not spawn");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene changes
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    

    // Called when a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainGameSceneName)
        {
            SpawnCurrentShip();
        }
        else
        {
            DespawnShip();
        }
    }
    

    // Check if currently in the MainGame scene
    bool IsInMainGameScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMainGame = currentScene == mainGameSceneName;
        return isMainGame;
    }
    

    // Despawn the current ship (when leaving MainGame scene)
    void DespawnShip()
    {
        if (currentShipInstance != null)
        {
            Destroy(currentShipInstance);
            currentShipInstance = null;
        }
    }
    

    // SHIP UNLOCK METHODS

    

    // Check if ship is unlocked
    public bool IsShipUnlocked(int index)
    {
        if (index < 0 || index >= shipUnlockStates.Length)
        {
            Debug.LogWarning($"Invalid ship index: {index}");
            return false;
        }
        return shipUnlockStates[index];
    }
    

    // Unlock a ship
    public void UnlockShip(int index)
    {
        if (index >= 0 && index < shipUnlockStates.Length)
        {
            shipUnlockStates[index] = true;
        }
        else
        {
            Debug.LogWarning($"Cannot unlock ship - invalid index: {index}");
        }
    }
    

    // Lock a ship
    public void LockShip(int index)
    {
        if (index >= 0 && index < shipUnlockStates.Length)
        {
            shipUnlockStates[index] = false;
        }
    }
    

    // Reset all ships (only first unlocked)
    public void ResetShipUnlocks()
    {
        for (int i = 0; i < shipUnlockStates.Length; i++)
        {
            shipUnlockStates[i] = (i == 0);
        }
    }
    

    // Get bitmask of unlocked ships (for save system)
    public int GetUnlockedBitmask()
    {
        int bitmask = 0;
        for (int i = 0; i < Mathf.Min(shipUnlockStates.Length, 4); i++)
        {
            if (shipUnlockStates[i])
            {
                bitmask |= (1 << i);
            }
        }
        return bitmask;
    }
    

    // Set unlocks from bitmask (for save system)
    public void SetUnlocksFromBitmask(int bitmask)
    {
        for (int i = 0; i < Mathf.Min(shipUnlockStates.Length, 4); i++)
        {
            shipUnlockStates[i] = (bitmask & (1 << i)) != 0;
        }
    }
    

    // SHIP SPAWNING
    

    // Spawn the current ship prefab as "PlayerShip" at root level
    public void SpawnCurrentShip()
    {
        // Double-check we're in the right scene
        if (!IsInMainGameScene())
        {
            Debug.LogWarning($"Cannot spawn ship - not in MainGame scene!");
            return;
        }
        
        
        if (currentShipIndex < 0 || currentShipIndex >= allShips.Count)
        {
            Debug.LogError($"Invalid currentShipIndex: {currentShipIndex}");
            return;
        }
        
        ShipData currentShip = allShips[currentShipIndex];
        
        if (currentShip == null)
        {
            Debug.LogError($"Ship at index {currentShipIndex} is NULL!");
            return;
        }
        
        /* DEBUG: Print ship data details
        Debug.Log($"--- SHIP DATA DEBUG ---");
        Debug.Log($"Ship Name: {currentShip.shipName}");
        Debug.Log($"Description: {currentShip.description}");
        Debug.Log($"Max Speed: {currentShip.maxSpeed}");
        Debug.Log($"Acceleration: {currentShip.acceleration}");
        Debug.Log($"Turn Speed: {currentShip.turnSpeed}");
        Debug.Log($"Coin Cost: {currentShip.coinCost}");
        Debug.Log($"Ship Prefab: {(currentShip.shipPrefab != null ? currentShip.shipPrefab.name : "NULL")}");
        Debug.Log($"------");*/
        
        if (currentShip.shipPrefab == null)
        {
            Debug.LogError($"Ship {currentShip.shipName} has no shipPrefab assigned!");
            return;
        }
        
        // Destroy old ship if exists
        if (currentShipInstance != null)
        {
            Destroy(currentShipInstance);
        }
        
        // Determine spawn position and rotation
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        
        if (shipSpawnPoint != null)
        {
            spawnPosition = shipSpawnPoint.position;
            spawnRotation = shipSpawnPoint.rotation;
        }
        else
        {
            spawnPosition = new Vector3(0, 1, 0);
        }
        
        // Spawn new ship
        currentShipInstance = Instantiate(currentShip.shipPrefab, spawnPosition, spawnRotation);
        
        // Rename to "PlayerShip"
        currentShipInstance.name = "PlayerShip";
        
        // Make sure it's at root level
        currentShipInstance.transform.SetParent(null);
        
        // Log components
        ShipController controller = currentShipInstance.GetComponent<ShipController>();
        Rigidbody rb = currentShipInstance.GetComponent<Rigidbody>();
        Renderer[] renderers = currentShipInstance.GetComponentsInChildren<Renderer>();
        
       
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("Ship has no renderers! It will be invisible!");
        }
        
        // APPLY SHIP STATS FROM SHIPDATA
        ApplyShipStats();
        
        // Notify camera
        NotifyCamera();
    }


    // Apply ship stats from ShipData to the spawned ship instance
    void ApplyShipStats()
    {
        if (currentShipInstance == null)
        {
            Debug.LogWarning("Cannot apply stats - currentShipInstance is null");
            return;
        }
        
        if (currentShipIndex < 0 || currentShipIndex >= allShips.Count)
        {
            Debug.LogWarning("Cannot apply stats - invalid currentShipIndex");
            return;
        }
        
        ShipData shipData = allShips[currentShipIndex];
        
        if (shipData == null)
        {
            Debug.LogWarning("Cannot apply stats - shipData is null");
            return;
        }
        
        // Get PlayerController from spawned ship
        ShipController controller = currentShipInstance.GetComponent<ShipController>();
        
        if (controller != null)
        {
            // Apply stats from ShipData
            controller.maxSpeed = shipData.maxSpeed;
            controller.acceleration = shipData.acceleration;
            controller.turnSpeed = shipData.turnSpeed;
        }
        else
        {
            Debug.LogWarning("PlayerController not found on spawned ship!");
        }
    }
    

    // Notify camera to follow the newly spawned ship
    void NotifyCamera()
    {
        if (currentShipInstance == null)
        {
            Debug.LogWarning("Cannot notify camera - currentShipInstance is null");
            return;
        }
        
        ShipCameraFollow cameraFollow = Camera.main?.GetComponent<ShipCameraFollow>();
        
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(currentShipInstance.transform);
        }
    }
    

    // SHIP SELECTION
    

    // Set the current active ship and spawn it
    public void SetCurrentShip(int shipIndex)
    {
        if (shipIndex < 0 || shipIndex >= allShips.Count)
        {
            Debug.LogError($"Invalid ship index: {shipIndex}");
            return;
        }
        
        if (!IsShipUnlocked(shipIndex))
        {
            Debug.LogWarning($"Cannot set ship {shipIndex} - not unlocked!");
            return;
        }
        
        currentShipIndex = shipIndex;
        
        // Only spawn if in MainGame scene
        if (IsInMainGameScene())
        {
            SpawnCurrentShip();
        }
        else
        {
            //Debug.Log("Not in MainGame scene - ship selection saved but not spawned yet");
        }
    }
    

    // Get the current ship data
    public ShipData GetCurrentShip()
    {
        if (currentShipIndex >= 0 && currentShipIndex < allShips.Count)
        {
            return allShips[currentShipIndex];
        }
        
        Debug.LogError($"Invalid currentShipIndex: {currentShipIndex}");
        return null;
    }
    

    // Get ship data by index
    public ShipData GetShip(int index)
    {
        if (index >= 0 && index < allShips.Count)
        {
            return allShips[index];
        }
        
        Debug.LogWarning($"Invalid ship index: {index}");
        return null;
    }
    

    // SHIP PURCHASE
    

    // Check if player can afford a ship
    public bool CanAffordShip(int shipIndex)
    {
        if (shipIndex < 0 || shipIndex >= allShips.Count)
        {
            return false;
        }
        
        if (GameManager.Instance == null)
        {
            return false;
        }
        
        ShipData ship = allShips[shipIndex];
        
        bool canAfford = GameManager.Instance.coins >= ship.coinCost &&
                        GameManager.Instance.wood >= ship.woodCost &&
                        GameManager.Instance.iron >= ship.ironCost &&
                        GameManager.Instance.cloth >= ship.clothCost;
        
        return canAfford;
    }
    

    // Purchase a ship (deduct cost and unlock)
    public bool PurchaseShip(int shipIndex)
    {
        if (shipIndex < 0 || shipIndex >= allShips.Count)
        {
            Debug.LogError($"Invalid ship index: {shipIndex}");
            return false;
        }
        
        if (IsShipUnlocked(shipIndex))
        {
            Debug.LogWarning($"Ship {shipIndex} is already unlocked!");
            return false;
        }
        
        if (!CanAffordShip(shipIndex))
        {
            Debug.LogWarning($"Cannot afford ship {shipIndex}!");
            return false;
        }
        
        ShipData ship = allShips[shipIndex];
        
        // Deduct resources
        GameManager.Instance.coins -= ship.coinCost;
        GameManager.Instance.wood -= ship.woodCost;
        GameManager.Instance.iron -= ship.ironCost;
        GameManager.Instance.cloth -= ship.clothCost;
        
        
        // Unlock ship
        UnlockShip(shipIndex);
        
        
        return true;
    }
    

    // DOCKED SHIP MODEL


    // Get the docked ship model (for DockedShipSpawner)
    public GameObject GetDockedShipModel()
    {
        if (currentShipIndex < 0 || currentShipIndex >= allShips.Count)
        {
            return null;
        }
        
        ShipData ship = allShips[currentShipIndex];
        
        if (ship.dockedShipModel != null)
        {
            return ship.dockedShipModel;
        }
        else if (ship.shipPrefab != null)
        {
            return ship.shipPrefab;
        }
        
        return null;
    }
    

    // DEBUG METHODS

    

    // Print all ship unlock states
    public void DebugPrintShipStates()
    {
        Debug.Log($"Current Ship: {currentShipIndex}");
        
        for (int i = 0; i < allShips.Count; i++)
        {
            string unlocked = IsShipUnlocked(i) ? "UNLOCKED" : "LOCKED";
            string current = (i == currentShipIndex) ? " [CURRENT]" : "";
            Debug.Log($"  Ship {i}: {allShips[i].shipName} - {unlocked}{current}");
        }
    }

    
}