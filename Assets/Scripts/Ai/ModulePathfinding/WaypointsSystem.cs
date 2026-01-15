using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class AIController
{
    [Header("Pathfinding System")]
    public bool ShowPathGizmos = false;
    //Waypoint system reference
    public WaypointsContainer waypointSystem;

    //Reference to the current waypoint index
    public int currentWaypointIndex;

    //The next waypoint jumping distance
    //If the vehicle is near to the targetted waypoint and the distance left
    //The targetted waypoint will be changed to the next waypoint
    public float distanceToCatchWaypoint = 10f;

    //This is to show the progress of this Ai
    public int WaypointsPassed = 0;
    public int Lap = 1;

    //Checking the required angle to reach the next waypoint
    //This is related to the steering input and acceleration controll
    private float NextAngle = 0;
    private Transform _CurrentWaypoint;
    private NavMeshAgent agent;
    private Vector3[] PathPoints = new Vector3[0];

    [HideInInspector] public float DistanceToPathPoint = 0;
    [HideInInspector] public float DistanceToWaypoint = 0;

    //This property returns the waypoint by index
    private Transform GetNextWaypoint
    {
        get
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointSystem.waypoints.Length;
            return waypointSystem.GetPoint(currentWaypointIndex);
        }
    }

    //Interpolating the smoothness of the steering
    private float GetSteerSmoothnessByCurrentSpeed
    {
        get
        {
            //Define the variables to make it clean and adjustable:
            const float minSpeedForMaxSmooth = 25f;
            const float maxSpeedForMinSmooth = 40f;

            const float maxSmoothness = 2.5f;
            const float minSmoothness = 1.5f;

            //Ensure the vehicleController reference is available
            float currentSpeed = vehicleController.GetCurrentSpeed;

            float t = Mathf.InverseLerp(minSpeedForMaxSmooth, maxSpeedForMinSmooth, currentSpeed);

            //Interpolate the smoothness value
            return Mathf.Lerp(maxSmoothness, minSmoothness, t);
        }
    }

    private float GetAcceleration
    {
        get
        {
            //Error Check
            if (_CurrentWaypoint == null)
            {
                Debug.LogWarning("Waypoint index error! _CurrentWaypoint is null.");
                return 0f;
            }

            //Input Variables
            //Get the absolute angle to the next waypoint
            float angleDiff = Mathf.Abs(NextAngle);

            //Define Acceleration Boundaries
            float accelForSharpTurn;
            float accelForStraight;

            // Using descriptive public/Serialized variables instead of magic numbers
            if (IsSpeedupDecision)
            {
                accelForSharpTurn = 0.30f;
                accelForStraight = 0.85f;
            }
            else
            {
                accelForSharpTurn = 0.15f;
                accelForStraight = 0.50f;
            }

            //Define the angle range for the acceleration curve
            const float straightAngleThreshold = 5f;
            const float sharpAngleThreshold = 60f;

            //InverseLerp converts the angle range into a 0-1 factor
            float t = Mathf.InverseLerp(straightAngleThreshold, sharpAngleThreshold, angleDiff);

            //Apply Smoothing and Interpolate
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            //Interpolate the acceleration
            float desiredAcceleration = Mathf.Lerp(accelForStraight, accelForSharpTurn, smoothedT);

            return desiredAcceleration;
        }
    }
    private Vector3 GetTargetPosition
    {
        get
        {
            return (PathPoints != null && PathPoints.Length > 1) ? PathPoints[1] : _CurrentWaypoint.position;
        }
    }
    private void InitializeWaypoints()
    {
        waypointSystem = FindObjectOfType<WaypointsContainer>();
        agent = GetComponent<NavMeshAgent>();

        _CurrentWaypoint = GetNextWaypoint;

    }

    private void UpdateWaypointsDrive()
    {
        //Check if the waypoint system reference assigned
        if (waypointSystem == null)
        {
            print("WaypointSystem reference missing!");

            return;
        }
        //If the current waypoint is null in reference
        //Abort the operation
        if (_CurrentWaypoint == null)
        {
            print("Waypoint index error!");
            return;
        }
        //Getting the new path by avoiding obstacles
        GetPath();

        //Sync steering senstivity according to the speed
        SteeringSensitivity = GetSteerSmoothnessByCurrentSpeed;

        //Get the direction towards the current waypoint
        //This is used to get the angle difference between vehicle's and the waypoint
        var _targetPosition = GetTargetPosition;
        var _targetDirection = (_targetPosition - transform.position).normalized;

        //Get the angle between the vehicles direction and the targetVehicle direction
        NextAngle = Vector3.SignedAngle(transform.forward, _targetDirection, transform.up);

        //Calculate the distance to the current waypoint
        DistanceToWaypoint = Vector3.Distance(transform.position, _CurrentWaypoint.position);

        DistanceToPathPoint = Vector3.Distance(transform.position, _targetPosition);

        float _accel = GetAcceleration;

        //Calculate the desired normalized steering angle
        float _normalAngle = Mathf.Clamp(NextAngle / vehicleController.maxSteeringAngle, -1f, 1f);

        //Smoothly transition to the desired steering angle over time
        float _smoothAngle = Mathf.Lerp(vehicleController.GetCurrentSteerAngle, _normalAngle, Time.deltaTime * SteeringSensitivity);

        //Inverse the angle is reversing by obstacles
        float _newSteeringAngle = IsReverseDecision ? -_smoothAngle : _smoothAngle;

        //The target point is from new path
        if (PathPoints.Length > 1)
        {
            float _braking = 0;

            if (IsBrakingDecision)
            {
                _braking = Mathf.Clamp(vehicleController.GetCurrentSpeed, 0, 50) / 50;
            }
            else if (!IsBrakingDecision)
            {
                _braking = 0;
            }

            vehicleController.SetInput(_accel, _newSteeringAngle, _braking);
        }
        //Path is not valid, going direct to Waypoint
        else
        {
            if (!IsBrakingDecision)
            {
                //Send the inputs to the vehicle controller
                vehicleController.SetInput(_accel, _newSteeringAngle, 0);
            }
        }

        if (IsReverseDecision)
        {
            _accel = -0.6f;

            vehicleController.SetInput(_accel, _newSteeringAngle, 0);
        }

        //If the vehicle is close to the current waypoint, proceed to the next waypoint
        if (DistanceToWaypoint < distanceToCatchWaypoint)
        {
            MoveToNextWaypoint();
        }
    }
    private void MoveToNextWaypoint()
    {
        WaypointsPassed++;

        _CurrentWaypoint = GetNextWaypoint;

        waypointSystem.BusyPoint(currentWaypointIndex);

        int previousWaypointIndex = Mathf.Max(currentWaypointIndex - 1, 0);
        waypointSystem.EasyPoint(previousWaypointIndex);

        if (currentWaypointIndex == 0)
        {
            waypointSystem.EasyPoint(waypointSystem.waypoints.Length - 1);
        }

        if (WaypointsPassed >= waypointSystem.waypoints.Length)
        {
            WaypointsPassed = 0;
            Lap++;
        }

        if (positionSystem) positionSystem.SyncProgress(this);
    }
    private void GetPath()
    {
        if (!agent) return;

        if (_CurrentWaypoint != null)
        {
            NavMeshPath navMeshPath = new NavMeshPath();

            if (agent.enabled && agent.isOnNavMesh)
            {
                if (agent.CalculatePath(_CurrentWaypoint.position, navMeshPath))
                {
                    PathPoints = navMeshPath.corners;

                    if (PathIsValid(PathPoints))
                    {
                        agent.SetPath(navMeshPath);
                    }
                    else
                    {
                        Vector3[] newPathCorners = GenerateAlternativePath(navMeshPath);

                        if (newPathCorners != null)
                        {
                            NavMeshPath newPath = new NavMeshPath();
                            Vector3 previousCorner = transform.position;

                            foreach (Vector3 corner in newPathCorners)
                            {
                                NavMesh.CalculatePath(previousCorner, corner, NavMesh.AllAreas, newPath);
                                previousCorner = corner;
                            }

                            agent.SetPath(newPath);
                            PathPoints = newPath.corners;
                        }
                    }
                }
            }
        }
    }

    private Vector3[] GenerateAlternativePath(NavMeshPath originalPath)
    {
        List<Vector3> newPathCorners = new List<Vector3>();

        foreach (Vector3 corner in originalPath.corners)
        {
            newPathCorners.Add(corner);

            if (!PathIsValid(newPathCorners.ToArray()))
            {
                newPathCorners.RemoveAt(newPathCorners.Count - 1);

                newPathCorners.Add(FindSafePointAroundObstacle(corner));
            }
        }

        return newPathCorners.ToArray();
    }

    private Vector3 FindSafePointAroundObstacle(Vector3 obstaclePosition)
    {
        Vector3 randomOffset = Random.insideUnitSphere * 3;
        randomOffset.y = 0;

        return obstaclePosition + randomOffset;
    }

    private bool PathIsValid(Vector3[] pathCorners)
    {
        if (pathCorners.Length <= 1)
        {
            return false;
        }

        for (int i = 1; i < pathCorners.Length; i++)
        {
            if (Physics.Linecast(pathCorners[i - 1], pathCorners[i], out RaycastHit hit) && !hit.collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }
    private void OnDrawGizmos()
    {
        if (!ShowPathGizmos) return;

        if (agent != null && agent.path != null)
        {
            for (int i = 0; i < PathPoints.Length - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(PathPoints[i], PathPoints[i + 1]);

                if (i == 1)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;
                Gizmos.DrawSphere(PathPoints[i], 1);
            }
        }
    }
}
