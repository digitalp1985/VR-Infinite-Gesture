using UnityEngine;
using System.Collections;
using System;
using Edwon.VR.Gesture;
using Edwon.VR.Input;

namespace Edwon.VR
{
    public class VRGestureRig : MonoBehaviour
    {

        //public VRRigAnchors vrRigAnchors;
        public Transform head;
        public Transform handLeft;
        public Transform handRight;

        GameObject leftController;
        GameObject rightController;

        public bool spawnControllerModels = false;

        public GameObject leftControllerModelPrefab;
        public GameObject rightControllerModelPrefab;

        IInput inputLeft = null;
        IInput inputRight = null;

        void Awake()
        {
            CreateInputHelper();
        }

        public void SetupRig()
        {
            Debug.Log("setup rig");

        
        }

        public Transform GetHand(HandType handedness)
        {
            if (handedness == HandType.Left)
            {
                return handLeft;
            }
            else
            {
                return handRight;
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
            if (VRGestureManager.Instance.vrType == VRTYPE.SteamVR)
            {
                
                #if EDWON_VR_STEAM
                SteamVR_ControllerManager[] steamVR_cm = FindObjectsOfType<SteamVR_ControllerManager>();
                leftController = steamVR_cm[0].left;
                rightController = steamVR_cm[0].right;

                inputLeft = gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Left, leftController);
                inputRight = gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Right, rightController);
				#endif

            }
            else if (VRGestureManager.Instance.vrType == VRTYPE.OculusTouchVR)
            {
				#if EDWON_VR_OCULUS
                inputLeft = handLeft.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Left);
                inputRight = handRight.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Right);
                if (spawnControllerModels)
                    SpawnControllerModels();
				#endif
            }
            else
            {
                Debug.Log("YOU NEED TO SELECT A BETTER VRTYPE in your config.");
            }
        }

        public void SpawnControllerModels ()
        {
            Transform leftModel = GameObject.Instantiate(leftControllerModelPrefab).transform;
            Transform rightModel = GameObject.Instantiate(rightControllerModelPrefab).transform;
            leftModel.parent = handLeft;
            rightModel.parent = handRight;
            leftModel.localPosition = Vector3.zero;
            rightModel.localPosition = Vector3.zero;
            leftModel.localRotation = Quaternion.identity;
            rightModel.localRotation = Quaternion.identity;
        }
    }
}