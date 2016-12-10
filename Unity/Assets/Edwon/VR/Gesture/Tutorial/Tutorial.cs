using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class Tutorial : MonoBehaviour
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

        public enum TutorialState { InitialSetup, VRSetupComplete, InVR };
        public TutorialState tutorialState;

        Camera cameraUI;
        public Camera CameraUI
        {
            get
            {
                if (cameraUI == null)
                {
                    cameraUI = transform.GetComponentInChildren<Camera>();
                    return cameraUI;
                }
                else
                {
                    return cameraUI;
                }
            }
        }

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

            if (tutorialState == TutorialState.InitialSetup)
            {
                GetComponent<Canvas>().worldCamera = CameraUI;
            }
        }

        void OnGUI()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnButtonNext();
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

        public void SwitchTutorialState(TutorialState state)
        {
            switch (state)
            {
                case TutorialState.InitialSetup:
                    {
                        Debug.Log("initial setup");
                        CameraUI.enabled = true;
                        EnableTutorialUISystem(true);
                        PlayerSettings.virtualRealitySupported = false;
                        if (GestureSettings.Rig != null)
                        {
                            GestureSettings.Rig.GetComponent<OVRCameraRig>().enabled = false;
                            GestureSettings.Rig.head.GetComponent<Camera>().enabled = false;
                        }
                    }
                    break;
                case TutorialState.VRSetupComplete:
                    {
                        Debug.Log("VR SEtup complete");
                        CameraUI.enabled = false;
                    }
                    break;
                case TutorialState.InVR:
                    {
                        Debug.Log("in VR");
                        CameraUI.enabled = false;
                        EnableTutorialUISystem(false);
                        PlayerSettings.virtualRealitySupported = true;
                        if (GestureSettings.Rig != null)
                        {
                            GestureSettings.Rig.GetComponent<OVRCameraRig>().enabled = true;
                            GestureSettings.Rig.head.GetComponent<Camera>().enabled = true;
                        }
                    }
                    break;
            }
        }

        void EnableTutorialUISystem(bool enabled)
        {
            GetComponent<EventSystem>().enabled = enabled;
            GetComponent<Canvas>().worldCamera = CameraUI;
            //GetComponent<StandaloneInputModule>().enabled = enabled;
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