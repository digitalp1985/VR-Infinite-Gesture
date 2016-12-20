#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TutorialInitializeOnLoad
{
    static TutorialInitializeOnLoad()
    {
        Debug.Log("Autorun!");
    }
}

#endif