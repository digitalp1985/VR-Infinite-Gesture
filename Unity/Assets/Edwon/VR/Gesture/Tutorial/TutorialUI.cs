using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class TutorialUI : MonoBehaviour
    {
        VRGestureSettings gestureSettings;
        VRGestureSettings GestureSettings
        {
            get
            {
                if (gestureSettings == null)
                {
                    gestureSettings = Utils.GetGestureSettings();
                }
                return gestureSettings;
            }
        }

        TutorialUIPanelManager panelManager;
        TutorialUIPanelManager PanelManager
        {
            get
            {
                if (panelManager == null)
                {
                    panelManager = GetComponentInChildren<TutorialUIPanelManager>();
                }
                return panelManager;
            }
        }

        public int currentTutorialStep = 1;

        void Start()
        {
            // start is also called when you exit play mode
            if (!EditorApplication.isPlaying)
            {
                ResetPanel();
            }
            else
            {
                GoToTutorialStep(2);
            }
        }

        void ResetPanel()
        {
            GoToTutorialStep(1);
        }

        void GoToTutorialStep(int step)
        {
            currentTutorialStep = step;
            PanelManager.FocusPanel(step.ToString());
        }

        #region BUTTONS

        public void OnButtonNext()
        {
            GoToTutorialStep(currentTutorialStep + 1);
        }

        public void OnButtonBack()
        {
            GoToTutorialStep(currentTutorialStep - 1);
        }

        #endregion

    }
}