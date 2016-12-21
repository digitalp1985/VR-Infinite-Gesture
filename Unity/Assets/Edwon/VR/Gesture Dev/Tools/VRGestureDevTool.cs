using UnityEngine;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class VRGestureDevTool : ScriptableObject
    {
        public void CheckCreateNeuralNetFolder()
        {
            Utils.CheckCreateNeuralNetFolder();
        }
    }
}