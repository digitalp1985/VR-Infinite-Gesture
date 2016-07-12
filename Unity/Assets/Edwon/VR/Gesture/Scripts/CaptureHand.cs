using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR.Input;

namespace Edwon.VR.Gesture
{
    public enum VRGestureCaptureState {EnteringCapture, ReadyToCapture, Captureing };

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
        public VRGestureCaptureState state;
        public InputOptions.Button gestureButton = InputOptions.Button.Trigger1;

        GestureTrail myTrail;
        List<Vector3> currentCapturedLine;

        float nextRenderTime = 0;
        float renderRateLimit = Config.CAPTURE_RATE;
        float nextTestTime = 0;
        float testRateLimit = 500;

        public delegate void StartCapture();
        public event StartCapture StartCaptureEvent;
        public delegate void ContinueCapture(Vector3 capturePoint);
        public event ContinueCapture ContinueCaptureEvent;
        public delegate void StopCapture();
        public event StopCapture StopCaptureEvent;

        public CaptureHand Init(VRGestureRig _rig, Transform _perp, HandType hand)
        {

            rig = _rig;
            playerHand = rig.GetHand(hand);
            playerHead = rig.head;
            perpTransform = _perp;
            input = rig.GetInput(hand);
            currentCapturedLine = new List<Vector3>();
            return this;
        }

        // Use this for initialization
        void Start() {
            if (VRGestureManager.Instance.displayGestureTrail)
            {
                myTrail = VRGestureManager.Instance.gameObject.AddComponent<GestureTrail>().Init(this);
            }
        }

        public void LineCaught(List<Vector3> capturedLine)
        {
            VRGestureManager.Instance.LineCaught(capturedLine);
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
            //get the position from the left anchor.
            //draw a point.

            if (rig != null)
            {

                if (VRGestureManager.Instance.state == VRGestureManagerState.Recording)
                {
                    UpdateRecord();
                }
                else if (VRGestureManager.Instance.state == VRGestureManagerState.Detecting)
                {
                    if (VRGestureManager.Instance.vrGestureDetectType == VRGestureDetectType.Continious)
                    {
                        //UpdateContinual();
                    }
                    else
                    {
                        UpdateRecord();
                    }
                }
            }
        }

        void UpdateRecord()
        {
            if (input.GetButtonUp(gestureButton))
            {
                state = VRGestureCaptureState.ReadyToCapture;
                StopRecording();
            }

            if (input.GetButtonDown(gestureButton) && state == VRGestureCaptureState.ReadyToCapture)
            {
                state = VRGestureCaptureState.Captureing;
                StartRecording();
            }

            if (state == VRGestureCaptureState.Captureing)
            {
                CapturePoint();
            }
        }

        void StartRecording()
        {
            Debug.Log("Start Record");
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
            Debug.Log("Stop Recording");
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
