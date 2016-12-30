#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class GettingStartedTutorialInitializeOnLoad
{
    static string sceneName = null;
    static string gettingStartedTutorialSceneName = "Getting Started";

    static GettingStartedTutorialInitializeOnLoad()
    {
        EditorApplication.update += EditorUpdate;
        //EditorSceneManager.activeSceneChanged += ActiveSceneChanged;
    }

    //static void ActiveSceneChanged(Scene previousScene, Scene newScene)
    //{
    //    Debug.Log("scene changed to: " + newScene.name);
    //}

    static void EditorUpdate()
    {
        if (sceneName != EditorSceneManager.GetActiveScene().name)
        {
            // New scene has been loaded
            sceneName = EditorSceneManager.GetActiveScene().name;
            if (sceneName == gettingStartedTutorialSceneName)
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.Log("PRESS PLAY TO BEGIN THE TUTORIAL!");
                    Debug.Log("if the scene starts in VR, reset the scene until it's 2D");
                    Debug.Log("make sure the 'Game' window is big enough to see the back and next buttons");
                }
            }
        }
    }
}

#endif