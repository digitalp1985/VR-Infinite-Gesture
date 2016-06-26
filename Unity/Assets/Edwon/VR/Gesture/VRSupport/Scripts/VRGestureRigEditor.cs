using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR
{
    [CustomEditor(typeof(VRGestureRig))]
    public class VRGestureRigEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            VRGestureRig vrGestureRig = (VRGestureRig)target;
            if (GUILayout.Button("Auto Setup"))
            {
                vrGestureRig.SetupRig();
            }
            DrawDefaultInspector();
        }
    }
}