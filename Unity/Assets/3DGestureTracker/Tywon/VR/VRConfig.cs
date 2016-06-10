using UnityEngine;
using System.Collections;

public class VRConfig {

}

public static class VROptions
{
    public enum VRTYPE { SimVR, OculusTouchVR, OculusGearVR, SteamVR, PlaystationVR, LeapMotionVR };
    public static VRTYPE vrType; // this is the actual vrType variable
}
