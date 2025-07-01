using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);

    [Header("Orbit Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Collision Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private LayerMask collisionLayers;

    private float currentRotationX = 10f; // Start angle looking down
    private float currentRotationY = 0f;
    private float currentDistance;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to OrbitCamera!");
            enabled = false;
            return;
        }

        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleRotationInput();
        UpdateCameraPosition();
        HandleCollision();
    }

    private void HandleRotationInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentRotationY += mouseX;
        currentRotationX -= mouseY;

        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 targetPosition = target.position + offset;

        Vector3 desiredPosition = targetPosition + rotation * new Vector3(0, 0, -currentDistance);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        transform.LookAt(targetPosition);
    }

    private void HandleCollision()
    {
        Vector3 origin = target.position + offset;
        Vector3 direction = (transform.position - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothSpeed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Max(minDistance, newDistance);
    }
}
