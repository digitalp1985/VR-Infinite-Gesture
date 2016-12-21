#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public enum MoveOption { ToPlugin, ToDev }

    public class VRGestureDevTool : ScriptableObject
    {
        public string GESTURE_PLUGIN_EXPORT_PATH; // absolute path, don't forget to end with a slash
        public string GESTURE_PLUGIN_EXPORT_NAME; // the name of the plugin package when exported

        const string GESTURE_DEV_PATH = @"Assets/Edwon/VR/Gesture Dev/";
        const string GESTURE_PLUGIN_PATH = @"Assets/Edwon/VR/Gesture/";

        const string EXAMPLES_PATH = "Examples/";
        const string INTEGRATIONS_PATH = "Integrations/";

        const string PLAYMAKER_PACKAGE_NAME = "PlaymakerIntegration";

        public void BuildAndExportPlugin()
        {
            MoveIntegrations(MoveOption.ToPlugin);
            ExportIntegrationsPackages();
            MoveIntegrations(MoveOption.ToDev);
            ExportPlugin();
            DeleteGeneratedPackages();
        }

        void ExportPlugin()
        {
            string fromPath = 
                GESTURE_PLUGIN_PATH.Substring(0, GESTURE_PLUGIN_PATH.Length - 1);
            string exportPath = GESTURE_PLUGIN_EXPORT_PATH + GESTURE_PLUGIN_EXPORT_NAME + ".unitypackage";
            AssetDatabase.ExportPackage(fromPath, exportPath, ExportPackageOptions.Recurse);
            AssetDatabase.Refresh();
        }

        public void MoveExamples(MoveOption moveOption)
        {
            // first move playmaker folder from dev to normal
            // this way it will re-import to the correct spot when users re-import the package
            string examplesDev = GESTURE_DEV_PATH + EXAMPLES_PATH;
            string examplesPlugin = GESTURE_PLUGIN_PATH + EXAMPLES_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(examplesDev, examplesPlugin);
                    break;
                case MoveOption.ToDev:
                    MoveFolder(examplesPlugin, examplesDev);
                    break;
            }
        }

        public void MoveIntegrations(MoveOption moveOption)
        {
            // first move playmaker folder from dev to normal
            // this way it will re-import to the correct spot when users re-import the package
            string integrationsDev = GESTURE_DEV_PATH + INTEGRATIONS_PATH;
            string integrationsPlugin = GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(integrationsDev, integrationsPlugin);
                    break;
                case MoveOption.ToDev:
                    MoveFolder(integrationsPlugin, integrationsDev);
                    break;
            }
        }

        public void ExportIntegrationsPackages()
        {
            string fromPath = 
                GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH + "Playmaker";

            string exportPath =
                Application.dataPath + GESTURE_PLUGIN_PATH.Remove(0, 6)
                + INTEGRATIONS_PATH + PLAYMAKER_PACKAGE_NAME + ".unitypackage";

            ExportPackage(fromPath, exportPath);

            AssetDatabase.Refresh();
        }

        public void DeleteGeneratedPackages()
        {
            AssetDatabase.DeleteAsset(GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH + PLAYMAKER_PACKAGE_NAME + ".unitypackage");
            AssetDatabase.Refresh();
        }

        #region UTILS

        void MoveFolder(string from, string to)
        {
            FileUtil.MoveFileOrDirectory(from, to);
            AssetDatabase.Refresh();
        }

        void ExportPackage(string fromPath, string exportPath)
        {
            AssetDatabase.ExportPackage(
                fromPath,
                exportPath,
                ExportPackageOptions.Recurse);
            AssetDatabase.Refresh();
        }

        #endregion
    }
}

#endif