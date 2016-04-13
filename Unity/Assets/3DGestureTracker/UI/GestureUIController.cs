using UnityEngine;
using System.Collections;

public class GestureUIController : MonoBehaviour
{
    public VROptions.Handedness handedness;
    VRHandBase uiHand;
    Camera uiCam;
    public float offsetZ;

	void Start ()
    {
        // get vr player hand and camera
        uiHand = PlayerManager.GetPlayerHand(0, handedness);
        uiCam = PlayerManager.GetPlayerCamera(0);
	}
	
	void Update ()
    {
        Vector3 handToCamVector = uiCam.transform.position - uiHand.transform.position;
        transform.position = uiHand.transform.position + (offsetZ * handToCamVector);
        transform.rotation = Quaternion.LookRotation(transform.position - uiCam.transform.position);
    }

    // sends events to the gesture system ( LineCapture )

    public void BeginRecordMode ()
    {
        Debug.Log("begin record mode");
    }

    public void BeginDetectMode ()
    {
        Debug.Log("begin detect mode");
    }
}
