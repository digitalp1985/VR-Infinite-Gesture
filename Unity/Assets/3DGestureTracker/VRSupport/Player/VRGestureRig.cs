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

    GameObject leftController;
    GameObject rightController;

    IInput inputLeft = null;
    IInput inputRight = null;

    // Use this for initialization
    void Start () {

        
    }

    void Awake()
    {
        SteamVR_ControllerManager[] steamVR_cm = FindObjectsOfType<SteamVR_ControllerManager>();
        //SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost;
        leftController = steamVR_cm[0].left;
        rightController = steamVR_cm[0].right;
        CreateInputHelper();
    }

    void LateUpdate()
    {

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
        if (Config.vrType == Config.VRTYPE.SteamVR)
        {
            //inputLeft = new VRControllerInputSteam(HandType.Left);
            //inputRight = new VRControllerInputSteam(HandType.Right);
        }
        else if (Config.vrType == Config.VRTYPE.OculusTouchVR)
        {
            //controllerInput = new VRControllerInputOculus();
            //controllerInput = hand.gameObject.AddComponent<VRControllerInputOculus>();
        }
        else
        {
            //inputLeft = new VRControllerInputSteam(HandType.Left);
            //inputRight = new VRControllerInputSteam(HandType.Right);
            inputLeft = leftController.gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Left);
            inputRight = rightController.gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Right);
        }
    }
}
