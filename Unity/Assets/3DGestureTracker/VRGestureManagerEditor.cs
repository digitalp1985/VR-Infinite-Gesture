using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VRGestureManager))]
public class VRGestureManagerEditor : Editor
{
	string editGesturesButtonText;
	bool editGestures = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		editGesturesButtonText = editGestures ? "Edit Gestures" : editGesturesButtonText = "Save Gestures";

        VRGestureManager script = (VRGestureManager)target;
		if (GUILayout.Button(editGesturesButtonText))
        {
			editGestures = !editGestures;
//            script.TestMe();
        }
    }
}