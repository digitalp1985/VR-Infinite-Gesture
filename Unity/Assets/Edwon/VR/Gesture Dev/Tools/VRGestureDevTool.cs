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

        const string PLAYMAKER_FOLDER_NAME = "Playmaker/";
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
            string examplesDev = GESTURE_DEV_PATH + EXAMPLES_PATH;
            string examplesPlugin = GESTURE_PLUGIN_PATH + EXAMPLES_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(examplesDev + "Example 1/", examplesPlugin + "Example 1");
                    MoveFolder(examplesDev + "Example 2/", examplesPlugin + "Example 2");
                    if (System.IO.Directory.Exists(examplesDev))
                        System.IO.Directory.Delete(examplesDev);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(examplesDev))
                        System.IO.Directory.CreateDirectory(examplesDev);
                    MoveFolder(examplesPlugin + "Example 1/", examplesDev + "Example 1");
                    MoveFolder(examplesPlugin + "Example 2/", examplesDev + "Example 2");
                    break;
            }
        }

        public void MoveIntegrations(MoveOption moveOption)
        {
            string integrationsDev = GESTURE_DEV_PATH + INTEGRATIONS_PATH;
            string integrationsPlugin = GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(integrationsDev + PLAYMAKER_FOLDER_NAME, integrationsPlugin + PLAYMAKER_FOLDER_NAME);
                    if (System.IO.Directory.Exists(integrationsDev))
                        System.IO.Directory.Delete(integrationsDev);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(integrationsDev))
                        System.IO.Directory.CreateDirectory(integrationsDev);
                    MoveFolder(integrationsPlugin + PLAYMAKER_FOLDER_NAME, integrationsDev + PLAYMAKER_FOLDER_NAME);
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