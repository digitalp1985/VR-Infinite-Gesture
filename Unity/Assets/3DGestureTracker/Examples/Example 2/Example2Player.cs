using UnityEngine;
using System.Collections;
using WinterMute;

public class Example2Player : MonoBehaviour
{

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}

    void OnEnable ()
    {
        VRGestureManager.GestureDetectedEvent += OnGestureDetected;
    }

    void OnDisable ()
    {
        VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
    }

    void OnGestureDetected (string gestureName, double confidence)
    {
        string confidenceString = confidence.ToString().Substring(0, 4);
        Debug.Log("detected gesture: " + gestureName + " with confidence: " + confidenceString);
    }
}
