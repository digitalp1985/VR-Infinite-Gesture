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
        TutorialSettings tutorialSettings;
        public TutorialSettings TutorialSettings
        {
            get
            {
                if (tutorialSettings == null)
                {
                    tutorialSettings = new TutorialSettings();
                    return tutorialSettings;
                }
                return tutorialSettings;                
            }
            set
            {
                tutorialSettings = value;
            }
        }

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
            // start - when play mode starts
            if (EditorApplication.isPlaying)
            {
                // if no file yet
                if (ReadTutorialSettings() == null)
                {
                    //if first time go to step to
                    GoToTutorialStep(2);
                }

                TutorialSettings = ReadTutorialSettings();
                if (TutorialSettings.currentTutorialStep == 1)
                {
                    GoToTutorialStep(2);
                }
                else
                {
                    GoToTutorialStep(TutorialSettings.currentTutorialStep);
                }
            }

            // start - when edit mode starts
            if (!EditorApplication.isPlaying)
            {
                RefreshTutorialSettings();
                GoToTutorialStep(TutorialSettings.currentTutorialStep);
            }

            if (tutorialState == TutorialState.InitialSetup)
            {
                GetComponent<Canvas>().worldCamera = CameraUI;
            }

            SwitchTutorialState(tutorialState);
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                GoToTutorialStep(TutorialSettings.currentTutorialStep + 1);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (TutorialSettings.currentTutorialStep - 1 >= 1)
                {
                    GoToTutorialStep(TutorialSettings.currentTutorialStep - 1);
                }
            }
        }

        void GoToTutorialStep(int step)
        {
            TutorialSettings.currentTutorialStep = step;
            SaveTutorialSettings(TutorialSettings);

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
                            #if EDWON_VR_OCULUS
                            GestureSettings.Rig.GetComponent<OVRCameraRig>().enabled = false;
                            GestureSettings.Rig.GetComponent<OVRManager>().enabled = false;
#endif
                            CameraUI.tag = "MainCamera";
                            GestureSettings.Rig.head.GetComponent<Camera>().tag = "Untagged";
                            GestureSettings.Rig.head.GetComponent<Camera>().enabled = false;
                            GestureSettings.Rig.enabled = false;
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
                            #if EDWON_VR_OCULUS
                            GestureSettings.Rig.GetComponent<OVRCameraRig>().enabled = true;
                            GestureSettings.Rig.GetComponent<OVRManager>().enabled = true;
                            #endif
                            CameraUI.tag = "Untagged";
                            GestureSettings.Rig.head.GetComponent<Camera>().tag = "MainCamera";
                            GestureSettings.Rig.head.GetComponent<Camera>().enabled = true;
                            GestureSettings.Rig.enabled = true;
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

        public void SaveTutorialSettings(TutorialSettings instance)
        {
            string json = JsonUtility.ToJson(instance, true);
            System.IO.File.WriteAllText(TutorialSettings.TUTORIAL_SAVE_PATH, json);
        }

        void RefreshTutorialSettings()
        {
            TutorialSettings = ReadTutorialSettings();
        }

        public TutorialSettings ReadTutorialSettings()
        {
            if (System.IO.File.Exists(TutorialSettings.TUTORIAL_SAVE_PATH))
            {
                string text = System.IO.File.ReadAllText(TutorialSettings.TUTORIAL_SAVE_PATH);
                return JsonUtility.FromJson<TutorialSettings>(text);
            }
            return null;
        }

        #region BUTTONS

        public void OnButtonNext()
        {
            GoToTutorialStep(TutorialSettings.currentTutorialStep + 1);
        }

        public void OnButtonBack()
        {
            GoToTutorialStep(TutorialSettings.currentTutorialStep - 1);
        }

        #endregion

    }
}