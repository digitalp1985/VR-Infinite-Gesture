using UnityEngine;
using System.Collections.Generic;

namespace Edwon.VR.Gesture
{
    // this script should be placed on the panels parent
    public class VRGestureUIPanelManager : PanelManager
    {
        private GestureSettings gestureSettings;

        new public void Awake()
        {
            base.Awake();

            gestureSettings = Utils.GetGestureSettings();

            if (gestureSettings.stateInitial == VRGestureUIState.ReadyToDetect)
            {
                initialPanel = "Detect Menu";
            }

            // initialize initial panel focused
            if (gestureSettings.neuralNets.Count <= 0)
                FocusPanel("No Neural Net Menu");
            else
                FocusPanel(initialPanel);
        }

        public new void FocusPanel(string panelName)
        {
            base.FocusPanel(panelName);

            foreach (CanvasGroup panel in panels)
            {
                if (panel.gameObject.name == panelName)
                {
                    VRGestureUI.ToggleCanvasGroup(panel, true);
                }
                else
                {
                    VRGestureUI.ToggleCanvasGroup(panel, false);
                }
            }
        }
    }
}