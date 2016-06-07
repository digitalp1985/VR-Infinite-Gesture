namespace WinterMute
{
    public enum GestureUIHand { Left, Right };
    public static class Config
    {
        public static string SAVE_FILE_PATH = @"Assets/3DGestureTracker/VRGestureData/";
        public static bool USE_RAW_DATA = true;
        public static int FIDELITY = 11;
        public static int CAPTURE_RATE = 30;
        public static double CONFIDENCE_LIMIT = 0.98;
        public static bool CONTINIOUS = false;
        public static GestureUIHand gestureUIHand = GestureUIHand.Right; // the hand to track
    }
}
