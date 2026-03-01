using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button saveProgressButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Save Code Dialog")]
    public GameObject saveCodeDialog;
    public TextMeshProUGUI codeDisplayText;
    public Button copyCodeButton;
    public Button closeSaveDialogButton;
    
    [Header("Confirmation Dialog")]
    public GameObject confirmationDialog;
    public TextMeshProUGUI confirmationText;
    public Button confirmYesButton;
    public Button confirmNoButton;
    
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;
    private System.Action confirmAction;
    
    void Awake()
    {
        //Debug.Log("=== PauseMenuManager Awake ===");
        //Debug.Log($"pauseMenuPanel assigned: {pauseMenuPanel != null}");
    }
    
    void Start()
    {
        // Hide all panels initially
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("pauseMenuPanel is NULL! Assign it in Inspector!");
        }
        
        if (saveCodeDialog != null) saveCodeDialog.SetActive(false);
        if (confirmationDialog != null) confirmationDialog.SetActive(false);
        
        // Connect button events
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        else
        {
            Debug.LogError("resumeButton is NULL!");
        }
        
        if (saveProgressButton != null)
        {
            saveProgressButton.onClick.RemoveAllListeners();
            saveProgressButton.onClick.AddListener(ShowSaveCode);
        }
        else
        {
            Debug.LogError("saveProgressButton is NULL!");
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        else
        {
            Debug.LogError("mainMenuButton is NULL!");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        else
        {
            Debug.LogError("quitButton is NULL!");
        }
        
        if (copyCodeButton != null)
        {
            copyCodeButton.onClick.RemoveAllListeners();
            copyCodeButton.onClick.AddListener(CopyCodeToClipboard);
        }
        
        if (closeSaveDialogButton != null)
        {
            closeSaveDialogButton.onClick.RemoveAllListeners();
            closeSaveDialogButton.onClick.AddListener(CloseSaveCodeDialog);
        }
        
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        }
        else
        {
            Debug.LogError("confirmYesButton is NULL!");
        }
        
        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        }
        else
        {
            Debug.LogError("confirmNoButton is NULL!");
        }
        
        // Ensure game starts unpaused
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Check for ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // If in save code dialog, close it first
                if (saveCodeDialog != null && saveCodeDialog.activeSelf)
                {
                    CloseSaveCodeDialog();
                    return;
                }
                
                // If in confirmation dialog, close it
                if (confirmationDialog != null && confirmationDialog.activeSelf)
                {
                    CloseConfirmation();
                    return;
                }
                
                // Otherwise resume game
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        
        // Show pause menu panel
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Cannot show pause menu - pauseMenuPanel is NULL!");
            return;
        }
        
        // Pause game
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Update save code
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.UpdateCurrentSaveCode();
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        
        // Hide all panels
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveCodeDialog != null) saveCodeDialog.SetActive(false);
        if (confirmationDialog != null) confirmationDialog.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void ShowSaveCode()
    {
        if (saveCodeDialog != null && SaveSystem.Instance != null)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            
            SaveSystem.Instance.UpdateCurrentSaveCode();
            if (codeDisplayText != null)
            {
                codeDisplayText.text = SaveSystem.Instance.currentSaveCode;
            }
            
            saveCodeDialog.SetActive(true);
        }
        else
        {
            Debug.LogError("Cannot show save code - dialog or SaveSystem is NULL!");
        }
    }
    
    void CloseSaveCodeDialog()
    {
        if (saveCodeDialog != null) saveCodeDialog.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }
    
    void CopyCodeToClipboard()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.CopyCodeToClipboard();
            
            if (copyCodeButton != null)
            {
                TextMeshProUGUI buttonText = copyCodeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string original = buttonText.text;
                    buttonText.text = "COPIED!";
                    StartCoroutine(ResetButtonTextAfterDelay(buttonText, original, 1.5f));
                }
            }
        }
    }
    
    IEnumerator ResetButtonTextAfterDelay(TextMeshProUGUI textComponent, string originalText, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (textComponent != null)
        {
            textComponent.text = originalText;
        }
    }
    
    void OnMainMenuClicked()
    {
        ShowConfirmation(
            "Save your progress before returning to Main Menu?",
            ReturnToMainMenuWithSave,
            ReturnToMainMenuWithoutSave
        );
    }
    
    void ReturnToMainMenuWithSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.AutoSave();
        }
        else
        {
            Debug.LogWarning("SaveSystem.Instance is NULL - cannot auto-save");
        }
        StartCoroutine(LoadMainMenuAsync(true));
    }
    
    void ReturnToMainMenuWithoutSave()
    {
        StartCoroutine(LoadMainMenuAsync(false));
    }
    
    IEnumerator LoadMainMenuAsync(bool saveFirst)
    {
        if (saveFirst)
        {
            Debug.Log("Auto-save already completed in ReturnToMainMenuWithSave");
        }

        Time.timeScale = 1f;
        
        // Wait one frame to ensure time scale is applied
        yield return null;
        
        // List all scenes in build
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        
        AsyncOperation asyncLoad = null;
        
        try
        {
            asyncLoad = SceneManager.LoadSceneAsync(mainMenuSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EXCEPTION when calling LoadSceneAsync: {e.Message}");
            Debug.LogError($"Stack Trace: {e.StackTrace}");
            yield break;
        }
        
        if (asyncLoad == null)
        {
            Debug.LogError("AsyncOperation is NULL! Scene load failed completely!");
            Debug.LogError($"Check that scene '{mainMenuSceneName}' exists in Build Settings");
            yield break;
        }
        
        int frameCount = 0;
        while (!asyncLoad.isDone)
        {
            frameCount++;
            yield return null;
        }
    }
    
    void OnQuitClicked()
    {
        ShowConfirmation(
            "Save your progress before quitting?",
            QuitWithSave,
            QuitWithoutSave
        );
    }
    
    void QuitWithSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.AutoSave();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void QuitWithoutSave()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void ShowConfirmation(string message, System.Action onYes, System.Action onNo)
    { 
        if (confirmationDialog != null)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            

            if (confirmationText != null)
            {
                confirmationText.text = message;
            }
            else
            {
                Debug.LogError("confirmationText is NULL!");
            }
            
            confirmAction = onYes;
            
            confirmationDialog.SetActive(true);
        }
        else
        {
            Debug.LogError("confirmationDialog is NULL! Cannot show confirmation.");
        }
    }
    
    void OnConfirmYes()
    {
        // STORE the action BEFORE closing (which clears it)
        System.Action actionToInvoke = confirmAction;
        
        // Close confirmation dialog
        CloseConfirmation();
        
        // NOW invoke the stored action
        if (actionToInvoke != null)
        {
            actionToInvoke.Invoke();
        }
        else
        {
            Debug.LogError("confirmAction is NULL! Nothing will happen.");
        }
    }
    
    void OnConfirmNo()
    {
        CloseConfirmation();
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }
    
    void CloseConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        
        confirmAction = null;
    }
}