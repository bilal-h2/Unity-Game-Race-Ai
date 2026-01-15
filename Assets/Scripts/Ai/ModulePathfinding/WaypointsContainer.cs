using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsContainer : MonoBehaviour
{
    public Color lineColor = Color.blue;
    public float SubpointsDistance = 1.0f;
    public Transform[] waypoints;

    private List<Transform> extraPoints = new List<Transform>();
    
    public bool[] BusyList = new bool[0];

    private void Start()
    {
        CreateExtraPoints();
    }
    public void RefreshWaypoints()
    {
        waypoints = GetComponentsInChildren<Transform>();

        List<Transform> filteredWaypoints = new List<Transform>(waypoints);

        foreach (Transform t in filteredWaypoints.ToArray())
        {
            if (!t.gameObject.name.ToLower().Contains("waypoint"))
            {
                filteredWaypoints.Remove(t);
            }
        }

        filteredWaypoints.Remove(transform);
        waypoints = filteredWaypoints.ToArray();

        int i = 0;

        foreach (var wp in waypoints)
        {
            wp.gameObject.name = $"Waypoint{i + 1}";

            i++;
        }
        AlignWaypoints();

        BusyList = new bool[waypoints.Length];
    }
    private void AlignWaypoints()
    {
        foreach (var wp in waypoints)
        {
            wp.LookAt(waypoints[(wp.GetSiblingIndex() + 1) % waypoints.Length]);
        }
    }
    private void CreateExtraPoints()
    {
        foreach (var wp in waypoints)
        {
            // Create extra point objects
            Transform leftPoint = new GameObject("LeftPoint").transform;
            Transform rightPoint = new GameObject("RightPoint").transform;
            // Parent extra points to the waypoint container
            leftPoint.parent = wp;
            rightPoint.parent = wp;

            // Position extra points
            leftPoint.localPosition = Vector3.zero;
            leftPoint.localRotation = Quaternion.identity;

            rightPoint.localPosition = Vector3.zero;
            rightPoint.localRotation = Quaternion.identity;

            leftPoint.localPosition -= Vector3.right * SubpointsDistance;
            rightPoint.localPosition += Vector3.right * SubpointsDistance;

            // Add extra points to the list
            extraPoints.Add(leftPoint);
            extraPoints.Add(rightPoint);
        }

    }

    private void OnDrawGizmos()
    {
        DrawPath();
        DrawExtraPoints();
    }

    private void DrawPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform currentWaypoint = waypoints[i];
            Transform nextWaypoint = waypoints[(i + 1) % waypoints.Length];

            if (currentWaypoint == null || nextWaypoint == null) continue;

            DrawLine(currentWaypoint.position, nextWaypoint.position);
        }
    }

    private void DrawExtraPoints()
    {
        if(extraPoints == null || extraPoints.Count == 0) return;

        Gizmos.color = Color.red;

        foreach (var extraPoint in extraPoints)
        {
            Gizmos.DrawSphere(extraPoint.position, 0.5f);
        }

        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i].position, 0.5f);
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos)
    {
        Gizmos.color = lineColor;
        Gizmos.DrawLine(startPos, endPos);
    }

    internal Transform GetPoint(int currentWaypointIndex)
    {
        if (BusyList[currentWaypointIndex])
        {
            return GetChildPoint(currentWaypointIndex);
        }
        else
        {
            return waypoints[currentWaypointIndex];
        }
    }
    internal Transform GetChildPoint(int index)
    {
        return waypoints[index].GetChild(Random.Range(0, waypoints[index].childCount));
    }

    public void EasyPoint(int index)
    {
        BusyList[index] = false;
    }
    public void BusyPoint(int index)
    {
        BusyList[index] = true;
    }
}
