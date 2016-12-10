using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
public class TutorialSettings : ScriptableObject
{
    const string SETTINGS_ASSET_PATH = @"Assets/Edwon/VR/Gesture/Settings/TutorialSettings.asset";

    public int currentTutorialStep;

    public static TutorialSettings instance;

    public static TutorialSettings CreateTutorialSettingsAsset()
    {
        if (instance == null)
        {
            instance = CreateInstance<TutorialSettings>();
            AssetDatabase.CreateAsset(instance, SETTINGS_ASSET_PATH);
        }
        return instance;
    }

}
