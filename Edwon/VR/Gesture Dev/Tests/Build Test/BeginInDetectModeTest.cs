using UnityEngine;
using System.Collections;
using Edwon.VR.Gesture;

public class BeginInDetectModeTest : MonoBehaviour
{
    void OnEnable()
    {
        GestureRecognizer.GestureDetectedEvent += OnGestureDetectedEvent;
    }

    void OnDisable()
    {
        GestureRecognizer.GestureDetectedEvent -= OnGestureDetectedEvent;
    }

    void OnGestureDetectedEvent(string gestureName, double confidence, Edwon.VR.Handedness handedness, bool isDouble)
    {
        if (gestureName == "Gesture 1")
        {
            Debug.Log("gesture 1");
        }
        if (gestureName == "Gesture 2")
        {
            Debug.Log("gesture 2");
        }

    }

}
