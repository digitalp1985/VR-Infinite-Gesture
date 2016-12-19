using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [CustomEditor(typeof(Tutorial))]
    [CanEditMultipleObjects]
    public class TutorialEditor : Editor
    {
        Tutorial tutorial;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //DrawDefaultInspector();
            tutorial = (Tutorial)target;

            EditorGUILayout.EnumPopup(tutorial.TutorialSettings.tutorialState);
            EditorGUILayout.IntField(tutorial.TutorialSettings.currentTutorialStep);

            if (GUILayout.Button("Restart Tutorial"))
            {
                tutorial.OnRestartTutorial();
            }

            if (GUILayout.Button("Next Step"))
            {
                tutorial.OnButtonNext();
            }

            if (GUILayout.Button("Previous Step"))
            {
                tutorial.OnButtonBack();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
