using UnityEngine;
using Edwon.VR.Gesture;

namespace HutongGames.PlayMaker.Actions
{

    [ActionCategory("VRGestureTracker")]
    [Tooltip ("Listens for gestures detected with the Edwon VR Gesture Tracker plugin")]
    public class VRGestureEvent : FsmStateAction
    {

	    // Code that runs on entering the state.
	    public override void OnEnter()
	    {
            VRGestureManager.GestureDetectedEvent += OnGestureDetected;
            VRGestureManager.GestureNullEvent += OnGestureNull;
        }

        // Code that runs every frame.
        public override void OnUpdate()
	    {
	    
	    }

	    // Code that runs when exiting the state.
	    public override void OnExit()
	    {
            VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
            VRGestureManager.GestureNullEvent -= OnGestureNull;
        }

        void OnGestureDetected (string gestureName, double confidence)
        {
        }

        void OnGestureNull ()
        {

        }
    }

}
