using UnityEngine;
using System.Collections;


namespace Edwon.VR.Gesture
{
    public class Panel : MonoBehaviour
    {
        [HideInInspector]
        public bool visible;
        PanelManager panelManager;
        [HideInInspector]
        public CanvasGroup canvasGroup;

        void Start()
        {
            Init();
        }

        void Init()
        {
            if (panelManager == null)
            {
                panelManager = GetComponentInParent<PanelManager>();
            }
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public virtual void TogglePanelVisibility(bool _visible)
        {
            Init();

            if (_visible == false)
            {
                Utils.ToggleCanvasGroup(canvasGroup, false);
                visible = false;
            }
            else
            {
                Utils.ToggleCanvasGroup(canvasGroup, true);
                visible = true;
            }
        }

        public void SoloPanelVisibility()
        {
            Init();

            panelManager.FocusPanel(gameObject.name);
        }

    }
}
