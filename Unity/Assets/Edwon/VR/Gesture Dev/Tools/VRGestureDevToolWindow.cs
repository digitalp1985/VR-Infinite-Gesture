#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Edwon.VR.Gesture
{
    public class VRGestureDevToolWindow : ScriptableObjectWindow
    {

        public VRGestureDevTool devTool;
        public SerializedObject serializedObject;

        const string DEV_TOOL_PATH = Config.PARENT_PATH + "Dev/DevTool";

        [MenuItem("Tools/VR Infinite Gesture/Dev Tool")]
        public static void Init()
        {
            InitWindow();
        }

        static void InitWindow()
        {
            // get exisiting open window or if none make one
            VRGestureDevToolWindow window = 
                (VRGestureDevToolWindow)EditorWindow.GetWindow<VRGestureDevToolWindow>(
                    false,
                    "VR Gesture Dev",
                    true);
        }

        void GetSetDevTool()
        {
            if (GetDevTool() == null)
            {
                devTool = CreateDevToolAsset();
            }
            else
            {
                devTool = GetDevTool();
            }
        }

        VRGestureDevTool CreateDevToolAsset()
        {
            VRGestureDevTool instance = CreateInstance<VRGestureDevTool>();
            string fullPath = Application.streamingAssetsPath + DEV_TOOL_PATH;
            AssetDatabase.CreateAsset(instance, fullPath);
            return instance;
        }


        VRGestureDevTool GetDevTool()
        {
            string fullPath = Application.streamingAssetsPath + DEV_TOOL_PATH;
            return AssetDatabase.LoadAssetAtPath(fullPath, typeof(VRGestureDevTool)) as VRGestureDevTool;
        }

    }
}

#endif