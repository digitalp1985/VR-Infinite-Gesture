using UnityEngine;
using System;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [Serializable]
    public class TutorialSettings
    {
        public const string TUTORIAL_SAVE_PATH = @"Assets/Edwon/VR/Gesture/Tutorial/Settings/TutorialSettings.txt";

        public int currentTutorialStep = 1;
        public TutorialState tutorialState = TutorialState.SetupVR;
    }
}