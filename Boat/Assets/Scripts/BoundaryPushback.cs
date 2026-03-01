using UnityEngine;

public class BoundaryPushback : MonoBehaviour
{
    [Header("Settings")]
    public float boundaryX = 23f;
    public float boundaryZ = 23f;
    public float pushForce = 15f;
    public float pushbackStartDistance = 2f; // Only push in last 2 units before edge
    
    private Rigidbody playerRb;
    private Transform player;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("BoundaryPushback: No player found!");
        }
    }
    
    void FixedUpdate()
    {
        if (player == null || playerRb == null) return;
        
        Vector3 pos = player.position;
        Vector3 pushDirection = Vector3.zero;
        bool needsPushback = false;
        
        // Calculate distance from boundary edges
        float distanceFromEdgeX = Mathf.Abs(pos.x) - (boundaryX - pushbackStartDistance);
        float distanceFromEdgeZ = Mathf.Abs(pos.z) - (boundaryZ - pushbackStartDistance);
        
        // Only push if VERY close to edge (in the pushback zone)
        if (distanceFromEdgeX > 0)
        {
            // Close to X boundary
            if (pos.x > 0)
            {
                pushDirection.x = -1; // Push left (west)
            }
            else
            {
                pushDirection.x = 1; // Push right (east)
            }
            needsPushback = true;
        }
        
        if (distanceFromEdgeZ > 0)
        {
            // Close to Z boundary
            if (pos.z > 0)
            {
                pushDirection.z = -1; // Push back (south)
            }
            else
            {
                pushDirection.z = 1; // Push forward (north)
            }
            needsPushback = true;
        }
        
        // Apply pushback ONLY if needed
        if (needsPushback)
        {
            pushDirection.Normalize();
            playerRb.AddForce(pushDirection * pushForce, ForceMode.Force);
            
            // Log only once per second to avoid spam (NOT USED)
            if (Time.frameCount % 50 == 0)
            {
                //Debug.Log("Pushing player from boundary edge");
            }
        }
    }
}