#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Edwon.VR.Gesture
{
    public class VRGestureDevToolWindow : EditorWindow
    {

        public VRGestureDevTool devTool;
        public SerializedObject serializedObject;

        public const string RESOURCES_PATH = @"Assets/Edwon/VR/Gesture Dev/Resources/VR Infinite Gesture/";
        const string DEV_TOOL_PATH = RESOURCES_PATH + "Dev/DevTool.asset";

        void OnGUI()
        {
            serializedObject.Update();

            GetSetDevTool();
            SetSerializedObject();

            GUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth));

            GUILayout.Space(5);

            // put GUI stuff here

            if (GUILayout.Button("Check Neural Net Folder"))
            {
                devTool.CheckCreateNeuralNetFolder();
            }


            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        #region SCRIPTABLE OBJECT ASSET LINK MANAGEMENT

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

        void OnEnable()
        {
            GetSetDevTool();
            SetSerializedObject();
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
            string fullPath = DEV_TOOL_PATH;
            AssetDatabase.CreateAsset(instance, fullPath);
            return instance;
        }


        VRGestureDevTool GetDevTool()
        {
            string fullPath = DEV_TOOL_PATH;
            return AssetDatabase.LoadAssetAtPath(fullPath, typeof(VRGestureDevTool)) as VRGestureDevTool;
        }

        void SetSerializedObject()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(devTool);
            }
        }

        #endregion
    }
}

#endif