using System.Collections;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public enum WheelsDrive
    {
        Front, Back, All
    }

    [Header("Basic Settings")]
    public WheelsDrive DriveType;
    public float MaximumSpeed = 100; // Max speed in km/h
    public float EngineTorque = 400; // Base engine power
    public float BrakeForce = 150f;  // Force applied when braking

    // UI/Telemetry Accessors (kept for compatibility)
    public float DistanceTraveled = 1;
    public float GetCurrentSpeed => _currentSpeed;
    public float GetCurrentGear => currentGear;
    public float GetCurrentSteerAngle => FrontLeftWheelsCollider.steerAngle / CurrentSteerAngle;
    public float GetCurrentAccel => accel;

    public int TeamID = 1;
    public int RacerID = 1;

    // Center of mass (CoM) is crucial for stability
    public Transform CenterOfMass;

    [Header("Wheels")]
    // Wheel Colliders
    public WheelCollider FrontLeftWheelsCollider;
    public WheelCollider FrontRightWheelsCollider;
    public WheelCollider BackLeftWheelsCollider;
    public WheelCollider BackRightWheelsCollider;
    // Visual Wheel Transforms
    public Transform FrontLeftTransform;
    public Transform FrontRightTransform;
    public Transform BackLeftTransform;
    public Transform BackRightTransform;

    [Header("Gears")]
    public int numberOfGears = 5;
    // Gear ratios should be set in the Inspector (e.g., [4.0f, 2.5f, 1.8f, 1.2f, 0.8f])
    public float[] gearRatios;
    public float UpShiftSpeed = 30f; // Speed threshold for up-shifting
    public float DownShiftSpeed = 15f; // Speed threshold for down-shifting

    [Header("Steering Logic")]
    public float maxSteeringAngle = 30f;
    public float minSteeringAngle = 10f;
    public float maxSpeedForMinSteering = 50f;
    public float minSpeedForMaxSteering = 15f;

    public float CurrentSteerAngle = 25f; // The calculated steering angle

    [Header("Reversing Logic")]
    // Kept as requested
    public float ReverseAlignThreshold = 0.1f;
    public bool isMovingReverse = false;
    public Vector3 Velocity = Vector3.zero;

    // Private State Variables
    private int currentGear = 0;
    private Rigidbody RIGIDBODY;
    [SerializeField] private float _currentSpeed; // Current speed in km/h
    private float accel = 0; // Current acceleration input (0 to 1)

    // UI reference (kept)
    private SpeedMeterUI meterUI;

    // Reverse Check Property (Kept as requested, though using Vector3 is more accurate)
    private bool ReverseCheck
    {
        get
        {
            Vector3 velocity3D = RIGIDBODY.velocity;
            Vector3 vehicleForward = transform.forward;
            float alignment = Vector3.Dot(velocity3D.normalized, vehicleForward.normalized);

            if (alignment < -ReverseAlignThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void Awake()
    {
        // Initial setup for the car
        InitialSettings();
    }

    private void InitialSettings()
    {
        meterUI = FindObjectOfType<SpeedMeterUI>();

        if (!TryGetComponent<Rigidbody>(out RIGIDBODY))
        {
            RIGIDBODY = gameObject.AddComponent<Rigidbody>();
        }

        // IMPROVEMENT: Lower the Center of Mass for better stability and realistic handling
        if (CenterOfMass != null)
        {
            Vector3 localCoM = CenterOfMass.localPosition;
            localCoM.y *= 0.85f; // Adjust this factor for desired stability (e.g., 0.85f lowers it by 15%)
            RIGIDBODY.centerOfMass = localCoM;
        }

        // Default gear ratios logic (kept)
        if (gearRatios == null || gearRatios.Length == 0)
        {
            gearRatios = new float[numberOfGears];

            for (int i = 0; i < numberOfGears; i++)
            {
                gearRatios[i] = 4.0f - i * 0.6f; // Simple decreasing ratio example
            }
        }
    }

    public void FixedUpdate()
    {
        // Apply rotation to the wheel models
        UpdateWheels();

        // Check the current speed
        SpeedCheck();

        // Control Gears
        ChangeGear();

        // Updating the UI
        if (meterUI) meterUI.SetUI(_currentSpeed, (currentGear + 1));
    }

    public void SetInput(float _accel, float steer, float _braking)
    {
        accel = _accel;
        bool braking = _braking > 0.01f;

        // --- 1. TORQUE APPLICATION (IMPROVEMENT) ---
        float currentGearRatio = gearRatios[currentGear];
        float finalTorque = 0f;

        // Apply Motor Torque only if not braking
        if (!braking)
        {
            // IMPROVEMENT: Only allow motor torque up to MaximumSpeed
            if (_currentSpeed < MaximumSpeed)
            {
                // Correct Physics Logic: Total Torque = Input * Base Engine Torque * Gear Ratio
                finalTorque = accel * EngineTorque * currentGearRatio;
            }

            // Reset brake torque
            ApplyBrakeTorque(0f);
        }
        else // Braking
        {
            // Apply brake force to ALL wheels (IMPROVEMENT: for better, realistic stopping)
            ApplyBrakeTorque(BrakeForce * _braking);

            // Set motor torque to zero when braking
            finalTorque = 0f;
        }

        // Apply the calculated final torque to the drive wheels
        ApplyMotorTorque(finalTorque);


        // --- 2. STEERING LOGIC ---
        if (steer != 0)
        {
            // Adjust steering angle based on speed for smoothness and realism
            float speedRatio = Mathf.Clamp01((_currentSpeed - minSpeedForMaxSteering) / (maxSpeedForMinSteering - minSpeedForMaxSteering));
            CurrentSteerAngle = Mathf.Lerp(maxSteeringAngle, minSteeringAngle, speedRatio);

            // Apply steering angle to the front wheels
            float steerAngle = steer * CurrentSteerAngle;
            FrontLeftWheelsCollider.steerAngle = steerAngle;
            FrontRightWheelsCollider.steerAngle = steerAngle;
        }
        else
        {
            // IMPROVEMENT: Slowly reset steer angle when input is zero for smoother return
            float currentSteer = FrontLeftWheelsCollider.steerAngle;
            float newSteer = Mathf.Lerp(currentSteer, 0f, Time.deltaTime * 5f);
            FrontLeftWheelsCollider.steerAngle = newSteer;
            FrontRightWheelsCollider.steerAngle = newSteer;
        }
    }

    // Helper function to cleanly apply motor torque based on drive type
    private void ApplyMotorTorque(float torque)
    {
        // Must reset torque every frame before applying the new one
        FrontLeftWheelsCollider.motorTorque = 0f;
        FrontRightWheelsCollider.motorTorque = 0f;
        BackLeftWheelsCollider.motorTorque = 0f;
        BackRightWheelsCollider.motorTorque = 0f;

        switch (DriveType)
        {
            case WheelsDrive.Front:
                FrontLeftWheelsCollider.motorTorque = torque;
                FrontRightWheelsCollider.motorTorque = torque;
                break;
            case WheelsDrive.Back:
                BackLeftWheelsCollider.motorTorque = torque;
                BackRightWheelsCollider.motorTorque = torque;
                break;
            case WheelsDrive.All:
                FrontLeftWheelsCollider.motorTorque = torque / 2f;
                FrontRightWheelsCollider.motorTorque = torque / 2f;
                BackLeftWheelsCollider.motorTorque = torque / 2f;
                BackRightWheelsCollider.motorTorque = torque / 2f;
                break;
        }
    }

    // Helper function to cleanly apply brake torque to all wheels
    private void ApplyBrakeTorque(float force)
    {
        FrontLeftWheelsCollider.brakeTorque = force;
        FrontRightWheelsCollider.brakeTorque = force;
        BackLeftWheelsCollider.brakeTorque = force;
        BackRightWheelsCollider.brakeTorque = force;
    }

    private void SpeedCheck()
    {
        // Calculate the current speed in kilometers per hour
        _currentSpeed = RIGIDBODY.velocity.magnitude * 3.6f;
    }

    private void UpdateWheels()
    {
        // Optimized wheel model update (kept as is, it's efficient)
        ApplyWheelTransform(FrontLeftWheelsCollider, FrontLeftTransform);
        ApplyWheelTransform(FrontRightWheelsCollider, FrontRightTransform);
        ApplyWheelTransform(BackLeftWheelsCollider, BackLeftTransform);
        ApplyWheelTransform(BackRightWheelsCollider, BackRightTransform);
    }

    private void ApplyWheelTransform(WheelCollider collider, Transform visualMesh)
    {
        collider.GetWorldPose(out var pos, out var rot);
        visualMesh.SetPositionAndRotation(pos, rot);
    }

    private void ChangeGear()
    {
        float currentAbsSpeed = Mathf.Abs(_currentSpeed);

        // Up-Shift Logic
        if (currentGear < numberOfGears - 1 && currentAbsSpeed > (UpShiftSpeed * (currentGear + 1)))
        {
            currentGear++;
        }
        // Down-Shift Logic
        else if (currentGear > 0 && currentAbsSpeed < (DownShiftSpeed * currentGear))
        {
            currentGear--;
        }
    }
}