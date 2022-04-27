using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    private void OnSceneGUI()
    {
        MovingPlatform plat = (MovingPlatform)target;

        Handles.color = Color.black;
        Handles.DrawLine(plat.transform.position, plat.transform.position + plat.MaxPos);
    }
}