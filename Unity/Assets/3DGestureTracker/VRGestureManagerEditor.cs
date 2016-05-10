using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VRGestureManager))]
public class VRGestureManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VRGestureManager myScript = (VRGestureManager)target;
        if (GUILayout.Button("Save Changes"))
        {
            myScript.TestMe();
        }
    }
}