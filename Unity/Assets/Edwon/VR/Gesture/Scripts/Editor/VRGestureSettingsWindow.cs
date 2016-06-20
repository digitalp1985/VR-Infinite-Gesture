using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class VRGestureSettingsWindow : EditorWindow
    {

        static VRGestureSettingsWindow window;
        const float spaceSize = 10f;

        [MenuItem("Tools/Edwon VR Gesture/Settings")]

        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (VRGestureSettingsWindow)EditorWindow.GetWindow(typeof(VRGestureSettingsWindow));
            //window.name = "EdwonVR Gesture Settings";
            window.titleContent = new GUIContent("VR Gesture");
            window.Show();
        }

        bool folderPathEditingEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        void OnGUI()
        {
            GUILayout.Label("Edwon VR Gesture Tracker Settings", EditorStyles.boldLabel);
            GUILayout.Space(spaceSize);

            folderPathEditingEnabled = EditorGUILayout.BeginToggleGroup("Edit Data Folder", folderPathEditingEnabled);
            GUILayout.Label("the folder to save gesture and neural net data \nbe careful changing this");
            //Config.SAVE_FILE_PATH = GUILayout.TextField(Config.SAVE_FILE_PATH);
            EditorGUILayout.EndToggleGroup();
            GUILayout.Space(spaceSize);

            GUILayout.Label("use raw data when recording gestures, this does... blah blah blah");
            //Config.USE_RAW_DATA = GUILayout.Toggle(Config.USE_RAW_DATA, " use raw data");
            GUILayout.Space(spaceSize);

            GUILayout.Label("use continious tracking of gestures");
            //Config.CONTINIOUS = GUILayout.Toggle(Config.CONTINIOUS, " Continious Tracking");
            GUILayout.Space(spaceSize);

            GUILayout.Label("how many points to examine from any given gesture recording");
            //Config.FIDELITY = EditorGUILayout.IntField(" Fidelity", Config.FIDELITY);
            GUILayout.Space(spaceSize);

            GUILayout.Label("this does something i don't know yet");
            //Config.CAPTURE_RATE = EditorGUILayout.IntField(" Capture Rate", Config.CAPTURE_RATE);
            GUILayout.Space(spaceSize);

            // optional settings example
            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    }
}