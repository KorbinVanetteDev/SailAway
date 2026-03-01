using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IslandLandingZone : MonoBehaviour
{
    [Header("Island Settings")]
    public string islandName = "Forest Island";
    public string islandSceneName = "Island_1_Scene";
    
    [Header("UI References")]
    public GameObject landingPrompt;
    public TextMeshProUGUI promptText;
    
    private bool playerInRange = false;
    private GameObject player;
    
    void Start()
    {
        if (landingPrompt != null)
        {
            landingPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKey(KeyCode.E))
        {
            LandOnIsland();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
            
            if (landingPrompt != null && promptText != null)
            {
                promptText.text = "Press and Hold E to Land on " + islandName;
                landingPrompt.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            
            if (landingPrompt != null)
            {
                landingPrompt.SetActive(false);
            }
        }
    }
    
    void LandOnIsland()
    {
        // Save current ship position
        if (player != null)
        {
            PlayerPrefs.SetFloat("ShipPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("ShipPosY", player.transform.position.y);
            PlayerPrefs.SetFloat("ShipPosZ", player.transform.position.z);
            PlayerPrefs.SetString("CurrentIsland", islandName);
            PlayerPrefs.Save();
        }
        
        // Load island scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(islandSceneName);
    }
}