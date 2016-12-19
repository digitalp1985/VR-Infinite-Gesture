#if UNITY_EDITOR

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
        SerializedProperty handLeftModel;
        SerializedProperty handRightModel;
        SerializedProperty gestureHand;
        SerializedProperty gestureButton;
        SerializedProperty displayGestureTrail;
        SerializedProperty useCustomControllerModels;
        SerializedProperty playerID;

        void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            head = serializedObject.FindProperty("head");
            handLeft = serializedObject.FindProperty("handLeft");
            handRight = serializedObject.FindProperty("handRight");
            handLeftModel = serializedObject.FindProperty("handLeftModel");
            handRightModel = serializedObject.FindProperty("handRightModel");
            gestureHand = serializedObject.FindProperty("mainHand");
            gestureButton = serializedObject.FindProperty("gestureButton");
            displayGestureTrail = serializedObject.FindProperty("displayGestureTrail");
            useCustomControllerModels = serializedObject.FindProperty("useCustomControllerModels");
            playerID = serializedObject.FindProperty("playerID");

            VRGestureRig vrGestureRig = (VRGestureRig)target;

            #if EDWON_VR_OCULUS || EDWON_VR_STEAM
            if (GUILayout.Button("Auto Setup"))
            {
                vrGestureRig.AutoSetup();
            }
            #endif

            EditorGUILayout.PropertyField(head);
            EditorGUILayout.PropertyField(handLeft);
            EditorGUILayout.PropertyField(handRight);

            EditorGUILayout.PropertyField(gestureHand);
            EditorGUILayout.PropertyField(gestureButton);
            EditorGUILayout.PropertyField(playerID);

            EditorGUILayout.PropertyField(displayGestureTrail);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Spawn Controller Models");
            vrGestureRig.spawnControllerModels = EditorGUILayout.Toggle(vrGestureRig.spawnControllerModels);
            EditorGUILayout.EndHorizontal();

            if (vrGestureRig.spawnControllerModels)
            {
                EditorGUILayout.PropertyField(useCustomControllerModels);
                if (vrGestureRig.useCustomControllerModels)
                {
                    EditorGUILayout.PropertyField(handLeftModel);
                    EditorGUILayout.PropertyField(handRightModel);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif