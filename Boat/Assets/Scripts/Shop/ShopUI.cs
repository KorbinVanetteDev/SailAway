using UnityEngine;
using TMPro;
using System.Collections;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;
    
    [Header("Resource HUD")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI clothText;
    
    [Header("Feedback Message")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 2f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Hide feedback panel
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        UpdateResourceDisplay();
    }
    

    // Update resource HUD
    void UpdateResourceDisplay()
    {
        if (GameManager.Instance == null) return;
        
        if (coinsText != null)
        {
            coinsText.text = $"{GameManager.Instance.coins}";
        }
        
        if (woodText != null)
        {
            woodText.text = $"{GameManager.Instance.wood}";
        }
        
        if (ironText != null)
        {
            ironText.text = $"{GameManager.Instance.iron}";
        }
        
        if (clothText != null)
        {
            clothText.text = $"{GameManager.Instance.cloth}";
        }
    }
    

    // Show feedback message
    public void ShowFeedback(string message)
    {
        if (feedbackPanel == null || feedbackText == null) return;
        
        feedbackText.text = message;
        feedbackPanel.SetActive(true);
        
        StopAllCoroutines();
        StartCoroutine(HideFeedbackAfterDelay());
    }
    
    IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
}