using UnityEngine;

public class ShipCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    public bool stopOnCollision = true;
    public string obstacleTag = "Barrier"; // Tag for barriers
    public float stopDuration = 0.5f; // How long to stop after collision
    
    [Header("Feedback")]
    public bool showCollisionDebug = true;
    public AudioClip collisionSound;
    
    [Header("References (Auto-assigned)")]
    private ShipController shipController;
    private Rigidbody shipRigidbody;
    private AudioSource audioSource;
    
    private bool isStopped = false;
    private float stopTimer = 0f;
    
    void Start()
    {
        // Get ship controller
        shipController = GetComponent<ShipController>();
        if (shipController == null)
        {
            Debug.LogWarning("ShipController not found on " + gameObject.name);
        }
        
        // Get rigidbody
        shipRigidbody = GetComponent<Rigidbody>();
        if (shipRigidbody == null)
        {
            Debug.LogWarning("Rigidbody not found on " + gameObject.name);
        }
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        // Handle stop timer
        if (isStopped)
        {
            stopTimer -= Time.deltaTime;
            
            if (stopTimer <= 0f)
            {
                isStopped = false;
                if (showCollisionDebug)
                {
                    Debug.Log("Ship can move again");
                }
            }
        }
    }
    

    // Called when ship collides with something
    void OnCollisionEnter(Collision collision)
    {
        // Check if it's an obstacle
        if (collision.gameObject.CompareTag(obstacleTag))
        {
            HandleObstacleCollision(collision);
        }
    }
    

    // Called while ship is touching something
    void OnCollisionStay(Collision collision)
    {
        // Keep ship stopped while touching obstacle
        if (collision.gameObject.CompareTag(obstacleTag) && stopOnCollision)
        {
            StopShipMovement();
        }
    }
    

    // Handle collision with obstacle
    void HandleObstacleCollision(Collision collision)
    {
        if (showCollisionDebug)
        {
            Debug.Log($"--- SHIP COLLISION ---");
            Debug.Log($"Hit: {collision.gameObject.name}");
            Debug.Log($"Point: {collision.contacts[0].point}");
            Debug.Log($"Normal: {collision.contacts[0].normal}");
            Debug.Log("-----------------------");
        }
        
        if (stopOnCollision)
        {
            StopShipMovement();
            isStopped = true;
            stopTimer = stopDuration;
        }
        
        // Play collision sound
        if (collisionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }
    

    // Stop all ship movement
    void StopShipMovement()
    {
        if (shipRigidbody != null)
        {
            // Stop velocity
            shipRigidbody.velocity = Vector3.zero;
            shipRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Optionally disable ship controller temporarily
        if (shipController != null && isStopped)
        {
            // You can add a public method to ShipController to pause movement
            // shipController.SetMovementEnabled(false);
        }
    }
    

    // Check if ship can move (for use by ShipController)
    public bool CanMove()
    {
        return !isStopped;
    }
}