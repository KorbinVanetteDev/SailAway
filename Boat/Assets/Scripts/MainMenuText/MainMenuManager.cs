using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "MainGame";
    
    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;
    public GameObject newGameWarningDialog;
    public GameObject loadGamePanel;
    
    [Header("Buttons")]
    public GameObject playButton;
    public GameObject newGameButton;
    public GameObject controlsButton;
    public GameObject loadGameButton;
    public GameObject quitButton;
    
    [Header("Optional: Dynamic Text")]
    public TextMeshProUGUI playButtonText;
    public TextMeshProUGUI warningDialogText;
    
    void Start()
    {
        // Show cursor in menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Make sure main menu is visible
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Hide other panels
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
        
        if (newGameWarningDialog != null)
        {
            newGameWarningDialog.SetActive(false);
        }
        
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }
        
        // Update Play button text based on save data
        UpdatePlayButton();
    }


    // Update Play button text: "Continue" if save exists, "Play" if new game
    void UpdatePlayButton()
    {
        bool hasSaveData = HasSaveData();
        
        if (playButtonText != null)
        {
            if (hasSaveData)
            {
                playButtonText.text = "CONTINUE";
            }
            else
            {
                playButtonText.text = "PLAY";
            }
        }
    }
    

    // Check if save data exists
    bool HasSaveData()
    {
        // Check multiple save keys to be sure
        bool hasCoins = PlayerPrefs.HasKey("SavedCoins");
        bool hasTimestamp = PlayerPrefs.HasKey("SavedTimestamp");
        
        return hasCoins || hasTimestamp;
    }
    

    // PLAY BUTTON
    // Called when Play button is clicked
    // If save exists: Continue game
    // If no save: Start new game
    public void OnPlayButtonClicked()
    {
        bool hasSaveData = HasSaveData();
        
        if (hasSaveData)
        {
            ContinueGame();
        }
        else
        {
            StartNewGame();
        }
    }


    // Continue from saved game
    void ContinueGame()
    {
        
        // Load saved data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
        }
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }
    
    // NEW GAME BUTTON
    // Called when New Game button is clicked
    // Shows warning if save exists, otherwise starts new game
    public void OnNewGameButtonClicked()
    {
        
        bool hasSaveData = HasSaveData();
        
        if (hasSaveData)
        {
            // Save exists - show warning dialog
            ShowNewGameWarning();
        }
        else
        {
            // No save - start new game directly
            StartNewGame();
        }
    }
    

    // Show new game warning dialog
    void ShowNewGameWarning()
    {
        if (newGameWarningDialog != null)
        {
            // Update warning text with current save info
            if (warningDialogText != null)
            {
                // Get save data from PlayerPrefs
                float coins = PlayerPrefs.GetFloat("SavedCoins", 0f);
                int wood = PlayerPrefs.GetInt("SavedWood", 0);
                int iron = PlayerPrefs.GetInt("SavedIron", 0);
                int cloth = PlayerPrefs.GetInt("SavedCloth", 0);
                string timestamp = PlayerPrefs.GetString("SavedTimestamp", "Unknown");
                
                // Generate save code
                string saveCode = "";
                if (SaveSystem.Instance != null)
                {
                    saveCode = SaveSystem.Instance.GenerateSaveCode();
                }
                
                // Build warning message
                string warningMessage =
                                    $"This will DELETE your current save!\n" +
                                    $"Current Progress:\n" +
                                    $"{coins} Coins\n" +
                                    $"{wood} Wood\n" +
                                    $"{iron} Iron\n" +
                                    $"{cloth} Cloth\n";
                
                // Add save code if generated
                if (!string.IsNullOrEmpty(saveCode))
                {
                    warningMessage += $"Press Play, then ESC, then Save to get your save code.\n";
                }
                
                warningMessage += $"Last Saved: {timestamp}\n" +
                                $"Are you sure?";
                
                warningDialogText.text = warningMessage;
            }
            else
            {
                Debug.LogWarning("warningDialogText is not assigned in Inspector!");
            }
            
            // Show the dialog
            newGameWarningDialog.SetActive(true);
        }
        else
        {
            Debug.LogError("newGameWarningDialog is not assigned in Inspector!");
            // Start new game anyway since we can't show warning
            StartNewGame();
        }
    }


    // Called by "Proceed" or "Yes" button in warning dialog
    public void ConfirmNewGame()
    {
        // Hide warning dialog
        if (newGameWarningDialog != null)
        {
            newGameWarningDialog.SetActive(false);
        }
        
        // Clear save and start new game
        ClearSaveData();
        StartNewGame();
    }


    // Called by "Cancel" or "No" button in warning dialog
    public void CancelNewGame()
    {
        // Hide warning dialog
        if (newGameWarningDialog != null)
        {
            newGameWarningDialog.SetActive(false);
        }
    }



    // Start a new game (reset everything and load game scene)
    void StartNewGame()
    {
        
        // Clear all save data
        ClearSaveData();
        
        // Reset GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0f;
            GameManager.Instance.wood = 0;
            GameManager.Instance.iron = 0;
            GameManager.Instance.cloth = 0;
        }
        
        // Reset ShipManager
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.ResetShipUnlocks();
            ShipManager.Instance.currentShipIndex = 0;
        }
        
        // Reset SaveSystem
        if (SaveSystem.Instance != null)
        {
            // Mark that we have a fresh start
            Debug.Log("SaveSystem reset");
        }
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }


    // Clear all save data from PlayerPrefs
    void ClearSaveData()
    {
        
        // Delete all PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    
    // CONTROLS BUTTON

    // Show controls screen
    public void ShowControls()
    {
        
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("controlsPanel is not assigned in Inspector!");
        }
        
        // Hide main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }
    
    // Hide controls screen (called by Back button in controls)
    public void HideControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
        
        // Show main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    

    // LOAD GAME BUTTON
    
    // Show load game screen
    public void ShowLoadGame()
    {
        
        bool hasSaveData = HasSaveData();
        
        if (!hasSaveData)
        {
            Debug.LogWarning("No save data to load!");
            // Optionally show a message to the user
            return;
        }
        
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("loadGamePanel is not assigned in Inspector!");
            // If no load panel, just load the game directly
            LoadGameDirectly();
        }
        
        // Hide main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }


    // Hide load game screen (called by Back button)
    public void HideLoadGame()
    {
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }
        
        // Show main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    

    // Load game directly (called by Load button in load game panel)
    public void LoadGameDirectly()
    {
        
        bool hasSaveData = HasSaveData();
        
        if (!hasSaveData)
        {
            Debug.LogError("No save data to load!");
            return;
        }
        
        // Load saved data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
        }
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }
    

    // QUIT BUTTON
    
    // Quit the game
    public void QuitGame()
    {     
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    

    // DEBUG METHODS
    
    void Update()
    {
        // Press P to print save data status
        if (Input.GetKeyDown(KeyCode.P))
        {
            DebugPrintSaveData();
        }
        
        // Press C to clear save data (testing)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearSaveData();
            UpdatePlayButton();
        }
    }
    
    void DebugPrintSaveData()
    {
        Debug.Log("--SAVE DATA DEBUG--");
        Debug.Log($"Has save data: {HasSaveData()}");
        Debug.Log($"SavedCoins: {PlayerPrefs.GetFloat("SavedCoins", -1)}");
        Debug.Log($"SavedWood: {PlayerPrefs.GetInt("SavedWood", -1)}");
        Debug.Log($"SavedIron: {PlayerPrefs.GetInt("SavedIron", -1)}");
        Debug.Log($"SavedCloth: {PlayerPrefs.GetInt("SavedCloth", -1)}");
        Debug.Log($"SavedTimestamp: {PlayerPrefs.GetString("SavedTimestamp", "None")}");
        Debug.Log("---");
    }


    // Copy save code to clipboard (called by Copy Code button)
    public void CopySaveCode()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("SaveSystem.Instance is null!");
            return;
        }
        
        // Check if save data exists
        if (!HasSaveData())
        {
            Debug.LogWarning("No save data to copy!");
            return;
        }
        
        // Generate save code
        string saveCode = SaveSystem.Instance.GenerateSaveCode();
        
        if (string.IsNullOrEmpty(saveCode))
        {
            Debug.LogError("Failed to generate save code!");
            return;
        }
        
        // Copy to clipboard
        GUIUtility.systemCopyBuffer = saveCode;
        
        ShowCopyFeedback();
    }


    // Show visual feedback that code was copied
    void ShowCopyFeedback()
    {

        Debug.Log("Code copied to clipboard!");
    }

}