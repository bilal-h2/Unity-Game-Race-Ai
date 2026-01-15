using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointsContainer))]
public class WaypointsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaypointsContainer waypointsContainer = (WaypointsContainer)target;

        if (GUILayout.Button("Refresh Waypoints"))
        {
            waypointsContainer.RefreshWaypoints();
        }
    }
}
