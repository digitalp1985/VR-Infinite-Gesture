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

        VRGestureManagerState state;
        VRGestureManagerState stateLast;
        public InputOptions.Button gestureButton = InputOptions.Button.Trigger1;

        GestureTrail myTrail;
        List<Vector3> currentCapturedLine;

        float nextRenderTime = 0;
        float renderRateLimit = Config.CAPTURE_RATE;
        float nextTestTime = 0;
        float testRateLimit = 500;

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

        void LineCaught(List<Vector3> captureLine)
        {

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

        public Vector3 getLocalizedPoint(Vector3 myDumbPoint)
        {
            perpTransform.position = playerHead.position;
            perpTransform.rotation = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);
            return perpTransform.InverseTransformPoint(myDumbPoint);
        }
    }
}
