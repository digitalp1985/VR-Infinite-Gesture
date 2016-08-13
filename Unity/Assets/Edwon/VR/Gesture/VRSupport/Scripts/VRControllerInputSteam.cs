#if EDWON_VR_STEAM

using UnityEngine;
using System.Collections;
using Edwon.VR.Gesture;
using Valve.VR;

namespace Edwon.VR.Input
{
    public class VRControllerInputSteam : VRController
    {
        [Header("SteamVR Options")]
        public int deviceIndex;
        public GameObject _hand;

        public IInput Init(HandType handy, GameObject hand)
        {
            handedness = handy;
            _hand = hand;
            //gameObject.SetActive(true);
            StartCoroutine(RegisterIndex());
            return this;
        }

        IEnumerator RegisterIndex()
        {
            //If the other controller as registered as the left and they get flopped before this one gets registered
            //We sometimes will end up with two controllers and the same index.
            //Steam always assumes that the first controller you turn on is the RIGHT hand controller.
            //But sometimes it will flip flop them around when you turn on the left controller.
            //It's not clear when or why this happens and it's a huge pain in the ass.
            
            //yield return new WaitForSeconds(.2f);
            while (true)
            {
                deviceIndex = (int)_hand.GetComponent<SteamVR_TrackedObject>().index;
                if (deviceIndex > -1)
                {
                    //Debug.Log("I just got registered. Index: " + deviceIndex);
                    yield break;
                }
                yield return null;
            }
        }

        void OnEnable()
        {
            SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
            SteamVR_Utils.Event.Listen("TrackedDeviceRoleChanged", OnTrackedDeviceRoleChanged);
        }

        void OnDestroy()
        {
            //Debug.Log("I am being destroyed");
            SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
            SteamVR_Utils.Event.Remove("TrackedDeviceRoleChanged", OnTrackedDeviceRoleChanged);
        }

        void LateUpdate()
        {
            //Only attempt this if Device Index != -1
            if (deviceIndex > -1)
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






        private void OnTrackedDeviceRoleChanged(params object[] args)
        {
            
            Debug.Log("TRACKED DEVICE ROLE CHANGE");
            Refresh();
        }


        private void OnDeviceConnected(params object[] args)
        {

            Debug.Log("THIS DEVICE DONE GOT CONNECTED!");
            Refresh();


            //var index = (uint)(int)args[0];
            //bool changed = this.connected[index];
            //this.connected[index] = false;

            //var connected = (bool)args[1];
            //if (connected)
            //{
            //    var system = OpenVR.System;
            //    if (system != null && system.GetTrackedDeviceClass(index) == ETrackedDeviceClass.Controller)
            //    {
            //        this.connected[index] = true;
            //        changed = !changed; // if we clear and set the same index, nothing has changed
            //    }
            //}

            //if (changed)
            //    Refresh();
        }

        public void Refresh()
        {

            uint leftIndex = 0;
            uint rightIndex = 0;

            var system = OpenVR.System;
            if (system != null)
            {
                leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            }


            Debug.Log("LEFT INDEX = " + leftIndex + " RIGHT INDEX =" + rightIndex);



            //int objectIndex = 0;

            //var system = OpenVR.System;
            //if (system != null)
            //{
            //    leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            //    rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            //}

            //// If neither role has been assigned yet, try hooking up at least the right controller.
            //if (leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
            //{
            //    for (uint deviceIndex = 0; deviceIndex < connected.Length; deviceIndex++)
            //    {
            //        if (connected[deviceIndex])
            //        {
            //            SetTrackedDeviceIndex(objectIndex++, deviceIndex);
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    SetTrackedDeviceIndex(objectIndex++, (rightIndex < connected.Length && connected[rightIndex]) ? rightIndex : OpenVR.k_unTrackedDeviceIndexInvalid);
            //    SetTrackedDeviceIndex(objectIndex++, (leftIndex < connected.Length && connected[leftIndex]) ? leftIndex : OpenVR.k_unTrackedDeviceIndexInvalid);

            //    // Assign out any additional controllers only after both left and right have been assigned.
            //    if (leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
            //    {
            //        for (uint deviceIndex = 0; deviceIndex < connected.Length; deviceIndex++)
            //        {
            //            if (objectIndex >= objects.Length)
            //                break;

            //            if (!connected[deviceIndex])
            //                continue;

            //            if (deviceIndex != leftIndex && deviceIndex != rightIndex)
            //            {
            //                SetTrackedDeviceIndex(objectIndex++, deviceIndex);
            //            }
            //        }
            //    }
            //}

            //// Reset the rest.
            //while (objectIndex < objects.Length)
            //{
            //    SetTrackedDeviceIndex(objectIndex++, OpenVR.k_unTrackedDeviceIndexInvalid);
            //}
        }

    }
}

#endif