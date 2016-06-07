using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WinterMute
{
    public class VRGestureSettingsWindow : EditorWindow
    {
        static VRGestureSettingsWindow window;

        [MenuItem("Tools/Edwon VR Gesture/Settings")]

        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (VRGestureSettingsWindow)EditorWindow.GetWindow(typeof(VRGestureSettingsWindow));
            //window.name = "EdwonVR Gesture Settings";
            window.titleContent = new GUIContent("VR Gesture");
            window.Show();
        }

        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        void OnGUI()
        {
            GUILayout.Label("Edwon VR Gesture Tracker Settings", EditorStyles.boldLabel);

            // optional settings example
            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    }
}