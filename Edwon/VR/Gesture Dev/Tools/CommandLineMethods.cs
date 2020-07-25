#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public class CommandLineMethods : MonoBehaviour
{
    // called by the project opener .cmd script
    // opens the script editor automatically
    static void OpenScriptEditor()
    {
        EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
    }

}
#endif