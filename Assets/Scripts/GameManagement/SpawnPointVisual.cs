using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointVisual : MonoBehaviour
{
    public Color gizmoColor = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Vector3 cubeSize = transform.localScale;
        Vector3 cubePosition = transform.position;
        Quaternion cubeRotation = transform.rotation;

        Gizmos.matrix = Matrix4x4.TRS(cubePosition, cubeRotation, cubeSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
