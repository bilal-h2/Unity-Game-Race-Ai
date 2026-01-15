using UnityEngine;

public class PacejkaTireModel : MonoBehaviour
{
    [Header("Coefficients")]
    public float longitudinalStiffness;
    public float lateralStiffness;
    public float camberStiffness;

    [Header("Debugging")]
    public float DebugLateralSlip;
    public float DebugLongitudinalSlip;
    public float DebugCamberAngle;
    public float DebugLateralForce;
    public float DebugLongitudinalForce;

    private WheelCollider wheelCollider;

    private void OnEnable()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    private void FixedUpdate()
    {
        float lateralSlip = CalculateLateralSlip;
        float longitudinalSlip = CalculateLongitudinalSlip;
        float camberAngle = CalculateCamberAngle;

        float lateralForce = CalculateLateralForce(lateralSlip, camberAngle);
        float longitudinalForce = CalculateLongitudinalForce(longitudinalSlip);

        DebugLateralSlip = lateralSlip;
        DebugLongitudinalSlip = longitudinalSlip;
        DebugCamberAngle = camberAngle;
        DebugLateralForce = lateralForce;
        DebugLongitudinalForce = longitudinalForce;

        ApplyForces(lateralForce, longitudinalForce);
    }
    private float CalculateLateralSlip
    {
        get
        {
            float stiff = wheelCollider.sidewaysFriction.stiffness;
            float lateralVelocity = 0.1f;

            if (wheelCollider.GetGroundHit(out WheelHit hit))
            {
                lateralVelocity = stiff * hit.sidewaysSlip;
            }

            return Mathf.Atan(lateralVelocity / wheelCollider.attachedRigidbody.velocity.magnitude);
        }
    }

    private float CalculateLongitudinalSlip
    {
        get
        {
            float stiff = wheelCollider.forwardFriction.stiffness;
            float longitudinalVelocity = 0;

            if (wheelCollider.GetGroundHit(out WheelHit hit))
            {
                longitudinalVelocity = stiff * hit.forwardSlip;
            }

            return (wheelCollider.attachedRigidbody.velocity.magnitude - longitudinalVelocity) / wheelCollider.attachedRigidbody.velocity.magnitude;
        }
    }

    private float CalculateCamberAngle
    {
        get
        {
            return wheelCollider.transform.localEulerAngles.z;
        }
    }

    private float CalculateLateralForce(float lateralSlip, float camberAngle)
    {
        return lateralStiffness * Mathf.Sin(lateralSlip - camberStiffness * Mathf.Atan(camberAngle));
    }

    private float CalculateLongitudinalForce(float longitudinalSlip)
    {
        return longitudinalStiffness * longitudinalSlip;
    }

    private void ApplyForces(float lateralForce, float longitudinalForce)
    {
        WheelFrictionCurve lateralFriction = wheelCollider.sidewaysFriction;
        lateralFriction.stiffness = lateralForce;
        wheelCollider.sidewaysFriction = lateralFriction;

        wheelCollider.motorTorque = longitudinalForce;
    }

}
