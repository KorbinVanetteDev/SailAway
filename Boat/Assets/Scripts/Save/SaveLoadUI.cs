using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Load Save Dialog")]
    public GameObject loadSaveDialog;
    public TMP_InputField saveCodeInput;
    public TextMeshProUGUI errorText;
    public Button loadButton;
    public Button cancelLoadButton;
    
    [Header("New Game Warning Dialog")]
    public GameObject newGameWarningDialog;
    public TextMeshProUGUI saveCodeDisplay;
    public Button copyCodeButton;
    public Button proceedButton;
    public Button cancelWarningButton;
    
    [Header("Settings")]
    public string gameSceneName = "MainGame";
    
    void Start()
    {
        // Hide dialogs initially
        if (loadSaveDialog != null) loadSaveDialog.SetActive(false);
        if (newGameWarningDialog != null) newGameWarningDialog.SetActive(false);
        if (errorText != null) errorText.gameObject.SetActive(false);
        
        // Connect button events
        if (loadButton != null) loadButton.onClick.AddListener(OnLoadButtonClicked);
        if (cancelLoadButton != null) cancelLoadButton.onClick.AddListener(CloseLoadDialog);
        
        if (copyCodeButton != null) copyCodeButton.onClick.AddListener(OnCopyCodeClicked);
        if (proceedButton != null) proceedButton.onClick.AddListener(OnProceedNewGame);
        if (cancelWarningButton != null) cancelWarningButton.onClick.AddListener(CloseWarningDialog);
    }
    
    // LOAD SAVE DIALOG
    
    public void OpenLoadDialog()
    {
        if (loadSaveDialog != null)
        {
            loadSaveDialog.SetActive(true);
            
            // Clear previous input
            if (saveCodeInput != null)
            {
                saveCodeInput.text = "";
                saveCodeInput.ActivateInputField(); // Auto-focus
            }
            
            // Hide error
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
            
        }
    }
    
    public void CloseLoadDialog()
    {
        if (loadSaveDialog != null)
        {
            loadSaveDialog.SetActive(false);
        }
    }
    
    void OnLoadButtonClicked()
    {
        string code = saveCodeInput != null ? saveCodeInput.text : "";
        
        // Validate and load
        if (SaveSystem.Instance != null && SaveSystem.Instance.LoadFromCode(code))
        {
            // Success!
            
            // Close dialog
            CloseLoadDialog();
            
            // Load game scene
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Failed - show error
            ShowError("Invalid save code. Please check and try again.");
        }
    }
    
    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }
    
    // NEW GAME WARNING DIALOG
    
    public void OpenNewGameWarning()
{
    if (newGameWarningDialog != null)
    {
        // Check if auto-save exists
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasAutoSave())
        {
            // Show auto-save code in warning
            string autoSaveCode = PlayerPrefs.GetString("AutoSaveCode", "");
            
            if (saveCodeDisplay != null)
            {
                saveCodeDisplay.text = autoSaveCode;
            }
        }
        else
        {
            // No auto-save, generate current code
            if (SaveSystem.Instance != null && saveCodeDisplay != null)
            {
                SaveSystem.Instance.UpdateCurrentSaveCode();
                saveCodeDisplay.text = SaveSystem.Instance.currentSaveCode;
            }
        }
        
        newGameWarningDialog.SetActive(true);
    }
}
    
    public void CloseWarningDialog()
    {
        if (newGameWarningDialog != null)
        {
            newGameWarningDialog.SetActive(false);
        }
    }
    
    void OnCopyCodeClicked()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.CopyCodeToClipboard();
            
            if (copyCodeButton != null)
            {
                TextMeshProUGUI buttonText = copyCodeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string originalText = buttonText.text;
                    buttonText.text = "COPIED!";
                    
                    // Reset after 1 second
                    Invoke(nameof(ResetCopyButtonText), 1f);
                }
            }
        }
    }
    
    void ResetCopyButtonText()
    {
        if (copyCodeButton != null)
        {
            TextMeshProUGUI buttonText = copyCodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "COPY CODE";
            }
        }
    }
    
    void OnProceedNewGame()
    {
        
        // Reset game state
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.ResetToNewGame();
        }
        
        // Close warning
        CloseWarningDialog();
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }
}