using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class GestureSettingsWindow : EditorWindow
    {
        public GestureSettings gestureSettings;

        [MenuItem("Tools/VR Infinite Gesture/Settings")]
        public static void Init()
        {
            // get exisiting open window or if none make one
            GestureSettingsWindow window = (
                GestureSettingsWindow)EditorWindow.GetWindowWithRect(typeof(GestureSettingsWindow),
                new Rect(50, 50, 800, 500)
                );
        }

        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            if (Utils.GetGestureSettings() == null)
            {
                Debug.Log("no gesture settings asset, please make it or write a thing that does it automatically for people");
                //gestureSettings = CreateInstance<GestureSettings>();
            }
            else
            {
                gestureSettings = Utils.GetGestureSettings();
            }
        }

        void OnGUI ()
        {
            
        }
    }
}