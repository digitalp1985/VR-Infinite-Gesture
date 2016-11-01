
namespace Edwon.VR
{
    public enum Handedness { Left, Right };
    public enum VRType { OculusVR, SteamVR };
}


namespace Edwon.VR.Gesture
{
    public static class Config
    {
        public const string SAVE_FILE_PATH = @"Assets/Edwon/VR/Gesture/Data/";
        public const string SETTINGS_ASSET_PATH = @"Assets/Edwon/VR/Gesture/Settings/Settings.asset";

        // records raw, unmodified tracking data when recording gestures
		// this should probably always be true
        public const bool USE_RAW_DATA = true;

        // how many points to use in a gesture line
        // should not be changed unless the neural network is changed as well
        // (changing this is not currently supported)
        public const int FIDELITY = 11; 

        // how many points captured per second
        public const int CAPTURE_RATE = 30;  
    }
}
