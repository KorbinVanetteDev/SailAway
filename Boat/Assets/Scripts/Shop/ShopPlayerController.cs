using UnityEngine;

public class ShopPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float mouseSensitivity = 2f;
    
    [Header("Camera")]
    public Camera playerCamera;
    
    private CharacterController characterController;
    private float verticalRotation = 0f;
    private float currentSpeed;
    
    void Start()
    {
        // Get or add CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1, 0);
        }
        
        // Get camera
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("No camera found! Add a camera as child.");
            }
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        
        // Unlock cursor with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    

    // Handle WASD movement
    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Determine speed
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Calculate movement direction
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        // Apply movement
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        
        // Apply gravity
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }
    

    // Handle mouse look
    void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate player body (horizontal)
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera (vertical)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}