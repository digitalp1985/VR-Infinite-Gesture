using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR.Input;

namespace Edwon.VR.Gesture
{


    public class CaptureHand : MonoBehaviour {

        public VRGestureRig rig;
        IInput input;
        Transform playerHead;
        Transform playerHand;
        Transform perpTransform;

        Trainer currentTrainer;
        GestureRecognizer currentRecognizer;
        string gestureToRecord;


        //Maybe have two states.
        //One that is: Record, Detect, Idel, Edit, Train
        //Another that is EnteringCapture, ReadyToCapture, Capturing
        VRGestureManagerState state;
        VRGestureManagerState stateLast;
        public InputOptions.Button gestureButton = InputOptions.Button.Trigger1;

        GestureTrail myTrail;
        List<Vector3> currentCapturedLine;

        float nextRenderTime = 0;
        float renderRateLimit = Config.CAPTURE_RATE;
        float nextTestTime = 0;
        float testRateLimit = 500;

        public delegate void GestureDetected(string gestureName, double confidence);
        public static event GestureDetected GestureDetectedEvent;
        public delegate void GestureRejected(string error, string gestureName = null, double confidence = 0);
        public static event GestureRejected GestureRejectedEvent;
        public delegate void StartCapture();
        public static event StartCapture StartCaptureEvent;
        public delegate void ContinueCapture(Vector3 capturePoint);
        public static event ContinueCapture ContinueCaptureEvent;
        public delegate void StopCapture();
        public static event StopCapture StopCaptureEvent;

        void Init(Transform _hand, Transform _head, Transform _perp, IInput _input)
        {
            playerHand = _hand;
            playerHead = _head;
            perpTransform = _perp;
            input = _input;
        }

        // Use this for initialization
        void Start() {

        }

        public void LineCaught(List<Vector3> capturedLine)
        {
            if (state == VRGestureManagerState.Recording || state == VRGestureManagerState.ReadyToRecord)
            {

                TrainLine(gestureToRecord, capturedLine);
            }
            else if (state == VRGestureManagerState.Detecting || state == VRGestureManagerState.ReadyToDetect)
            {
                RecognizeLine(capturedLine);
            }
        }

        public void TrainLine(string gesture, List<Vector3> capturedLine)
        {

            currentTrainer.AddGestureToTrainingExamples(gesture, capturedLine);
            VRGestureManager.Instance.debugString = "trained : " + gesture;
        }

        public void RecognizeLine(List<Vector3> capturedLine)
        {
            if (IsGestureBigEnough(capturedLine))
            {
                //Detect if the captured line meets minimum gesture size requirements
                double[] networkInput = Utils.Instance.FormatLine(capturedLine);
                string gesture = currentRecognizer.GetGesture(networkInput);
                string confidenceValue = currentRecognizer.currentConfidenceValue.ToString("N3");

                // broadcast gesture detected event
                if (currentRecognizer.currentConfidenceValue > VRGestureManager.Instance.confidenceThreshold)
                {
                    VRGestureManager.Instance.debugString = gesture + " " + confidenceValue;
                    if (GestureDetectedEvent != null)
                        GestureDetectedEvent(gesture, currentRecognizer.currentConfidenceValue);
                }
                else
                {
                    VRGestureManager.Instance.debugString = "Null \n" + gesture + " " + confidenceValue;
                    if (GestureRejectedEvent != null)
                        GestureRejectedEvent("Confidence Too Low", gesture, currentRecognizer.currentConfidenceValue);
                }
            }
            else
            {
                //broadcast that a gesture is too small??
                VRGestureManager.Instance.debugString = "Gesture is too small!";
                if (GestureRejectedEvent != null)
                    GestureRejectedEvent("Gesture is too small");

            }
        }

        public bool IsGestureBigEnough(List<Vector3> capturedLine)
        {
            float check = Utils.Instance.FindMaxAxis(capturedLine);
            return (check > VRGestureManager.Instance.minimumGestureAxisLength);
        }


        //This will get points in relation to a users head.
        public Vector3 getLocalizedPoint(Vector3 myDumbPoint)
        {
            perpTransform.position = playerHead.position;
            perpTransform.rotation = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);
            return perpTransform.InverseTransformPoint(myDumbPoint);
        }

        #region UPDATE
        // Update is called once per frame
        void Update()
        {
            stateLast = state;
            //get the position from the left anchor.
            //draw a point.
            if (rig != null)
            {
                if (state == VRGestureManagerState.ReadyToRecord ||
                    state == VRGestureManagerState.EnteringRecord ||
                    state == VRGestureManagerState.Recording)
                {
                    UpdateRecord();
                }
                else if (state == VRGestureManagerState.Detecting ||
                         state == VRGestureManagerState.EnteringDetect ||
                            state == VRGestureManagerState.ReadyToDetect)
                {
                    if (VRGestureManager.Instance.vrGestureDetectType == VRGestureDetectType.Continious)
                    {
                        //UpdateContinual();
                    }
                    else
                    {
                        UpdateDetectWithButtons();
                    }
                }
            }
        }

        void UpdateRecord()
        {
            if (input.GetButtonUp(gestureButton))
            {
                state = VRGestureManagerState.ReadyToRecord;
                StopRecording();
            }

            if (input.GetButtonDown(gestureButton) && state == VRGestureManagerState.ReadyToRecord)
            {
                state = VRGestureManagerState.Recording;
                StartRecording();
            }

            if (state == VRGestureManagerState.Recording)
            {
                CapturePoint();
            }
        }

        void UpdateDetectWithButtons()
        {
            if (input.GetButtonUp(gestureButton))
            {
                state = VRGestureManagerState.ReadyToDetect;
                StopRecording();
            }

            if (input.GetButtonDown(gestureButton) && state == VRGestureManagerState.ReadyToDetect)
            {
                state = VRGestureManagerState.Detecting;
                StartRecording();
            }

            if (state == VRGestureManagerState.Detecting)
            {
                CapturePoint();
            }
        }

        void StartRecording()
        {
            nextRenderTime = Time.time + renderRateLimit / 1000;
            if (StartCaptureEvent != null)
                StartCaptureEvent();
            CapturePoint();


        }

        void CapturePoint()
        {
            Vector3 rightHandPoint = playerHand.position;
            Vector3 localizedPoint = getLocalizedPoint(rightHandPoint);
            currentCapturedLine.Add(localizedPoint);
            if (ContinueCaptureEvent != null)
                ContinueCaptureEvent(rightHandPoint);
        }

        void StopRecording()
        {

            if (currentCapturedLine.Count > 0)
            {
                LineCaught(currentCapturedLine);
                currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
                currentCapturedLine.Clear();

                if (StopCaptureEvent != null)
                    StopCaptureEvent();
            }

        }

        #endregion
    }
}
