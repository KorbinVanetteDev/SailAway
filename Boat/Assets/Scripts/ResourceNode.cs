using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Settings")]
    public string resourceType = "wood";
    public int resourceAmount = 5;
    public float respawnTime = 30f;
    
    [Header("Visual")]
    public GameObject visualModel;
    
    private bool isAvailable = true;
    private float respawnTimer = 0f;
    
    void Update()
    {
        if (!isAvailable)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
        }
    }
    
    public void Collect()
    {
        if (!isAvailable) return;
        
        GameManager.Instance.AddResource(resourceType, resourceAmount);
        
        isAvailable = false;
        respawnTimer = 0f;
        
        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
    
    void Respawn()
    {
        isAvailable = true;
        
        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isAvailable)
        {
            Collect();
        }
    }
}