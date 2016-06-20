using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Edwon.VR.Gesture
{
    public class VRGestureUIPanelManager : MonoBehaviour
    {
        Animator panelAnim;
        private string initialPanel = "Select Neural Net Menu";
        public string currentPanel;

        public delegate void PanelFocusChanged(string panelName);
        public static event PanelFocusChanged OnPanelFocusChanged;

        void Start()
        {
            panelAnim = GetComponent<Animator>();

            if (VRGestureManager.Instance.stateInitial == VRGestureManagerState.ReadyToDetect)
            {
                initialPanel = "Detect Menu";
            }

            // initialize initial panel focused
            FocusPanel(initialPanel);
        }

        public void FocusPanel(string panelName)
        {
            OnPanelFocusChanged(panelName);
            panelAnim.SetTrigger(panelName);
            currentPanel = panelName;
        }

        // UTILITY

        static GameObject FindFirstEnabledSelectable(GameObject gameObject)
        {
            GameObject go = null;
            var selectables = gameObject.GetComponentsInChildren<Selectable>(true);
            foreach (var selectable in selectables)
            {
                if (selectable.IsActive() && selectable.IsInteractable())
                {
                    go = selectable.gameObject;
                    break;
                }
            }
            return go;
        }

        private void SetSelected(GameObject go)
        {
            EventSystem.current.SetSelectedGameObject(go);

            var standaloneInputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
            if (standaloneInputModule != null && standaloneInputModule.inputMode == StandaloneInputModule.InputMode.Buttons)
                return;

            EventSystem.current.SetSelectedGameObject(null);
        }

    }
}