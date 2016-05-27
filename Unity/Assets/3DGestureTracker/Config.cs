namespace WinterMute
{
    public static class Config
    {
        public const string SAVE_FILE_PATH = @"Assets/3DGestureTracker/SAVE_DATA/";
        public const bool USE_RAW_DATA = true;
        public const int FIDELITY = 11;
        public const int CAPTURE_RATE = 30;
        public const double CONFIDENCE_LIMIT = 0.98;
        public const bool CONTINIOUS = false;
        public enum Handedness { Left, Right, Both };
        public const Handedness handedness = Handedness.Right; // the hand to track
    }
}
