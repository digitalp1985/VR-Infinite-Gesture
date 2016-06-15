using UnityEngine;
using Edwon.VR.Gesture;

namespace HutongGames.PlayMaker.Actions
{

    [ActionCategory("VRGestureTracker")]
    [Tooltip ("Fires when a gesture is captured but can't be recognized fromt the Edwon VR Gesture Tracker plugin")]
    public class VRGestureNullEvent : FsmStateAction
    {
        public FsmEvent gestureNullEvent;

        // Code that runs on entering the state.
        public override void OnEnter()
	    {
            VRGestureManager.GestureNullEvent += OnGestureNull;
        }

	    // Code that runs when exiting the state.
	    public override void OnExit()
	    {
            VRGestureManager.GestureNullEvent -= OnGestureNull;
        }

        void OnGestureNull ()
        {
            Fsm.Event(gestureNullEvent);
        }
    }

}
