using UnityEngine;
using TMPro;
using System;

public class ShopCounter : MonoBehaviour
{
    [Header("Counter Settings")]
    public ResourceType resourceType;
    public string resourceName = "Wood";
    public int quantityPerPurchase = 1;
    public int coinCost = 50;
    public int maxStock = 25;
    
    [Header("UI References")]
    public TextMeshProUGUI stockText;
    public TextMeshProUGUI timerText;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Audio")]
    public AudioClip purchaseSound;
    private AudioSource audioSource;
    
    [Header("Visual Feedback")]
    public Renderer counterRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    
    private int currentStock;
    private bool playerInRange = false;
    private DateTime lastPurchaseTime;
    private bool isInitialized = false;
    
    void Start()
    {
        
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Load stock data
        LoadStockData();
        
        // Update UI
        UpdateUI();
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Check if stock should reset
        CheckStockReset();
        
        // Update UI every frame
        UpdateUI();
        
        // Handle interaction
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            AttemptPurchase();
        }
    }
    
    /// <summary>
    /// Load stock data from PlayerPrefs
    /// </summary>
    void LoadStockData()
    {
        string stockKey = $"ShopStock_{resourceType}";
        string timeKey = $"ShopTime_{resourceType}";
        
        // Load current stock
        if (PlayerPrefs.HasKey(stockKey))
        {
            currentStock = PlayerPrefs.GetInt(stockKey);
        }
        else
        {
            currentStock = maxStock;
            SaveStockData();
        }
        
        // Load last purchase time
        if (PlayerPrefs.HasKey(timeKey))
        {
            long timeBinary = long.Parse(PlayerPrefs.GetString(timeKey));
            lastPurchaseTime = DateTime.FromBinary(timeBinary);
        }
        else
        {
            lastPurchaseTime = DateTime.Now;
            SaveStockData();
        }
    }
    

    // Save stock data to PlayerPrefs
    void SaveStockData()
    {
        string stockKey = $"ShopStock_{resourceType}";
        string timeKey = $"ShopTime_{resourceType}";
        
        PlayerPrefs.SetInt(stockKey, currentStock);
        PlayerPrefs.SetString(timeKey, lastPurchaseTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }
    

    // Check if stock should reset (1 hour elapsed)
    void CheckStockReset()
    {
        TimeSpan timeSinceLastPurchase = DateTime.Now - lastPurchaseTime;
        
        // If 1 hour has passed, reset stock
        if (timeSinceLastPurchase.TotalHours >= 1.0 && currentStock < maxStock)
        {
            currentStock = maxStock;
            lastPurchaseTime = DateTime.Now;
            SaveStockData();
        }
    }
    

    // Get time remaining until stock reset
    string GetTimeUntilReset()
    {
        TimeSpan timeSinceLastPurchase = DateTime.Now - lastPurchaseTime;
        TimeSpan timeUntilReset = TimeSpan.FromHours(1) - timeSinceLastPurchase;
        
        if (timeUntilReset.TotalSeconds <= 0 || currentStock >= maxStock)
        {
            return "Stock Full";
        }
        
        return $"{timeUntilReset.Minutes:D2}:{timeUntilReset.Seconds:D2}";
    }
    

    // Update UI elements
    void UpdateUI()
    {
        // Update stock display
        if (stockText != null)
        {
            stockText.text = $"Stock: {currentStock}/{maxStock}";
        }
        
        // Update timer display
        if (timerText != null)
        {
            timerText.text = $"Resets in: \n{GetTimeUntilReset()}";
        }
        
        // Update interaction prompt
        if (playerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            
            if (promptText != null)
            {
                if (currentStock < 1)
                {
                    promptText.text = $"Out of stock!\nResets in: {GetTimeUntilReset()}";
                }
                else if (GameManager.Instance != null && GameManager.Instance.coins < coinCost)
                {
                    promptText.text = $"Not enough coins!\nNeed: {coinCost}\nHave: {GameManager.Instance.coins}";
                }
                else
                {
                    promptText.text = $"Press E to buy {resourceName} (x{quantityPerPurchase})\n{coinCost} Coins\nStock: {currentStock}/{maxStock}";
                }
            }
        }
    }
    

    // Attempt to purchase resources
    void AttemptPurchase()
    { 
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is NULL!");
            return;
        }
        
        // Check if any stock remaining (need at least 1 stock unit)
        if (currentStock < 1)
        {
            Debug.LogWarning($"Out of stock!");
            if (ShopUI.Instance != null)
            {
                ShopUI.Instance.ShowFeedback($"Out of stock! Restock in {GetTimeUntilReset()}");
            }
            return;
        }
        
        // Check if enough coins
        if (GameManager.Instance.coins < coinCost)
        {
            if (ShopUI.Instance != null)
            {
                ShopUI.Instance.ShowFeedback($"Not enough coins! Need {coinCost}, have {GameManager.Instance.coins}");
            }
            return;
        }
        
        // SUCCESSFUL PURCHASE
        Debug.Log($"Purchase approved!");
        
        // Deduct coins
        GameManager.Instance.coins -= coinCost;
        
        // Add resources (player gets quantityPerPurchase)
        switch (resourceType)
        {
            case ResourceType.Wood:
                GameManager.Instance.wood += quantityPerPurchase;
                break;
            case ResourceType.Iron:
                GameManager.Instance.iron += quantityPerPurchase;
                break;
            case ResourceType.Cloth:
                GameManager.Instance.cloth += quantityPerPurchase;
                break;
        }
        
        // CRITICAL: Decrease stock by 1 (tracking number of purchases, not units)
        int stockBefore = currentStock;
        currentStock -= 1; // ✅ DECREASE BY 1
        int stockAfter = currentStock;
        
        /*Debug.Log($"AFTER PURCHASE:");
        Debug.Log($"  Stock before: {stockBefore}");
        Debug.Log($"  Purchases made: 1");
        Debug.Log($"  Stock after: {stockAfter}");
        Debug.Log($"  Resources given: {quantityPerPurchase}");*/
        
        lastPurchaseTime = DateTime.Now;
        SaveStockData();
        
        if (purchaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(purchaseSound);
        }
        
        if (ShopUI.Instance != null)
        {
            ShopUI.Instance.ShowFeedback($"Purchased {quantityPerPurchase} {resourceName}!");
        }
        
        UpdateUI();
    }
    

    // Player entered trigger zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Highlight counter
            if (counterRenderer != null)
            {
                counterRenderer.material.color = highlightColor;
            }
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }
    

    // Player left trigger zone
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Remove highlight
            if (counterRenderer != null)
            {
                counterRenderer.material.color = normalColor;
            }
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}

public enum ResourceType
{
    Wood,
    Iron,
    Cloth
}