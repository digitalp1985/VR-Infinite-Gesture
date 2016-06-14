using UnityEngine;
using System.Collections;
using System;

namespace WinterMute
{
    public class VRGestureRig : MonoBehaviour
    {

        //public VRRigAnchors vrRigAnchors;
        public Transform headTF;
        public Transform cameraEyeTransform;
        public Transform lHandTF;
        public Transform rHandTF;

        GameObject leftController;
        GameObject rightController;

        IInput inputLeft = null;
        IInput inputRight = null;

        // Use this for initialization
        void Start()
        {


        }

        void Awake()
        {
            CreateInputHelper();
        }

        void LateUpdate()
        {

        }

        public Transform GetHand(HandType handedness)
        {
            if (handedness == HandType.Left)
            {
                return lHandTF;
            }
            else
            {
                return rHandTF;
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
                SteamVR_ControllerManager[] steamVR_cm = FindObjectsOfType<SteamVR_ControllerManager>();
                leftController = steamVR_cm[0].left;
                rightController = steamVR_cm[0].right;
                inputLeft = leftController.gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Left);
                inputRight = rightController.gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Right);

            }
            else if (Config.vrType == Config.VRTYPE.OculusTouchVR)
            {
                inputLeft = lHandTF.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Left);
                inputRight = rHandTF.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Right);
            }
            else
            {
                Debug.Log("YOU NEED TO SELECT A BETTER VRTYPE in your config.");
            }
        }
    }
}