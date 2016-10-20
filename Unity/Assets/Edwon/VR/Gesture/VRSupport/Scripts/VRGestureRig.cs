using UnityEngine;
using System.Collections;
using System;
using Edwon.VR.Gesture;
using Edwon.VR.Input;
using System.Collections.Generic;
using UnityEditor;

namespace Edwon.VR
{
    public class VRGestureRig : MonoBehaviour
    {
        public HandType gestureHand = HandType.Right;
        public InputOptions.Button gestureButton = InputOptions.Button.Trigger1;
        public VRGestureManagerState state = VRGestureManagerState.Idle;
        public VRGestureManagerState stateLast;
        public bool displayGestureTrail;
        public int ID = 0;

        GestureSettings gestureSettings;

        //public VRRigAnchors vrRigAnchors;
        [SerializeField]
        public Transform head;
        [SerializeField]
        public Transform handLeft;
        [SerializeField]
        public Transform handRight;

        [SerializeField]
        GameObject leftController;
        [SerializeField]
        GameObject rightController;

        [SerializeField]
        public bool spawnControllerModels = false;

        [SerializeField]
        public GameObject handLeftModel;
        [SerializeField]
        public GameObject handRightModel;

        IInput inputLeft = null;
        IInput inputRight = null;

        Transform perpTransform;
        public CaptureHand leftCapture;
        public CaptureHand rightCapture;


        //current NeuralNetwork
        //current Recognizer?
        public GestureRecognizer currentRecognizer;
        public Trainer currentTrainer;
        //current Trainer?


        public static VRGestureRig GetPlayerRig(int rigID = -1)
        {
            VRGestureRig rig = null;

            VRGestureRig[] rigs = FindObjectsOfType(typeof(VRGestureRig)) as VRGestureRig[];
            foreach (VRGestureRig _rig in rigs)
            {
                if(_rig.ID == rigID)
                {
                    rig = _rig;
                }
            }

            //if (FindObjectOfType<VRGestureRig>() != null)
            //{
            //    rig = FindObjectOfType<VRGestureRig>();
            //}
            return rig;
        }

        void Awake()
        {
            gestureSettings = AssetDatabase.LoadAssetAtPath("Assets/Edwon/VR/Gesture/Settings/Settings.asset", typeof(GestureSettings)) as GestureSettings;

            CreateInputHelper();
            
        }


        #region INITIALIZATION
        void Init()
        {
            //maybe only init this if it does not exist.
            //Remove all game objects
            perpTransform = transform.Find("Perpindicular Head");
            if (perpTransform == null)
            {
                perpTransform = new GameObject("Perpindicular Head").transform;
                perpTransform.parent = this.transform;
            }
            GestureTrail leftTrail = null;
            GestureTrail rightTrail = null;

            if (displayGestureTrail)
            {
                leftTrail = gameObject.AddComponent<GestureTrail>();
                rightTrail = gameObject.AddComponent<GestureTrail>();
            }
            leftCapture = new CaptureHand(this, perpTransform, HandType.Left, leftTrail);
            rightCapture = new CaptureHand(this, perpTransform, HandType.Right, rightTrail);
        }

        void OnEnable()
        {
            Init();
            if (leftCapture != null && rightCapture != null)
            {
                SubscribeToEvents();
            }
        }

        void SubscribeToEvents()
        {
            leftCapture.StartCaptureEvent += StartCapturing;
            leftCapture.StopCaptureEvent += StopCapturing;
            rightCapture.StartCaptureEvent += StartCapturing;
            rightCapture.StopCaptureEvent += StopCapturing;
        }

        void OnDisable()
        {
            leftCapture.StartCaptureEvent -= StartCapturing;
            leftCapture.StopCaptureEvent -= StopCapturing;
            rightCapture.StartCaptureEvent -= StartCapturing;
            rightCapture.StopCaptureEvent -= StopCapturing;
        }
        #endregion

        #region UPDATE
        void Update()
        {
            if (leftCapture != null)
            {
                leftCapture.Update();
            }
            if (rightCapture != null)
            {
                rightCapture.Update();
            }
        }

        void StartCapturing()
        {
            if (state == VRGestureManagerState.ReadyToRecord)
            {
                state = VRGestureManagerState.Recording;
            }
            else if (state == VRGestureManagerState.ReadyToDetect)
            {
                state = VRGestureManagerState.Detecting;
            }
        }

        void StopCapturing()
        {
            if (leftCapture.state == VRGestureCaptureState.Capturing || rightCapture.state == VRGestureCaptureState.Capturing)
            {
                //do nothing
            }
            else
            {
                //set state to READY
                if (state == VRGestureManagerState.Recording)
                {
                    state = VRGestureManagerState.ReadyToRecord;
                }
                else if (state == VRGestureManagerState.Detecting)
                {
                    state = VRGestureManagerState.ReadyToDetect;
                }
            }
        }

        #endregion

        #region LINE CAPTURE
        public void LineCaught(List<Vector3> capturedLine, HandType hand)
        {
            if (state == VRGestureManagerState.Recording || state == VRGestureManagerState.ReadyToRecord)
            {
                currentTrainer.TrainLine(capturedLine, hand);
            }
            else if (state == VRGestureManagerState.Detecting || state == VRGestureManagerState.ReadyToDetect)
            {
                currentRecognizer.RecognizeLine(capturedLine, hand, this);
            }
        }
        #endregion

        public void AutoSetup()
        {
            #if EDWON_VR_OCULUS
                if (GetComponent<OVRCameraRig>() != null)
                {
                    OVRCameraRig ovrCameraRig = GetComponent<OVRCameraRig>();
                    head = ovrCameraRig.centerEyeAnchor;
                    handLeft = ovrCameraRig.leftHandAnchor;
                    handRight = ovrCameraRig.rightHandAnchor;
                }
                else
                {
                    Debug.Log(
                        "Could not setup OculusVR rig, is this script on the top level of your OVRCameraRig?\nDid you define EDWON_VR_OCULUS in Project Settings > Player ?"
                        );
                }
            #endif
            #if EDWON_VR_STEAM
                if (GetComponent<SteamVR_ControllerManager>() != null)
                {
                    SteamVR_ControllerManager steamVRControllerManager = GetComponent<SteamVR_ControllerManager>();
                    head = GetComponentInChildren<SteamVR_Camera>().transform;
                    //TODO: CHECK IN EARLIER VERSION OF UNITY. MIGHT NEED A OR for SteamVR_Gameview
                    handLeft = steamVRControllerManager.left.transform;
                    handRight = steamVRControllerManager.right.transform;
                }
                else
                {
                    Debug.Log(
                        "Could not setup SteamVR rig, is this script on the top level of your SteamVR camera prefab?\nDid you define EDWON_VR_STEAM in Project Settings > Player ?"
                        );
                }
            #endif
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
            #if EDWON_VR_STEAM
                SteamVR_ControllerManager[] steamVR_cm = FindObjectsOfType<SteamVR_ControllerManager>();
                //What happens when we get here and we ONLY have 1 controller online.
                //Do both controllers end up getting the LEFT controller?

                leftController = steamVR_cm[0].left;
                rightController = steamVR_cm[0].right;

                inputLeft = gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Left, leftController);
                inputRight = gameObject.AddComponent<VRControllerInputSteam>().Init(HandType.Right, rightController);
                if (spawnControllerModels)
                    SpawnControllerModels();
            #endif

            #if EDWON_VR_OCULUS
                inputLeft = handLeft.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Left);
                inputRight = handRight.gameObject.AddComponent<VRControllerInputOculus>().Init(HandType.Right);
                if (spawnControllerModels)
                    SpawnControllerModels();
            #endif
        }

        public void SpawnControllerModels ()
        {
            Transform leftModel = GameObject.Instantiate(handLeftModel).transform;
            Transform rightModel = GameObject.Instantiate(handRightModel).transform;
            leftModel.parent = handLeft;
            rightModel.parent = handRight;
            leftModel.localPosition = Vector3.zero;
            rightModel.localPosition = Vector3.zero;
            leftModel.localRotation = Quaternion.identity;
            rightModel.localRotation = Quaternion.identity;
        }


        #region RECORDING/DETECTING
        public void BeginReadyToRecord(string gesture)
        {
            currentTrainer = new Trainer(gestureSettings.currentNeuralNet, gestureSettings.gestureBank);
            currentTrainer.CurrentGesture = gestureSettings.FindGesture(gesture); ;
            state = VRGestureManagerState.ReadyToRecord;
            leftCapture.state = VRGestureCaptureState.EnteringCapture;
            rightCapture.state = VRGestureCaptureState.EnteringCapture;
        }

        public void BeginEditing(string gesture)
        {
            currentTrainer.CurrentGesture = gestureSettings.FindGesture(gesture); ;
        }

        public void BeginDetect()
        {
            state = VRGestureManagerState.ReadyToDetect;
            currentRecognizer = new GestureRecognizer(gestureSettings.currentNeuralNet);
        }


        #endregion
    }


}









