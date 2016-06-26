using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR
{
    [CustomEditor(typeof(VRGestureRig))]
    public class VRGestureRigEditor : Editor
    {
        SerializedProperty head;
        SerializedProperty handLeft;
        SerializedProperty handRight;

        void OnEnable()
        {
            head = serializedObject.FindProperty("head");
            handLeft = serializedObject.FindProperty("handLeft");
            handRight = serializedObject.FindProperty("handRight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            VRGestureRig vrGestureRig = (VRGestureRig)target;
            if (GUILayout.Button("Auto Setup"))
            {
                vrGestureRig.SetupRig();
            }

            EditorGUILayout.PropertyField(head);
            EditorGUILayout.PropertyField(handLeft);
            EditorGUILayout.PropertyField(handRight);

            serializedObject.ApplyModifiedProperties();
            //DrawDefaultInspector();
        }
    }
}