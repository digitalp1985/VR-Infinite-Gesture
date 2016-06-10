using UnityEngine;
using System.Collections;
using WinterMute;
using System;

public class VRGestureRig : MonoBehaviour {

    //public VRRigAnchors vrRigAnchors;
    public Transform headTF;
    public Transform cameraEyeTransform;
    public Transform lHandTF;
    public Rigidbody lHandRB;
    public Transform rHandTF;
    public Rigidbody rHandRB;

    IInput inputLeft;
    IInput inputRight;

    // Use this for initialization
    void Start () {
        //CreateInputHelper();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public Transform GetHand(HandType handedness)
    {
        if(handedness == HandType.Left)
        {
            return lHandTF;
        }
        else
        {
            return rHandTF;
        }
    }

    internal Rigidbody GetHandRB(HandType handedness)
    {
        if (handedness == HandType.Right)
        {
            return rHandRB;
        }
        else
        {
            return lHandRB;
        }
    }

    //These are still expecting a proper HAND.
    //They will not find them because Inputs are not connected to HANDS.
    //These are now part of VR GestureManager maybe.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="handedness"></param>
    /// <returns></returns>
    public IInput GetInput(HandType handedness)
    {
        if(inputLeft == null && inputRight == null)
        {
            CreateInputHelper();
        }

        if (handedness == HandType.Left)
        {
            return inputLeft;
        }
        else
        {
            return inputRight;
        }
    }

    /// <summary>
    /// This will check to see if an IInput interface is attached to the controller.
    /// If not it will attach the default VRControllerInput from EdwonVR
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    public void CreateInputHelper()
    {
        if (VROptions.vrType == VROptions.VRTYPE.SteamVR)
        {
            inputLeft = new VRControllerInputSteam(HandType.Left);
            inputRight = new VRControllerInputSteam(HandType.Right);
        }
        else if (VROptions.vrType == VROptions.VRTYPE.OculusTouchVR)
        {
            //controllerInput = new VRControllerInputOculus();
            //controllerInput = hand.gameObject.AddComponent<VRControllerInputOculus>();
        }
    }
}
