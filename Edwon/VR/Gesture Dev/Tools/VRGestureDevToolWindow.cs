#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Edwon.VR.Gesture
{
    public class VRGestureDevToolWindow : EditorWindow
    {

        public VRGestureDevTool devTool;
        public SerializedObject serializedObject;

        public const string RESOURCES_PATH = @"Assets/Edwon/VR/Gesture Dev/Resources/VR Infinite Gesture/";
        const string DEV_TOOL_PATH = RESOURCES_PATH + @"Dev/DevTool.asset";

        void OnGUI()
        {
            serializedObject.Update();

            GetSetDevTool();
            SetSerializedObject();

            GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth));
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            GUILayout.Space(20);

            GUILayout.Label("VR Infinite Gesture Dev Tool");

            GUILayout.Space(20);

            #region METHOD TESTS

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Examples To Dev"))
            {
                devTool.MoveExamples(MoveOption.ToDev);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Move Examples To Plugin"))
            {
                devTool.MoveExamples(MoveOption.ToPlugin);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Examples Nets To Dev"))
            {
                devTool.MoveExamplesNeuralNets(MoveOption.ToDev);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Move Examples Nets To Plugin"))
            {
                devTool.MoveExamplesNeuralNets(MoveOption.ToPlugin);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Integrations To Dev"))
            {
                devTool.MoveIntegrations(MoveOption.ToDev);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Move Integrations To Plugin"))
            {
                devTool.MoveIntegrations(MoveOption.ToPlugin);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Tutorials To Dev"))
            {
                devTool.MoveTutorials(MoveOption.ToDev);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Move Tutorials To Plugin"))
            {
                devTool.MoveTutorials(MoveOption.ToPlugin);
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Export Integrations Packages"))
            {
                devTool.ExportIntegrationsPackages();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("Export Examples Package"))
            {
                devTool.ExportExamplesPackages();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("Export Tutorials Package"))
            {
                devTool.ExportTutorialsPackages();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Delete Generated Packages"))
            {
                devTool.DeleteGeneratedPackages();
                AssetDatabase.Refresh();
            }

            #endregion

            GUILayout.Space(20);

            #region UNITY ASSET STORE SUBMIT PREP

            if (GUILayout.Button("PREP FOR SUBMISSION"))
            {
                devTool.PrepForSubmission();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("RESET AFTER SUBMISSION"))
            {
                devTool.ResetAfterSubmission();
            }
            #endregion

            GUILayout.Space(20);

            #region EXPORT PACKAGE
            GUILayout.Label("PLUGIN EXPORT PATH");
            GUILayout.Label("full windows path with a slash at the end");
            SerializedProperty GESTURE_PLUGIN_EXPORT_PATH = serializedObject.FindProperty("GESTURE_PLUGIN_EXPORT_PATH");
            GESTURE_PLUGIN_EXPORT_PATH.stringValue = EditorGUILayout.TextField(GESTURE_PLUGIN_EXPORT_PATH.stringValue);

            GUILayout.Space(10);

            GUILayout.Label("PLUGIN PACKAGE NAME");
            SerializedProperty GESTURE_PLUGIN_EXPORT_NAME = serializedObject.FindProperty("GESTURE_PLUGIN_EXPORT_NAME");
            GESTURE_PLUGIN_EXPORT_NAME.stringValue = EditorGUILayout.TextField(GESTURE_PLUGIN_EXPORT_NAME.stringValue);

            GUILayout.Space(10);

            if (GUILayout.Button("BUILD AND EXPORT PACKAGE"))
            {
                devTool.BuildAndExportPlugin();
            }
            #endregion

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

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
            string parentPath = Application.dataPath + RESOURCES_PATH.Substring(RESOURCES_PATH.IndexOf('/'));
            //Debug.Log("parentPath: " + parentPath);
            string devToolDirectoryPath = parentPath + "Dev/";
            Debug.Log(devToolDirectoryPath);
            if (!Directory.Exists(devToolDirectoryPath))
            {
                Directory.CreateDirectory(devToolDirectoryPath);
            }
            AssetDatabase.CreateAsset(instance, DEV_TOOL_PATH);
            return instance;
        }

        VRGestureDevTool GetDevTool()
        {
            return AssetDatabase.LoadAssetAtPath(DEV_TOOL_PATH, typeof(VRGestureDevTool)) as VRGestureDevTool;
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