using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Player Resources")]
    public float coins = 0f;
    public int wood = 0;
    public int iron = 0;
    public int cloth = 0;
    
    [Header("Ship References")]
    public GameObject currentShip;
    public ShipController shipController;
    
    [Header("Debug")]
    private bool hasInitialized = false;
    
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
            return;
        }
    }
    
    void Start()
    {
        // Find ship references (if in game scene)
        if (currentShip == null)
        {
            currentShip = GameObject.Find("PlayerShip");
            /*if (currentShip != null)
            {
                Debug.Log("PlayerShip found");
            }*/
        }
        
        if (currentShip != null)
        {
            shipController = currentShip.GetComponent<ShipController>();
            /*if (shipController != null)
            {
                Debug.Log("ShipController found");
            }*/
        }
        
        // ONLY load saved data on first initialization
        if (!hasInitialized)
        {
            LoadGameData();
            hasInitialized = true;
        }
        else
        {
            Debug.Log("Already initialized - keeping current values");
        }
    }
    

    // Load saved game data from PlayerPrefs
    void LoadGameData()
    {
        if (PlayerPrefs.HasKey("SavedCoins"))
        {
            float oldCoins = coins;
            int oldWood = wood;
            int oldIron = iron;
            int oldCloth = cloth;
            
            coins = PlayerPrefs.GetFloat("SavedCoins", 0f);
            wood = PlayerPrefs.GetInt("SavedWood", 0);
            iron = PlayerPrefs.GetInt("SavedIron", 0);
            cloth = PlayerPrefs.GetInt("SavedCloth", 0);
            
            /*Debug.Log($"✓ Loaded from save:");
            Debug.Log($"  Coins: {oldCoins} → {coins}");
            Debug.Log($"  Wood: {oldWood} → {wood}");
            Debug.Log($"  Iron: {oldIron} → {iron}");
            Debug.Log($"  Cloth: {oldCloth} → {cloth}");**/
        }
        else
        {
            Debug.Log("No saved data found - starting fresh (resources stay at 0)");
        }
    }
    
    public void AddCoins(float amount)
    {
        coins += amount;
    }
    
    public bool SpendCoins(float amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            return true;
        }
        else
        {
            //Debug.Log($"Not enough coins! Need: {amount}, Have: {coins}");
            return false;
        }
    }
    
    public void AddResource(string resourceType, int amount)
    {
        switch (resourceType.ToLower())
        {
            case "wood":
                wood += amount;
                break;
            case "iron":
                iron += amount;
                break;
            case "cloth":
                cloth += amount;
                break;
        }
    }
    
    public bool SpendResources(int woodCost, int ironCost, int clothCost)
    {
        if (wood >= woodCost && iron >= ironCost && cloth >= clothCost)
        {
            wood -= woodCost;
            iron -= ironCost;
            cloth -= clothCost;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    void OnApplicationQuit()
    {        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("SaveSystem.Instance is null - cannot auto-save!");
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
            }
        }
    }
}