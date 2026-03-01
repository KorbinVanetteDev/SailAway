using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 20f;
    public float acceleration = 2f;
    public float deceleration = 1f;
    public float turnSpeed = 30f;
    
    [Header("Bobbing")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 1f;
    public float bobbingAmount = 0.3f;
    
    [Header("Tilting")]
    public bool enableTilting = true;
    public float tiltAmount = 5f;
    public float tiltSpeed = 2f;
    
    [Header("Wave Motion")]
    public bool enableWaveRocking = true;
    public float rockingSpeed = 0.5f;
    public float rockingAmount = 2f;
    
    [Header("Visuals")]
    public Transform visualsObject;
    
    [Header("Controls")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float currentTurnSpeed = 0f;
    
    private float bobbingTimer = 0f;
    private float rockingTimer = 0f;
    private Vector3 originalPosition;
    private Vector3 visualOriginalPosition;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Setup Rigidbody
        rb.mass = 500f;
        rb.drag = 0.5f;
        rb.angularDrag = 2f;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Only freeze Y position and X/Z rotation
        rb.constraints = RigidbodyConstraints.FreezePositionY | 
                        RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationZ;
        
        // Find or create visuals object
        if (visualsObject == null)
        {
            visualsObject = transform.Find("ShipVisuals");
        }
        
        originalPosition = transform.position;
        
        if (visualsObject != null)
        {
            visualOriginalPosition = visualsObject.localPosition;
        }
    }
    
    void Update()
    {
        HandleInput();
        HandleVisualEffects();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }
    
    void HandleInput()
    {
        // Forward/Backward
        if (Input.GetKey(forwardKey))
        {
            targetSpeed = maxSpeed;
        }
        else if (Input.GetKey(backwardKey))
        {
            targetSpeed = -maxSpeed * 0.5f;
        }
        else
        {
            targetSpeed = 0f;
        }
        
        // Turning
        if (Input.GetKey(leftKey))
        {
            currentTurnSpeed = -turnSpeed;
        }
        else if (Input.GetKey(rightKey))
        {
            currentTurnSpeed = turnSpeed;
        }
        else
        {
            currentTurnSpeed = 0f;
        }
    }
    
    void HandleMovement()
    {
        // Smooth acceleration
        if (Mathf.Abs(targetSpeed) > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }
        
        // Move in forward direction
        Vector3 movement = transform.forward * currentSpeed;
        rb.velocity = new Vector3(movement.x, 0f, movement.z);
    }
    
    void HandleRotation()
    {
        if (Mathf.Abs(currentTurnSpeed) < 0.1f) return;
        
        // Calculate turn (with minimum speed factor for testing)
        float speedFactor = Mathf.Max(Mathf.Abs(currentSpeed) / maxSpeed, 0.5f);
        float turnAmount = currentTurnSpeed * speedFactor * Time.fixedDeltaTime;
        
        // Rotate around Y axis
        transform.Rotate(0f, turnAmount, 0f, Space.World);
    }
    
    void HandleVisualEffects()
    {
        if (visualsObject == null) return;
        
        // Bobbing
        Vector3 targetPos = visualOriginalPosition;
        if (enableBobbing)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float bobOffset = Mathf.Sin(bobbingTimer) * bobbingAmount;
            targetPos.y += bobOffset;
        }
        
        // Tilting and rocking
        float tilt = 0f;
        float rock = 0f;
        
        if (enableTilting && Mathf.Abs(currentSpeed) > 0.1f)
        {
            float speedFactor = Mathf.Abs(currentSpeed) / maxSpeed;
            tilt = -currentTurnSpeed * 0.1f * speedFactor;
        }
        
        if (enableWaveRocking)
        {
            rockingTimer += Time.deltaTime * rockingSpeed;
            rock = Mathf.Sin(rockingTimer) * rockingAmount;
        }
        
        // Apply to visuals only
        visualsObject.localPosition = Vector3.Lerp(visualsObject.localPosition, targetPos, Time.deltaTime * 5f);
        
        Quaternion targetRot = Quaternion.Euler(rock * 0.5f, 0f, tilt + rock);
        visualsObject.localRotation = Quaternion.Slerp(visualsObject.localRotation, targetRot, Time.deltaTime * tiltSpeed);
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}