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
        const string STREAMING_ASSETS_PATH = @"Assets/StreamingAssets";

        const string EXAMPLES_PATH = "Examples/";
        const string EXAMPLES_NETS_PATH = "Example Neural Nets/";
        const string INTEGRATIONS_PATH = "Integrations/";
        const string TUTORIALS_PATH = "Tutorials/";

        const string PLAYMAKER_FOLDER_NAME = "Playmaker/";
        const string PLAYMAKER_PACKAGE_NAME = "PlaymakerIntegration";
        const string EXAMPLES_PACKAGE_NAME = "Examples";
        const string TUTORIALS_PACKAGE_NAME = "Tutorials";

        const string EXAMPLE_1_NAME = "Example 1 - Basics";
        const string EXAMPLE_2_NAME = "Example 2 - Powers";
        const string EXAMPLE_3_NAME = "Example 3 - Custom Trail";

        public void BuildAndExportPlugin()
        {
            PrepForSubmission();

            ExportPlugin();

            ResetAfterSubmission();
        }

        void ExportPlugin()
        {
            string fromPath = 
                GESTURE_PLUGIN_PATH.Substring(0, GESTURE_PLUGIN_PATH.Length - 1);
            string exportPath = GESTURE_PLUGIN_EXPORT_PATH + GESTURE_PLUGIN_EXPORT_NAME + ".unitypackage";
            AssetDatabase.ExportPackage(fromPath, exportPath, ExportPackageOptions.Recurse);
        }

        public void PrepForSubmission()
        {
            // move examples neural nets first
            MoveExamplesNeuralNets(MoveOption.ToPlugin);

            AssetDatabase.Refresh();

            // export packages
            ExportIntegrationsPackages();
            ExportExamplesPackages();
            ExportTutorialsPackages();

            AssetDatabase.Refresh();

            // move all the stuff that was in the packages to dev
            MoveIntegrations(MoveOption.ToDev);
            MoveExamples(MoveOption.ToDev);
            MoveTutorials(MoveOption.ToDev);

            AssetDatabase.Refresh();
        }

        public void ResetAfterSubmission()
        {
            AssetDatabase.Refresh();

            // delete the generated packages
            DeleteGeneratedPackages();

            AssetDatabase.Refresh();

            // move everything back to where it was
            MoveExamplesNeuralNets(MoveOption.ToDev);
            MoveIntegrations(MoveOption.ToPlugin);
            MoveExamples(MoveOption.ToPlugin);
            MoveTutorials(MoveOption.ToPlugin);

            AssetDatabase.Refresh();
        }

        public void MoveIntegrations(MoveOption moveOption)
        {
            string integrationsDev = GESTURE_DEV_PATH + INTEGRATIONS_PATH;
            string integrationsPlugin = GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(integrationsDev + PLAYMAKER_FOLDER_NAME, integrationsPlugin + PLAYMAKER_FOLDER_NAME);
                    //if (System.IO.Directory.Exists(integrationsDev))
                    //    System.IO.Directory.Delete(integrationsDev);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(integrationsDev))
                        System.IO.Directory.CreateDirectory(integrationsDev);
                    MoveFolder(integrationsPlugin + PLAYMAKER_FOLDER_NAME, integrationsDev + PLAYMAKER_FOLDER_NAME);
                    break;
            }
        }

        public void MoveExamples(MoveOption moveOption)
        {
            string examplesDev = GESTURE_DEV_PATH + EXAMPLES_PATH;
            string examplesPlugin = GESTURE_PLUGIN_PATH + EXAMPLES_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(examplesDev + EXAMPLE_1_NAME + "/", examplesPlugin + EXAMPLE_1_NAME);
                    MoveFolder(examplesDev + EXAMPLE_2_NAME + "/", examplesPlugin + EXAMPLE_2_NAME);
                    //if (System.IO.Directory.Exists(examplesDev))
                    //    System.IO.Directory.Delete(examplesDev);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(examplesDev))
                        System.IO.Directory.CreateDirectory(examplesDev);
                    MoveFolder(examplesPlugin + EXAMPLE_1_NAME + "/", examplesDev + EXAMPLE_1_NAME);
                    MoveFolder(examplesPlugin + EXAMPLE_2_NAME + "/", examplesDev + EXAMPLE_2_NAME);
                    break;
            }
        }

        public void MoveExamplesNeuralNets(MoveOption moveOption)
        {
            // moves the neural nets examples to the streaming assets folder 
            // instead of the normal plugin path

            string examplesNetsDev = GESTURE_DEV_PATH + EXAMPLES_NETS_PATH;
            string examplesNetsPlugin = STREAMING_ASSETS_PATH + Config.NEURAL_NET_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(examplesNetsDev + EXAMPLE_1_NAME + "/", examplesNetsPlugin + EXAMPLE_1_NAME);
                    MoveFolder(examplesNetsDev + EXAMPLE_2_NAME + "/", examplesNetsPlugin + EXAMPLE_2_NAME);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(examplesNetsDev))
                        System.IO.Directory.CreateDirectory(examplesNetsDev);
                    MoveFolder(examplesNetsPlugin + EXAMPLE_1_NAME + "/", examplesNetsDev + EXAMPLE_1_NAME);
                    MoveFolder(examplesNetsPlugin + EXAMPLE_2_NAME + "/", examplesNetsDev + EXAMPLE_2_NAME);
                    break;
            }
        }

        public void MoveTutorials(MoveOption moveOption)
        {
            string tutorialsDev = GESTURE_DEV_PATH + TUTORIALS_PATH;
            string tutorialsPlugin = GESTURE_PLUGIN_PATH + TUTORIALS_PATH;
            switch (moveOption)
            {
                case MoveOption.ToPlugin:
                    MoveFolder(tutorialsDev + "Getting Started/", tutorialsPlugin + "Getting Started/");
                    //if (System.IO.Directory.Exists(tutorialsDev))
                    //    System.IO.Directory.Delete(tutorialsDev);
                    break;
                case MoveOption.ToDev:
                    if (!System.IO.Directory.Exists(tutorialsDev))
                        System.IO.Directory.CreateDirectory(tutorialsDev);
                    MoveFolder(tutorialsPlugin + "Getting Started/", tutorialsDev + "Getting Started/");
                    break;
            }
        }

        public void ExportIntegrationsPackages()
        {
            string fromPath = 
                GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH + "Playmaker";

            string exportPath =
                Application.dataPath + GESTURE_PLUGIN_PATH.Remove(0, 6)
                + INTEGRATIONS_PATH + PLAYMAKER_PACKAGE_NAME;

            ExportPackage(fromPath, exportPath);
        }

        public void ExportExamplesPackages()
        {
            // export the example scenes and assets, 
            // as well as the neural nets from the streaming assets folder

            string examplesFromPath =
                GESTURE_PLUGIN_PATH + EXAMPLES_PATH.Substring(0, EXAMPLES_PATH.Length - 1);

            string streamingAssetsNetsPath = STREAMING_ASSETS_PATH + Config.NEURAL_NET_PATH;
            string example1NetFromPath = streamingAssetsNetsPath + "Example 1 - Basics";
            string example2NetFromPath = streamingAssetsNetsPath + "Example 2 - Powers";

            string exportPath =
                Application.dataPath + GESTURE_PLUGIN_PATH.Remove(0, 6)
                + EXAMPLES_PATH + EXAMPLES_PACKAGE_NAME;

            string[] fromPaths = new string[] {examplesFromPath, example1NetFromPath, example2NetFromPath};

            AssetDatabase.ExportPackage(
            fromPaths,
            exportPath + ".unitypackage",
            ExportPackageOptions.Recurse);
            //AssetDatabase.Refresh();
        }

        public void ExportTutorialsPackages()
        {
            string fromPath =
                GESTURE_PLUGIN_PATH + TUTORIALS_PATH.Substring(0, TUTORIALS_PATH.Length - 1);

            string exportPath =
                Application.dataPath + GESTURE_PLUGIN_PATH.Remove(0, 6)
                + TUTORIALS_PATH + TUTORIALS_PACKAGE_NAME;

            ExportPackage(fromPath, exportPath);
        }

        public void DeleteGeneratedPackages()
        {
            AssetDatabase.DeleteAsset(GESTURE_PLUGIN_PATH + INTEGRATIONS_PATH + PLAYMAKER_PACKAGE_NAME + ".unitypackage");
            AssetDatabase.DeleteAsset(GESTURE_PLUGIN_PATH + EXAMPLES_PATH + EXAMPLES_PACKAGE_NAME + ".unitypackage");
            AssetDatabase.DeleteAsset(GESTURE_PLUGIN_PATH + TUTORIALS_PATH + TUTORIALS_PACKAGE_NAME + ".unitypackage");
            //AssetDatabase.Refresh();
        }

        #region UTILS

        void MoveFolder(string from, string to)
        {
            FileUtil.MoveFileOrDirectory(from, to);
            //AssetDatabase.Refresh();
        }

        void ExportPackage(string fromPath, string exportPath)
        {
            AssetDatabase.ExportPackage(
                fromPath,
                exportPath + ".unitypackage",
                ExportPackageOptions.Recurse);
            //AssetDatabase.Refresh();
        }

        #endregion
    }
}

#endif