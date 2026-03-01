using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Resource UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI clothText;
    
    void Awake()
    {UpdateResourceDisplay();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        UpdateResourceDisplay();
    }
    
    void UpdateResourceDisplay()
    {
        if (GameManager.Instance != null)
        {
            coinsText.text = "Coins: " + Mathf.Floor(GameManager.Instance.coins).ToString();
            woodText.text = "Wood: " + GameManager.Instance.wood.ToString();
            ironText.text = "Iron: " + GameManager.Instance.iron.ToString();
            clothText.text = "Cloth: " + GameManager.Instance.cloth.ToString();
        }
    }
}