using UnityEngine;
using TMPro;

public class BoundaryWarning : MonoBehaviour
{
    [Header("UI")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;
    
    [Header("Settings")]
    public float warningDistance = 22f; // Distance from CENTER to show warning
    public float updateInterval = 0.1f; // Check every 0.1 seconds
    
    private Transform player;
    private bool isShowingWarning = false;
    private float updateTimer = 0f;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
        
        if (player == null)
        {
            Debug.LogError("BoundaryWarning: No player found with 'Player' tag!");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Update check timer
        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return;
        updateTimer = 0f;
        
        // Get player position
        Vector3 pos = player.position;
        float distanceFromCenter = new Vector2(pos.x, pos.z).magnitude;
        
        // Check if near boundary
        bool shouldShowWarning = distanceFromCenter > warningDistance;
        
        // Update warning state
        if (shouldShowWarning && !isShowingWarning)
        {
            ShowWarning();
        }
        else if (!shouldShowWarning && isShowingWarning)
        {
            HideWarning();
        }
    }
    
    void ShowWarning()
    {
        isShowingWarning = true;
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
        if (warningText != null)
        {
            warningText.text = "TURN BACK - ISLAND EDGE";
        }
    }
    
    void HideWarning()
    {
        isShowingWarning = false;
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
}