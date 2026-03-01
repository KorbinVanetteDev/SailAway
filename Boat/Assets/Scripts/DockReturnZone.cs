using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DockReturnZone : MonoBehaviour
{
    [Header("UI References")]
    public GameObject returnPrompt;
    public TextMeshProUGUI promptText;
    
    private bool playerInRange = false;
    
    void Start()
    {
        if (returnPrompt != null)
        {
            returnPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKey(KeyCode.E))
        {
            ReturnToSea();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (returnPrompt != null && promptText != null)
            {
                promptText.text = "Press and Hold E to Return to Ship";
                returnPrompt.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (returnPrompt != null)
            {
                returnPrompt.SetActive(false);
            }
        }
    }
    
    void ReturnToSea()
    {
        // Load main ocean scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
}