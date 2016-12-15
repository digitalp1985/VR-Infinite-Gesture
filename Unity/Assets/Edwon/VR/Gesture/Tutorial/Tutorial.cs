using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public enum TutorialState { SetupVR, InVR };

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

        int inVRStep = 9; // starting at this step enter into VR

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
                else
                {
                    RefreshTutorialSettings();
                }

                // set to vr setup mode
                if (TutorialSettings.currentTutorialStep >= 1
                    && TutorialSettings.currentTutorialStep < inVRStep)
                {
                    SwitchTutorialState(TutorialState.SetupVR);
                }

                // load tutorial settings from file
                TutorialSettings = ReadTutorialSettings();
                if (TutorialSettings.currentTutorialStep == 1)
                {
                    GoToTutorialStep(2);
                }
                // if at the VR transition step
                else if (TutorialSettings.currentTutorialStep == inVRStep)
                {
                    // enter VR
                    GoToTutorialStep(inVRStep + 1);
                    SwitchTutorialState(TutorialState.InVR);
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

                // if at the VR transition step
                if (TutorialSettings.currentTutorialStep == inVRStep)
                {
                    // enter VR
                    SwitchTutorialState(TutorialState.InVR);
                }

                RefreshTutorialSettings();
                GoToTutorialStep(TutorialSettings.currentTutorialStep);
            }

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

        public void GoToTutorialStep(int step)
        {
            TutorialSettings.currentTutorialStep = step;
            SaveTutorialSettings(TutorialSettings);

            PanelManager.FocusPanel(step.ToString());

            if (step < 8)
            {
                SwitchTutorialState(TutorialState.SetupVR);
            }
            SaveTutorialSettings(tutorialSettings);
        }

        public void SwitchTutorialState(TutorialState state)
        {
            switch (state)
            {
                case TutorialState.SetupVR:
                    {
                        CameraUI.enabled = true;
                        GetComponent<EventSystem>().enabled = true;
                        GetComponent<Canvas>().worldCamera = transform.GetComponentInChildren<Camera>();
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
                case TutorialState.InVR:
                    {
                        CameraUI.enabled = false;
                        GetComponent<EventSystem>().enabled = false;
                        // set the tutorial canvas UI camera to the VR ui camera
                        if (FindObjectOfType<VRGestureUI>() != null)
                        {
                            VRGestureUI ui = FindObjectOfType<VRGestureUI>();
                            LaserPointerInputModule laserPointerInput = ui.GetComponent<LaserPointerInputModule>();
                            GetComponent<Canvas>().worldCamera = laserPointerInput.UICamera;
                        }
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

            TutorialSettings.tutorialState = state;
            SaveTutorialSettings(TutorialSettings);
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