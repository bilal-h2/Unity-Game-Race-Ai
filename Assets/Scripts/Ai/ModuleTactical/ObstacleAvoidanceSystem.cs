using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AIController : MonoBehaviour
{
    public enum SensorPosition
    {
        Left,
        Right,
        FrontLeft,
        FrontRight,
        Back,
        Front,
        BackLeft,
        BackRight
    }

    [Header("Obstacle Avoidance")]
    public bool ShowObstaclesGizmos = false;

    public Transform SensorsContainer;
    public float SensorDistence = 10;
    public float FrontSensorDistence = 10;

    private ObstacleDetectionSensor[] sensors;

    public bool Collision_OnFront = false;
    public bool Collision_OnLeft = false;
    public bool Collision_OnRight = false;
    public bool Collision_OnFrontLeft = false;
    public bool Collision_OnFrontRight = false;
    public bool ThereIsSomethingOnBack = false;
    public bool ThereIsSomethingOnBackLeft = false;
    public bool ThereIsSomethingOnBackRight = false;

    public float CollisionDistance = 0;

    private void InitializeObstacleSensors()
    {
        sensors = SensorsContainer.GetComponentsInChildren<ObstacleDetectionSensor>();
    }

    private void AvoidObstacles()
    {
        if(sensors == null || sensors.Length == 0) return;

        int _Count = sensors.Length;

        for (int i = 0; i < _Count; ++i)
        {
            var raycastOrigin = sensors[i].transform;
            var _sensor = sensors[i].sensorPosition;
            var raycastDirection = raycastOrigin.forward;

            if (Physics.Raycast(raycastOrigin.position, raycastDirection, out RaycastHit hit, _sensor == SensorPosition.Front ? FrontSensorDistence : SensorDistence))
            {
                if (ShowObstaclesGizmos)
                {
                    Debug.DrawRay(raycastOrigin.position, raycastDirection * (_sensor == SensorPosition.Front ? FrontSensorDistence : SensorDistence), Color.red);
                }

                bool isGround = hit.collider.gameObject.name.Contains("Terrain");
                if (!isGround)
                {
                    switch (_sensor)
                    {
                        case SensorPosition.Left: Collision_OnLeft = true; break;
                        case SensorPosition.Right: Collision_OnRight = true; break;
                        case SensorPosition.FrontLeft: Collision_OnFrontLeft = true; break;
                        case SensorPosition.FrontRight: Collision_OnFrontRight = true; break;
                        case SensorPosition.Front:
                            Collision_OnFront = true;
                            CollisionDistance = Vector3.Distance(sensors[i].transform.position, hit.collider.transform.position);
                            break;
                        case SensorPosition.BackLeft: ThereIsSomethingOnBackLeft = true; break;
                        case SensorPosition.BackRight: ThereIsSomethingOnBackRight = true; break;
                        case SensorPosition.Back: ThereIsSomethingOnBack = true; break;
                    }
                }
                else
                {
                    switch (_sensor)
                    {
                        case SensorPosition.Left: Collision_OnLeft = false; break;
                        case SensorPosition.Right: Collision_OnRight = false; break;
                        case SensorPosition.FrontLeft: Collision_OnFrontLeft = false; break;
                        case SensorPosition.FrontRight: Collision_OnFrontRight = false; break;
                        case SensorPosition.Front:
                            Collision_OnFront = false;
                            CollisionDistance = 0;
                            break;
                        case SensorPosition.BackLeft: ThereIsSomethingOnBackLeft = false; break;
                        case SensorPosition.BackRight: ThereIsSomethingOnBackRight = false; break;
                        case SensorPosition.Back: ThereIsSomethingOnBack = false; break;
                    }
                }
            }
            else
            {
                switch (_sensor)
                {
                    case SensorPosition.Left: Collision_OnLeft = false; break;
                    case SensorPosition.Right: Collision_OnRight = false; break;
                    case SensorPosition.FrontLeft: Collision_OnFrontLeft = false; break;
                    case SensorPosition.FrontRight: Collision_OnFrontRight = false; break;
                    case SensorPosition.Front:
                        Collision_OnFront = false;
                        CollisionDistance = 0;
                        break;
                    case SensorPosition.BackLeft: ThereIsSomethingOnBackLeft = false; break;
                    case SensorPosition.BackRight: ThereIsSomethingOnBackRight = false; break;
                    case SensorPosition.Back: ThereIsSomethingOnBack = false; break;
                }

                if (ShowObstaclesGizmos)
                {
                    Debug.DrawRay(raycastOrigin.position, raycastDirection * (_sensor == SensorPosition.Front ? FrontSensorDistence : SensorDistence), Color.green);
                }
            }
        }
    }

    public float modifiedAccelInput;

    private void Avoidance()
    {
        if (!Collision_OnFront) return;


    }
    public bool CheckClearPathForOvertake(float opponentDistance, float safeDistance)
    {
        float futurePosition = vehicleController.DistanceTraveled + safeDistance;
        bool isPathClear = CheckObstaclesInPath(vehicleController.DistanceTraveled, futurePosition);

        return isPathClear;
    }

    public bool CheckClearPathForDefense(float defensiveDistance)
    {
        float futurePosition = vehicleController.DistanceTraveled - defensiveDistance;
        bool isPathClear = CheckObstaclesInPath(futurePosition, vehicleController.DistanceTraveled);

        return isPathClear;
    }
    
    private bool CheckObstaclesInPath(float startDistance, float endDistance)
    {
        Vector3 startPosition = GetPositionAlongTrack(startDistance);
        Vector3 endPosition = GetPositionAlongTrack(endDistance);

        Vector3 pathDirection = (endPosition - startPosition).normalized;
        float pathLength = Vector3.Distance(startPosition, endPosition);

        RaycastHit hit;
        if (Physics.Raycast(startPosition, pathDirection, out hit, pathLength))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.DrawRay(startPosition, pathDirection * hit.distance, Color.red);
                return false;
            }
        }

        Debug.DrawRay(startPosition, pathDirection * pathLength, Color.green);
        return true;
    }

    private Vector3 GetPositionAlongTrack(float distance)
    {
        Transform[] trackWaypoints = waypointSystem.waypoints;

        int segmentIndex = 0;
        float segmentDistance = distance;

        for (int i = 0; i < trackWaypoints.Length - 1; i++)
        {
            Vector3 waypointA = trackWaypoints[i].position;
            Vector3 waypointB = trackWaypoints[i + 1].position;
            float segmentLength = Vector3.Distance(waypointA, waypointB);

            if (segmentDistance < segmentLength)
            {
                segmentIndex = i;
                break;
            }
            else
            {
                segmentDistance -= segmentLength;
            }
        }

        Vector3 startPosition = trackWaypoints[segmentIndex].position;
        Vector3 endPosition = trackWaypoints[segmentIndex + 1].position;
        float t = segmentDistance / Vector3.Distance(startPosition, endPosition);
        Vector3 interpolatedPosition = Vector3.Lerp(startPosition, endPosition, t);

        return interpolatedPosition;
    }

}
