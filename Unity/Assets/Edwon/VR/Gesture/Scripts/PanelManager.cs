using UnityEngine;
using System.Collections.Generic;

namespace Edwon.VR.Gesture
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelManager : MonoBehaviour
    {

        [HideInInspector]
        public string currentPanel;

        public delegate void PanelFocusChanged(string panelName);
        public static event PanelFocusChanged OnPanelFocusChanged;

        [HideInInspector]
        public List<CanvasGroup> panels;

        [HideInInspector]
        public CanvasGroup canvasGroup;

        public void Awake()
        {
            InitPanels();
        }

        void InitPanels()
        {
            // get the panels below me
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            panels = new List<CanvasGroup>();
            CanvasGroup[] panelsTemp = transform.GetComponentsInChildren<CanvasGroup>();
            for (int i = 0; i < panelsTemp.Length; i++)
            {
                if (panelsTemp[i] != canvasGroup)
                {
                    if (panelsTemp[i].transform.parent == transform)
                        panels.Add(panelsTemp[i]);
                }
            }
        }

        public void FocusPanel(string panelName)
        {
            currentPanel = panelName;

            foreach (CanvasGroup panel in panels)
            {
                if (panel.gameObject.name == panelName)
                {
                    Utils.ToggleCanvasGroup(panel, true);
                }
                else
                {
                    Utils.ToggleCanvasGroup(panel, false);
                }
            }

            if (OnPanelFocusChanged != null)
            {
                OnPanelFocusChanged(panelName);
            }

        }

    }
}