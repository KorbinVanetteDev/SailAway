//using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class PLayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float runSpeed = 12f;
    public float mouseSensitivity = 2f;

    [Header("Physics")]
    public float slopeForceDown = 0f;
    public float airControlMultiplier = 0.8f;

    [Header("Custom Gravity ;)")]
    public float gravity = -25f;
    public float maxFallSpeed = -20f;

    [Header("Jump")]
    public bool enableJump = true;
    public float jumpForce =12f;
    public float jumpHoldForce = 3f;
    public float maxJumpTime = 0.3f;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Jump Smoothing")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    //This was the death of me.
    [Header("Wall Interaction")]
    public bool enableWallClimbing = true;
    public float wallClimbSpeed = 3f;
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 10f;
    public float wallJumpAwayForce = 5f;
    public float wallCheckDistance = 0.8f;

    [Header("Landing")]
    public bool enableLandingSmoothing = true;

    [Header("Crouching")]
    public bool enableCrouch = true;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Step Climbing")]
    public bool enableStepClimbing = true;
    public float stepHeight = 0.8f;
    public float stepSmooth = 0.15f;
    public float climbCheckDistance = 0.8f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private bool wasGrounded = false;
    private bool isCrouching = false;
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;
    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;
    private bool isTouchingWall = false;
    private Vector3 wallNormal;
    private float verticalVelocity = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(rb==null) return;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 0f;
        rb.angularDrag = 0f;

        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        cameraTransform = GetComponentInChildren<Camera>()?.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(cameraTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(0, mouseX, 0);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.5f);

        CheckForWalls();

        if(isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            if(!isJumping && verticalVelocity <0)
            {
                verticalVelocity = 0f;
            }
        } else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Buffer
        if(Input.GetKeyDown(jumpKey))
        {
            jumpBufferCounter = jumpBufferTime;
        } else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        //Crouch
        if(enableCrouch)
        {
            HandlingCrouching();
        }

        // Jumping
        if(enableJump)
        {
            if(jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
            {
                Jump();
                jumpBufferCounter = 0f;
            }

            if(Input.GetKeyDown(jumpKey) && isTouchingWall && !isGrounded)
            {
                WallJump();
                jumpBufferCounter = 0f;
            }

            if(Input.GetKey(jumpKey) && isJumping)
            {
                if(jumpTimeCounter < maxJumpTime)
                {
                    verticalVelocity += jumpHoldForce * Time.deltaTime;
                    jumpTimeCounter += Time.deltaTime;
                }
            }

            if(Input.GetKeyUp(jumpKey))
            {
                isJumping = false;
                jumpTimeCounter = 0f;

                if(verticalVelocity > 0)
                {
                    verticalVelocity *= 0.5f;
                }
            }

            if(verticalVelocity <0)
            {
                isJumping = false;
            }
        }

        if(enableLandingSmoothing)
        {
            if(!wasGrounded && isGrounded)
            {
                OnLand();
            }
        }

        wasGrounded = isGrounded;

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        if(rb == null) return;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float deadZone = 0.15f;
        if(Mathf.Abs(horizontal) < deadZone) horizontal = 0f;
        if(Mathf.Abs(vertical) < deadZone) vertical = 0f;

        bool hasInput = Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f;

        Vector3 horizontalVelocity = Vector3.zero;

        if(hasInput) {
            Vector3 movement = (transform.forward * vertical + transform.right * horizontal).normalized;

            float currentSpeed = moveSpeed;

            if(isCrouching)
            {
                currentSpeed = moveSpeed * 0.5f;
            } else if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
            }

            if(!isGrounded)
            {
                currentSpeed *= airControlMultiplier;
            }

            horizontalVelocity = movement * currentSpeed;

            if(enableStepClimbing && isGrounded && !isCrouching)
            {
                HandleSteps();
            }
        } else if (isGrounded)
        {
            horizontalVelocity = Vector3.zero;
        } else
        {
            Vector3 currentVel = rb.velocity;
            horizontalVelocity = new Vector3(currentVel.x,0,currentVel.z);
        }
        
        if(!isGrounded)
        {
            if(isTouchingWall && enableWallClimbing)
            {
                if(verticalVelocity > 0)
                {
                    verticalVelocity += gravity * 0.3f * Time.fixedDeltaTime;
                } else
                {
                    verticalVelocity += gravity * 0.5f * Time.fixedDeltaTime;

                    if(verticalVelocity < -wallSlideSpeed)
                    {
                        verticalVelocity = -wallSlideSpeed;
                    }
                }
            } else
            {
                verticalVelocity += gravity * Time.fixedDeltaTime;

                if(verticalVelocity < maxFallSpeed)
                {
                    verticalVelocity = maxFallSpeed;
                }
            }
        } else
        {
            if(slopeForceDown > 0 && hasInput)
            {
                verticalVelocity = -slopeForceDown;
            } else if (!isJumping)
            {
                verticalVelocity = 0f;
            }
        }

        Vector3 finalVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

        rb.velocity = finalVelocity;

        if(Time.frameCount %30 == 0)
        {
            //Debug.Log($"Velocity: {rb.velocity}, Grounded: {isGrounded}, Touching Wall: {isTouchingWall}");
        }
    }

    void LateUpdate()
    {
        if(rb == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if(Mathf.Abs(horizontal) < 0.1f && Mathf.Abs(vertical) < 0.1f && isGrounded)
        {
            Vector3 vel = rb.velocity;

            if(Mathf.Abs(vel.x) > 0.01f || Mathf.Abs(vel.z) > 0.01f)
            {
                vel.x = 0f;
                vel.x = 0f;
                vel.y = verticalVelocity;
                rb.velocity = vel;
            }
        }
    }

    void CheckForWalls()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        bool hitWall = false;

        if(Physics.Raycast(origin, transform.forward, out hit, wallCheckDistance))
        {
            if(Vector3.Angle(hit.normal, Vector3.up) > 45f)
            {
                hitWall = true;
                wallNormal = hit.normal;
            }
        }
        isTouchingWall = hitWall;
    }

    void WallJump()
    {
        verticalVelocity = jumpForce;

        Vector3 awayDirection = wallNormal * wallJumpAwayForce;

        rb.velocity = new Vector3(awayDirection.x, verticalVelocity, awayDirection.z);

        isJumping = true;
        jumpTimeCounter = 0f;
    }

    void Jump()
    {
        verticalVelocity = jumpForce;
        isJumping = true;
        jumpTimeCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    void OnLand()
    {
        isJumping = false;

        jumpTimeCounter = 0f;

        if(verticalVelocity < -5f)
        {
            verticalVelocity = -1f;
        }
    }

    void HandleSteps()
    {
        float vertical = Input.GetAxis("Vertical");
        if(Mathf.Abs(vertical)<0.1f) return;

        RaycastHit hitObstical; 
        Vector3 chestRayStart = transform.position;

        if(!Physics.Raycast(chestRayStart, transform.forward, out hitObstical, climbCheckDistance))
        {
            return;
        }

        float playerFeet = transform.position.y - 1f;
        float obsticalBottom = hitObstical.point.y;
        float obsticalHeight = obsticalBottom - playerFeet;

        if(obsticalHeight <= 0.05f || obsticalHeight > stepHeight)
        {
            return;
        }
        Vector3 topCheckPosition = new Vector3(hitObstical.point.x + transform.forward.x * 0.3f, hitObstical.point.y + 0.5f, hitObstical.point.z + transform.forward.z * 0.3f);

        RaycastHit hitTop;
        if(Physics.Raycast(topCheckPosition, Vector3.down, out hitTop, 1f))
        {
            float topSurfaceHeight = hitTop.point.y - playerFeet;

            if(topSurfaceHeight > 0.05f && topSurfaceHeight <= stepHeight)
            {
                RaycastHit ceilingCheck;
                if(!Physics.Raycast(transform.position, Vector3.up, out ceilingCheck, 1.5f))
                {
                    rb.position += Vector3.up * stepSmooth;
                    rb.position += transform.forward * 0.5f;
                }
            }
        }
    }

    void HandlingCrouching()
    {
        if(Input.GetKeyDown(crouchKey))
        {
            if(!isCrouching)
            {
                Crouch();
            }
        } else
        {
            if(isCrouching)
            {
                if(CanStandUp())
                {
                    StandUp();
                }
            }
        }
    }

    void Crouch()
    {
        isCrouching = true;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if(capsule != null)
        {
            capsule.height = crouchHeight;
            capsule.center = new Vector3(0, -0.5f, 0);
        }

        if(cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(0, -0.5f, 0);
        }
    }

    void StandUp()
    {
        isCrouching = false;
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.height = standingHeight;
            capsule.center = new Vector3(0, 0, 0);
        }
        
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(0, 0.5f, 0);
        }
    }

    bool CanStandUp()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position;
        float checkHeight = standingHeight + 0.2f;
        return !Physics.Raycast(rayStart, Vector3.up, out hit, checkHeight);
    }

    void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;

        if(isTouchingWall)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(origin, transform.forward * wallCheckDistance);

        Gizmos.color = verticalVelocity > 0 ? Color.yellow : Color.gray;
        Gizmos.DrawRay(transform.position, Vector3.up * verticalVelocity * 0.2f);

        Gizmos.color = isGrounded ? Color.yellow : Color.gray;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.5f);
    }

}