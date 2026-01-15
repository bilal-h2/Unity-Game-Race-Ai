using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [Header("Target Object")]
    public Transform targetVehicle;
    [Header("Follow Distance")]
    public float distance = 5f;
    public float height = 2f;

    [Header("Speeds")]
    public float turnSpeed = 5.0f;

    public void SetTarget(Transform target)
    {
        targetVehicle  = target;
    }

    //void FixedUpdate()
    //{
    //    if (!targetVehicle) return;

    //    var _TargetYAngle = targetVehicle.eulerAngles.y;
    //    var _TargetHeight = targetVehicle.position.y + height;

    //    var _camHeight = transform.position.y;
    //    var _camYAngle = transform.eulerAngles.y;

    //    _camYAngle = Mathf.LerpAngle(_camYAngle, _TargetYAngle, turnSpeed * Time.deltaTime);
    //    _camHeight = Mathf.Lerp(_camHeight, _TargetHeight, turnSpeed * Time.deltaTime);

    //    var _finalRotation = Quaternion.Euler(0, _camYAngle, 0);

    //    transform.position = targetVehicle.position;
    //    transform.position -= _finalRotation * Vector3.forward * distance;
    //    transform.position = new Vector3(transform.position.x, _camHeight, transform.position.z);

    //    transform.LookAt(targetVehicle);
    //}
    void LateUpdate()
    {
        if (!targetVehicle) return;

        var _TargetYAngle = targetVehicle.eulerAngles.y;
        var _TargetHeight = targetVehicle.position.y + height;

        var _camHeight = transform.position.y;
        var _camYAngle = transform.eulerAngles.y;

        _camYAngle = Mathf.LerpAngle(_camYAngle, _TargetYAngle, turnSpeed * Time.fixedDeltaTime);
        _camHeight = Mathf.Lerp(_camHeight, _TargetHeight, (turnSpeed/2) * Time.fixedDeltaTime);

        var _finalRotation = Quaternion.Euler(0, _camYAngle, 0);

        transform.position = targetVehicle.position;
        transform.position -= _finalRotation * Vector3.forward * distance;
        transform.position = new Vector3(transform.position.x, _camHeight, transform.position.z);

        transform.LookAt(targetVehicle);
    }

}
