using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopExitDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    public string islandSceneName = "Island";
    
    [Header("UI")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Audio")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;
    
    private bool playerInRange = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ExitShop();
        }
    }
    
    void ExitShop()
    {
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        SceneManager.LoadScene(islandSceneName);
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
                promptText.text = "Press E to exit shop";
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