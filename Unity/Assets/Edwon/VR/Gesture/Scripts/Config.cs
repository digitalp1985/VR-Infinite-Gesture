
namespace Edwon.VR
{
    public enum Handedness { Left, Right };
    public enum VRType { OculusVR, SteamVR };
}


namespace Edwon.VR.Gesture
{
    public static class Config
    {
        public const string RESOURCES_PATH = @"Assets/Edwon/VR/Gesture/Resources/VR Infinite Gesture/";
        public const string RESOURCES_PARENT_PATH = @"VR Infinite Gesture/";
        public const string GESTURE_FILE_PATH = 
            RESOURCES_PATH + "Neural Networks/";
        public const string SETTINGS_FILE_PATH = 
            RESOURCES_PATH + "Settings/Settings.asset";

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
