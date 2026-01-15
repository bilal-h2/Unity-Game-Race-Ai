using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifferentialSystem : MonoBehaviour
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;

    public float powerBias = 0.5f;
    public float differentialEffect = 0.2f;
    public float maxDrag = 0.08f;
    public float minDrag = 0.00f;
    public float CurrentDrag = 0;
    public float CurrentSteering = 0;



    private Rigidbody rb;
    private VehicleController vehicleController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        vehicleController = GetComponent<VehicleController>();
    }

    private void FixedUpdate()
    {
        float differentialTorque = (leftWheel.motorTorque - rightWheel.motorTorque) * differentialEffect;

        float totalTorque = rb.mass * rb.centerOfMass.y * Mathf.PI * (leftWheel.radius + rightWheel.radius) * powerBias;

        float steeringAngle = vehicleController.GetCurrentSteerAngle;
        CurrentSteering = steeringAngle;

        float normalizedSteering = Mathf.Abs(steeringAngle);
        
        if (normalizedSteering > 0.1f)
        {
            CurrentDrag = Mathf.Lerp(CurrentDrag, maxDrag, 3f * Time.deltaTime);
        }
        else
        {
            CurrentDrag = Mathf.Lerp(CurrentDrag, minDrag, 3f * Time.deltaTime);
        }

        rb.drag = CurrentDrag;

        float leftTorque = totalTorque - differentialTorque * 0.5f;
        float rightTorque = totalTorque + differentialTorque * 0.5f;

        if (Mathf.Abs(steeringAngle) > 0.1f)
        {
            float turnFactor = Mathf.Clamp(rb.velocity.magnitude / 10f, 0f, 1f);
            leftTorque -= turnFactor * Mathf.Sign(steeringAngle) * differentialTorque * 0.5f;
            rightTorque += turnFactor * Mathf.Sign(steeringAngle) * differentialTorque * 0.5f;
        }

        leftWheel.motorTorque = leftTorque;
        rightWheel.motorTorque = rightTorque;
    }
}
