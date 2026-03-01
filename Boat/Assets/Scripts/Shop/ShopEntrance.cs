using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopEntrance : MonoBehaviour
{
    [Header("Scene Settings")]
    public string shopSceneName = "ShopScene";
    
    [Header("UI")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Audio")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;
    
    private bool playerInRange = false;
    
    void Start()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        // Hide prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterShop();
        }
    }
    

    // Load shop scene
    void EnterShop()
    {
        // Play sound
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Load shop scene
        SceneManager.LoadScene(shopSceneName);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            
            if (promptText != null)
            {
                promptText.text = "Press E to enter shop";
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}