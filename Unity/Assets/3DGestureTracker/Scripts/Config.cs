namespace WinterMute
{
    public enum GestureHand { Left, Right };
    public static class Config
    {
       
        public static string SAVE_FILE_PATH = @"Assets/3dGestureTracker/VRGestureData/";
        public static bool USE_RAW_DATA;
        public static int FIDELITY;
        public static int CAPTURE_RATE;
        public static double CONFIDENCE_LIMIT = 0.98;
        public static bool CONTINIOUS = false;
        public static GestureHand gestureHand = GestureHand.Right; // the hand to track
    }
}
