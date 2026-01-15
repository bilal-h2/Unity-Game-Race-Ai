using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifferentialSystemTire : MonoBehaviour
{
    public float gripCoefficient = 1.0f;

    private WheelCollider wheelCollider;

    private void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    private void FixedUpdate()
    {
        float wheelSlip = Mathf.Abs(wheelCollider.rpm) / wheelCollider.forwardFriction.stiffness;

        float gripFactor = Mathf.Clamp01(1.0f - gripCoefficient * wheelSlip);

        WheelFrictionCurve friction = wheelCollider.forwardFriction;
        friction.stiffness *= gripFactor;
    }
}
