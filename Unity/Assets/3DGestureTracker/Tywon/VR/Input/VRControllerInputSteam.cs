using UnityEngine;
using System.Collections;
using WinterMute;

public class VRControllerInputSteam : VRController {
    [Header("SteamVR Options")]
    public float curlTime = .3f;
    public int deviceIndex;


    public VRControllerInputSteam(HandType whichHand)
    {
        handedness = whichHand;
        deviceIndex = GetSteamVRController();
    }
  
    // GET STEAM VR CONTROLLER
    // returns device index of left or right steam controller
    // I'm not sure we actually want this to happen... This is most likely why
    // the flip flop from left to right is always happening.
    //Maybe we should call this once on Start and keep the controllers set as left and right.
    public int GetSteamVRController()
    {
        int index = 0;
        if (handedness == HandType.Left)
        {
            index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
        }
        else
        {
            index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        }
        // do something if null
        if (index <= 0)
        {
            if (handedness == HandType.Left)
            {
                index = 1;
            }
            else
            {
                index = 2;
            }
        }
        return index;
    }

    public override void InputUpdate()
    {
        directional1 = SteamVR_Controller.Input(deviceIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);

        button1 = SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Touchpad);
        button1Down = SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
        button2 = SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);
        button2Down = SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
        trigger1Button = SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
        trigger1ButtonDown = SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
        trigger1ButtonUp = SteamVR_Controller.Input(deviceIndex).GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
        trigger2Button = SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Grip);
        trigger2ButtonDown = SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Grip);
        trigger2ButtonUp = SteamVR_Controller.Input(deviceIndex).GetPressUp(SteamVR_Controller.ButtonMask.Grip);
    }
}
