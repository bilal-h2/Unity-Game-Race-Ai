using UnityEngine;

public class CinematicCameraLook : MonoBehaviour
{
    // === Public Variables ===

    // The target the camera will look at.
    public Transform target;

    [Header("Rotation Settings")]
    // Controls how quickly the camera rotates (lower is slower/smoother).
    public float rotationSmoothTime = 0.2f;

    [Header("FOV Settings")]
    // The default/resting Field of View.
    public float defaultFOV = 60f;
    // How much the FOV can increase when close.
    public float maxFOVIncrease = 15f;
    // The distance at which the FOV will be at its maximum increase.
    public float closeDistance = 3f;
    // How quickly the FOV changes.
    public float fovChangeSpeed = 5f;

    // === Private Variables ===

    // Used for smooth damping the rotation.
    private Vector3 currentVelocity;
    private Quaternion lookRotation;
    private Camera cam;

    private void Start()
    {
        // Get the Camera component on this GameObject
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CinematicCameraLook requires a Camera component on the same GameObject.");
            enabled = false;
            return;
        }
        // Set the initial FOV
        cam.fieldOfView = defaultFOV;
    }

    private void FixedUpdate()
    {
        // 1. Calculate Direction and Smooth Rotation (Fixing the Lag)
        Vector3 direction = target.position - transform.position;

        // Calculate the target rotation
        lookRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the camera toward the target rotation using Slerp
        // The * Time.deltaTime is removed here because the SmoothDamp for rotation will handle the smoothness
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSmoothTime);

        // 2. Dynamic Field of View (The Cinematic Effect)

        // Calculate the distance to the target
        float distance = direction.magnitude;

        // Determine the factor for FOV increase (0 when far, 1 when at 'closeDistance' or closer)
        // InverseLerp is great for getting a 0-1 factor based on a range.
        float closeFactor = Mathf.InverseLerp(closeDistance * 2, closeDistance, distance);

        // Calculate the new target FOV
        float targetFOV = defaultFOV + (maxFOVIncrease * closeFactor);

        // Smoothly transition the current FOV to the target FOV
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }
}