using UnityEngine;
using Edwon.VR.Gesture;

namespace HutongGames.PlayMaker.Actions
{

    [ActionCategory("VRGestureTracker")]
    [Tooltip ("Listens for gestures detected with the Edwon VR Gesture Tracker plugin")]
    public class VRGestureDetectedEvent : FsmStateAction
    {
        public FsmString gestureName;
        public FsmEvent gestureDetectedEvent;

        public override void Reset()
        {

        }

        // Code that runs on entering the state.
        public override void OnEnter()
	    {

            VRGestureManager.GestureDetectedEvent += OnGestureDetected;
        }

	    // Code that runs when exiting the state.
	    public override void OnExit()
	    {
            VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
        }

        void OnGestureDetected (string _gestureName, double _confidence)
        {
            if (_gestureName == gestureName.Value)
            {
                Debug.Log(_gestureName);
                Fsm.Event(gestureDetectedEvent);
            }
        }
    }

}
