using UnityEngine;
using System.Collections;
using WinterMute;

public class VRControllerInputSteam : VRController
{
    [Header("SteamVR Options")]
    public float curlTime = .3f;
    public int deviceIndex;
    bool properlySetIndex = false;

    public IInput Init(HandType handy)
    {
        handedness = handy;
        gameObject.active = true;
        StartCoroutine("RegisterIndex");
        return this;
    }

    IEnumerator RegisterIndex()
    {
        for (;;)
        {
            deviceIndex = (int)gameObject.GetComponent<SteamVR_TrackedObject>().index;
            Debug.Log("CoRoutine : "+ deviceIndex);
            if(deviceIndex > -1)
            {
                Debug.Log("FOUND IT STOPPING NOW");
                yield break;
            }
            else
            {
                Debug.Log("I'm going");
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    void LateUpdate()
    {
        //Only attempt this if Device Index != -1
        if(deviceIndex > -1)
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
            if (trigger1Button)
            {
                Debug.Log("YOU PUSHED A TRIGGER");
            }
        }

    }
}
