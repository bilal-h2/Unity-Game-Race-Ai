using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform vehicleTransform;
    public float sharpTurnThreshold = 30.0f;

    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints.");
            return;
        }

        Vector3[] waypointDirections = new Vector3[waypoints.Length - 1];
        for (int i = 0; i < waypointDirections.Length; i++)
        {
            waypointDirections[i] = waypoints[i + 1].position - waypoints[i].position;
            waypointDirections[i].Normalize();
        }

        if (currentWaypointIndex < waypointDirections.Length - 1)
        {
            Vector3 currentDirection = waypointDirections[currentWaypointIndex];
            Vector3 nextDirection = waypointDirections[currentWaypointIndex + 1];

            float angle = Vector3.Angle(currentDirection, nextDirection);
            if (angle > sharpTurnThreshold)
            {
                Debug.Log("Sharp turn ahead!");
            }
        }

        if (Vector3.Distance(vehicleTransform.position, waypoints[currentWaypointIndex + 1].position) < 1.0f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length - 1)
            {
                Debug.Log("final waypoint.");
            }
        }
    }
}
