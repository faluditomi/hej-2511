using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerDetection))]
public class PlayerDetectionEditor : Editor
{
    private void OnSceneGUI()
    {
        PlayerDetection playerDetection = (PlayerDetection)target;

        Handles.color = Color.red;

        Handles.DrawWireArc(playerDetection.transform.position, Vector3.up, Vector3.forward, 360, playerDetection.GetViewRadius());

        Vector3 viewAngleA = playerDetection.VectorFromAngle(-playerDetection.GetViewAngle() / 2f, false);

        Vector3 viewAngleB = playerDetection.VectorFromAngle(playerDetection.GetViewAngle() / 2f, false);

        Handles.DrawLine(playerDetection.transform.position, playerDetection.transform.position + viewAngleA * playerDetection.GetViewRadius());

        Handles.DrawLine(playerDetection.transform.position, playerDetection.transform.position + viewAngleB * playerDetection.GetViewRadius());
    }
}
