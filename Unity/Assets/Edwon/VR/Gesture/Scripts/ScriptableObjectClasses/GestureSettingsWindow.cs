using UnityEngine;
using UnityEditor;
using System.Collections;

public class GestureSettingsWindow : EditorWindow
{
    public GestureSettings settings;

    [MenuItem("Tools/VR Infinite Gesture/Settings")]
    public static void Init ()
    {
        // get exisiting open window or if none make one
        GestureSettingsWindow window = (
            GestureSettingsWindow)EditorWindow.GetWindowWithRect(typeof(GestureSettingsWindow), 
            new Rect(50, 50, 800, 500)
            );
    }

    void OnEnable ()
    {
        hideFlags = HideFlags.HideAndDontSave;
        if (AssetDatabase.LoadAssetAtPath("Assets/Inventory/InventoryManager.asset", typeof(GestureSettings)) == null)
        {
            settings = CreateInstance<GestureSettings>();
        }
        else {
            settings = (GestureSettings)AssetDatabase.LoadAssetAtPath("Assets/Inventory/InventoryManager.asset", typeof(GestureSettings));
        }
    }
}
