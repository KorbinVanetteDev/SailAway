using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    
    [Header("Save Code Settings")]
    private const int CODE_LENGTH = 13; // 12 digits data + 1 checksum
    private const string DEFAULT_CODE = "0000000000010"; // Default new game state
    
    [HideInInspector]
    public string currentSaveCode = "";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Generate initial code for current state
        UpdateCurrentSaveCode();
    }
    

    // SAVE CODE GENERATION
    

    // Generates save code from current game state
    public string GenerateSaveCode()
    {
        try
        {
            // Get current game data
            float coins = GameManager.Instance != null ? GameManager.Instance.coins : 0f;
            int wood = GameManager.Instance != null ? GameManager.Instance.wood : 0;
            int iron = GameManager.Instance != null ? GameManager.Instance.iron : 0;
            int cloth = GameManager.Instance != null ? GameManager.Instance.cloth : 0;
        
            
            int currentShip = ShipManager.Instance != null ? ShipManager.Instance.currentShipIndex : 0;
            int unlockedShips = GetUnlockedShipsBitmask();
            
            
            // Encode data into segments (NO division for coins - store exact amount)
            int coinsEncoded = Mathf.Clamp((int)coins, 0, 9999); // Max 9999 coins
            int woodEncoded = Mathf.Clamp(wood, 0, 99);
            int ironEncoded = Mathf.Clamp(iron, 0, 99);
            int clothEncoded = Mathf.Clamp(cloth, 0, 99);
            int shipEncoded = Mathf.Clamp(currentShip, 0, 9);
            int unlockedEncoded = Mathf.Clamp(unlockedShips, 0, 15);
            
            Debug.Log($"Encoded values:");
            Debug.Log($"  Coins: {coins} → {coinsEncoded}");
            Debug.Log($"  Wood: {wood} → {woodEncoded}");
            Debug.Log($"  Iron: {iron} → {ironEncoded}");
            Debug.Log($"  Cloth: {cloth} → {clothEncoded}");
            Debug.Log($"  Ship: {currentShip} → {shipEncoded}");
            Debug.Log($"  Unlocked: {unlockedShips} → {unlockedEncoded}");
            
            // Build code string (12 digits without checksum)
            string code = string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D1}{5:D1}",
                coinsEncoded, woodEncoded, ironEncoded, clothEncoded, shipEncoded, unlockedEncoded);
            
            
            // Calculate checksum (last digit)
            int checksum = CalculateChecksum(code);
            code += checksum.ToString();
            
            currentSaveCode = code;
            return code;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating save code: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            return DEFAULT_CODE;
        }
    }
    

    // Updates the current save code
    public void UpdateCurrentSaveCode()
    {
        currentSaveCode = GenerateSaveCode();
    }
    

    // Copies current save code to clipboard
    public void CopyCodeToClipboard()
    {
        if (!string.IsNullOrEmpty(currentSaveCode))
        {
            GUIUtility.systemCopyBuffer = currentSaveCode;
        }
    }
    

    // SAVE CODE LOADING
    

    // Loads game state from a save code
    public bool LoadFromCode(string code)
    {
        
        // Validate code
        bool isValid = ValidateSaveCode(code);
        
        if (!isValid)
        {
            Debug.LogWarning($"Invalid save code: {code}");
            return false;
        }
        
        try
        {
            
            // Parse 13-digit code (12 data + 1 checksum)
            int coinsEncoded = int.Parse(code.Substring(0, 4));
            int woodEncoded = int.Parse(code.Substring(4, 2));
            int ironEncoded = int.Parse(code.Substring(6, 2));
            int clothEncoded = int.Parse(code.Substring(8, 2));
            int shipEncoded = int.Parse(code.Substring(10, 1));
            int unlockedEncoded = int.Parse(code.Substring(11, 1));
            
            Debug.Log($"Parsed - Coins:{coinsEncoded} Wood:{woodEncoded} Iron:{ironEncoded} Cloth:{clothEncoded} Ship:{shipEncoded} Unlocked:{unlockedEncoded}");
            
            // Decode values (NO multiplication for coins - direct value)
            float coins = coinsEncoded;
            int wood = woodEncoded;
            int iron = ironEncoded;
            int cloth = clothEncoded;
            int currentShip = shipEncoded;
            
            Debug.Log($"Decoded - Coins:{coins} Wood:{wood} Iron:{iron} Cloth:{cloth}");
            
            // Apply to game managers
            if (GameManager.Instance != null)
            {
                GameManager.Instance.coins = coins;
                GameManager.Instance.wood = wood;
                GameManager.Instance.iron = iron;
                GameManager.Instance.cloth = cloth;
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is NULL - cannot load resources");
            }
            
            if (ShipManager.Instance != null)
            {
                // Unlock ships based on bitmask
                SetUnlockedShipsFromBitmask(unlockedEncoded);
                
                // Set current ship
                ShipManager.Instance.currentShipIndex = currentShip;
            }
            else
            {
                Debug.LogWarning("ShipManager.Instance is NULL - cannot load ships");
            }
            
            // Update current save code
            currentSaveCode = code;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading save code: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            return false;
        }
    }
    

    // Validates a save code format and checksum
    public bool ValidateSaveCode(string code)
    {
        
        // Check if null or empty
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning($"Validation failed: Code is null or empty");
            return false;
        }
        
        
        // Check length
        if (code.Length != CODE_LENGTH)
        {
            Debug.LogWarning($"Validation failed: Invalid code length: {code.Length} (expected {CODE_LENGTH})");
            return false;
        }
        
        // Check all digits
        for (int i = 0; i < code.Length; i++)
        {
            if (!char.IsDigit(code[i]))
            {
                Debug.LogWarning($"Validation failed: Invalid character at position {i}: '{code[i]}' (not a digit)");
                return false;
            }
        }
        
        // Verify checksum (first 12 digits, 13th is checksum)
        string codeWithoutChecksum = code.Substring(0, CODE_LENGTH - 1);
        int expectedChecksum = CalculateChecksum(codeWithoutChecksum);
        int actualChecksum = int.Parse(code.Substring(CODE_LENGTH - 1, 1));
        /*
        Debug.Log($">>> Code without checksum: '{codeWithoutChecksum}'");
        Debug.Log($">>> Expected checksum: {expectedChecksum}");
        Debug.Log($">>> Actual checksum: {actualChecksum}");*/
        
        if (expectedChecksum != actualChecksum)
        {
            Debug.LogWarning($"Validation failed: Checksum mismatch! Expected {expectedChecksum}, got {actualChecksum}");
            return false;
        }
        return true;
    }
    

    // Calculates checksum digit for validation
    private int CalculateChecksum(string code)
    {
        int sum = 0;
        for (int i = 0; i < code.Length; i++)
        {
            int digit = int.Parse(code[i].ToString());
            sum += digit * (i + 1);
        }
        return sum % 10;
    }
    

    // AUTO-SAVE FUNCTIONALITY
    
    public void AutoSave()
    {
        
        // Update save code (if you use codes)
        UpdateCurrentSaveCode();
        
        // ALSO save resources to PlayerPrefs
        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetFloat("SavedCoins", GameManager.Instance.coins);
            PlayerPrefs.SetInt("SavedWood", GameManager.Instance.wood);
            PlayerPrefs.SetInt("SavedIron", GameManager.Instance.iron);
            PlayerPrefs.SetInt("SavedCloth", GameManager.Instance.cloth);
        }
        
        // Save ship data
        if (ShipManager.Instance != null)
        {
            PlayerPrefs.SetInt("SavedShipUnlocks", ShipManager.Instance.GetUnlockedBitmask());
            PlayerPrefs.SetInt("SavedCurrentShip", ShipManager.Instance.currentShipIndex);
        }
        
        // Write to disk
        PlayerPrefs.Save();
    }
    

    // Loads game state from auto-save if it exists
    public bool AutoLoad()
    {
        if (!HasAutoSave())
        {
            return false;
        }
        
        string code = PlayerPrefs.GetString("AutoSaveCode", "");
        string timestamp = PlayerPrefs.GetString("AutoSaveTimestamp", "Unknown");
        
        /*Debug.Log($"Auto-save found:");
        Debug.Log($"  Code: {code}");
        Debug.Log($"  Timestamp: {timestamp}");*/
        
        if (LoadFromCode(code))
        {
            return true;
        }
        else
        {
            Debug.LogWarning("Auto-save code is invalid. Clearing...");
            ClearAutoSave();
            return false;
        }
    }
    

    // Checks if an auto-save exists
    public bool HasAutoSave()
    {
        bool exists = PlayerPrefs.HasKey("AutoSaveCode");
        return exists;
    }
    

    // Clears the auto-save data
    public void ClearAutoSave()
    {
        PlayerPrefs.DeleteKey("AutoSaveCode");
        PlayerPrefs.DeleteKey("AutoSaveTimestamp");
        PlayerPrefs.Save();
    }
    

    // Gets auto-save timestamp for display
    public string GetAutoSaveTimestamp()
    {
        if (PlayerPrefs.HasKey("AutoSaveTimestamp"))
        {
            return PlayerPrefs.GetString("AutoSaveTimestamp", "Unknown");
        }
        return "No auto-save";
    }
    
    // Checks if player has any progress to save
    private bool HasProgress()
    {
        if (GameManager.Instance == null) return false;
        
        // Check if player has more than starting resources
        return GameManager.Instance.coins > 0 ||
               GameManager.Instance.wood > 0 ||
               GameManager.Instance.iron > 0 ||
               GameManager.Instance.cloth > 0;
    }
    

    // Auto-save on application quit
    void OnApplicationQuit()
    {
        if (HasProgress())
        {
            AutoSave();
        }
        else
        {
            Debug.Log("Application quitting - no progress to save");
        }
    }
    

    /// Auto-save on mobile pause
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && HasProgress())
        {
            AutoSave();
        }
    }
    

    // RESET FUNCTIONALITY
    

    // Resets game to default new game state
    public void ResetToNewGame()
    {
        // Reset GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0f;
            GameManager.Instance.wood = 0;
            GameManager.Instance.iron = 0;
            GameManager.Instance.cloth = 0;
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is NULL - cannot reset resources");
        }
        
        // Reset ShipManager
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.ResetShipUnlocks(); // Only first ship unlocked
            ShipManager.Instance.currentShipIndex = 0;
        }
        else
        {
            Debug.LogWarning("ShipManager.Instance is NULL - cannot reset ships");
        }
        
        // Clear auto-save
        ClearAutoSave();
        
        // Update save code
        UpdateCurrentSaveCode();
    }
    

    // SHIP UNLOCK HELPERS

    

    // Gets bitmask representing which ships are unlocked
    private int GetUnlockedShipsBitmask()
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogWarning("ShipManager.Instance is NULL - returning default bitmask (1)");
            return 1; // Only basic ship
        }
        
        return ShipManager.Instance.GetUnlockedBitmask();
    }
    

    // Sets ship unlocked status from bitmask
    private void SetUnlockedShipsFromBitmask(int bitmask)
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogWarning("ShipManager.Instance is NULL - cannot set ship unlocks");
            return;
        }
        
        ShipManager.Instance.SetUnlocksFromBitmask(bitmask);
    }


    // Save all game data to PlayerPrefs
    public void SaveGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null - cannot save!");
            return;
        }
        
        // Save resources
        PlayerPrefs.SetFloat("SavedCoins", GameManager.Instance.coins);
        PlayerPrefs.SetInt("SavedWood", GameManager.Instance.wood);
        PlayerPrefs.SetInt("SavedIron", GameManager.Instance.iron);
        PlayerPrefs.SetInt("SavedCloth", GameManager.Instance.cloth);
        
        /*Debug.Log($"Saved coins: {GameManager.Instance.coins}");
        Debug.Log($"Saved wood: {GameManager.Instance.wood}");
        Debug.Log($"Saved iron: {GameManager.Instance.iron}");
        Debug.Log($"Saved cloth: {GameManager.Instance.cloth}");*/
        
        // Save ship data
        if (ShipManager.Instance != null)
        {
            int shipUnlocks = ShipManager.Instance.GetUnlockedBitmask();
            int currentShip = ShipManager.Instance.currentShipIndex;
            
            PlayerPrefs.SetInt("SavedShipUnlocks", shipUnlocks);
            PlayerPrefs.SetInt("SavedCurrentShip", currentShip);
        }
        
        // Save timestamp
        PlayerPrefs.SetString("SavedTimestamp", System.DateTime.Now.ToString());
        
        // CRITICAL: Write to disk
        PlayerPrefs.Save();
    }

    // Load all game data from PlayerPrefs
    public void LoadGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null - cannot load!");
            return;
        }
        
        // Check if save data exists
        if (!PlayerPrefs.HasKey("SavedCoins"))
        {
            Debug.LogWarning("No saved game found!");
            return;
        }
        
        // Load resources
        GameManager.Instance.coins = PlayerPrefs.GetInt("SavedCoins", 100);
        GameManager.Instance.wood = PlayerPrefs.GetInt("SavedWood", 0);
        GameManager.Instance.iron = PlayerPrefs.GetInt("SavedIron", 0);
        GameManager.Instance.cloth = PlayerPrefs.GetInt("SavedCloth", 0);
        
        /*Debug.Log($"Loaded coins: {GameManager.Instance.coins}");
        Debug.Log($"Loaded wood: {GameManager.Instance.wood}");
        Debug.Log($"Loaded iron: {GameManager.Instance.iron}");
        Debug.Log($"Loaded cloth: {GameManager.Instance.cloth}");*/
        
        // Load ship data
        if (ShipManager.Instance != null)
        {
            if (PlayerPrefs.HasKey("SavedShipUnlocks"))
            {
                int shipUnlocks = PlayerPrefs.GetInt("SavedShipUnlocks");
                ShipManager.Instance.SetUnlocksFromBitmask(shipUnlocks);
            }
            
            if (PlayerPrefs.HasKey("SavedCurrentShip"))
            {
                int currentShip = PlayerPrefs.GetInt("SavedCurrentShip");
                ShipManager.Instance.currentShipIndex = currentShip;
            }
        }
        
        // Show when last saved
        if (PlayerPrefs.HasKey("SavedTimestamp"))
        {
            string timestamp = PlayerPrefs.GetString("SavedTimestamp");
        }
    }

    // Check if save data exists
    public bool HasSaveData()
    {
        bool hasSave = PlayerPrefs.HasKey("SavedCoins");
        return hasSave;
    }


    // Start a new game (reset all progress)
    public void NewGame()
    {
        // Delete all saved data
        PlayerPrefs.DeleteKey("SavedCoins");
        PlayerPrefs.DeleteKey("SavedWood");
        PlayerPrefs.DeleteKey("SavedIron");
        PlayerPrefs.DeleteKey("SavedCloth");
        PlayerPrefs.DeleteKey("SavedShipUnlocks");
        PlayerPrefs.DeleteKey("SavedCurrentShip");
        PlayerPrefs.DeleteKey("SavedTimestamp");
        
        // Also delete shop stock data
        PlayerPrefs.DeleteKey("ShopStock_Wood");
        PlayerPrefs.DeleteKey("ShopStock_Iron");
        PlayerPrefs.DeleteKey("ShopStock_Cloth");
        PlayerPrefs.DeleteKey("ShopTime_Wood");
        PlayerPrefs.DeleteKey("ShopTime_Iron");
        PlayerPrefs.DeleteKey("ShopTime_Cloth");
        
        PlayerPrefs.Save();
        
        // Reset GameManager to defaults
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0f;
            GameManager.Instance.wood = 0;
            GameManager.Instance.iron = 0;
            GameManager.Instance.cloth = 0;
        }
        
        // Reset ShipManager to defaults
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.ResetShipUnlocks(); // Only basic ship unlocked
            ShipManager.Instance.currentShipIndex = 0;
        }
        
        // Load MainGame scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
}