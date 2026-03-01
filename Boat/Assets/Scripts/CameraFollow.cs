using UnityEngine;

public class ShipCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public bool autoFindTarget = true;
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 8, -12);
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;
    
    [Header("Look Settings")]
    public bool lookAtTarget = true;
    public Vector3 lookOffset = new Vector3(0, 2, 0);
    
    void Start()
    {
        FindTarget();
    }
    
    void LateUpdate()
    {
        // Try to find target if missing
        if (target == null && autoFindTarget)
        {
            FindTarget();
            return;
        }
        
        if (target == null) return;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // Look at target
        if (lookAtTarget)
        {
            Vector3 lookPosition = target.position + lookOffset;
            Quaternion targetRotation = Quaternion.LookRotation(lookPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void FindTarget()
    {
        // Try to find by name
        GameObject ship = GameObject.Find("PlayerShip");
        
        if (ship != null)
        {
            target = ship.transform;
        }
        else
        {
            // Try to find by tag
            ship = GameObject.FindGameObjectWithTag("Player");
            if (ship != null)
            {
                target = ship.transform;
            }
            else
            {
                Debug.LogWarning("Camera cannot find ship!");
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}