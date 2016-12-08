using UnityEngine;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class GettingStartedTutorialUIPanelManager : PanelManager
    {
        public string initialPanel;

        new public void Awake ()
        {
            base.Awake();
            FocusPanel(initialPanel);
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                FocusPanel("2");
            }
        }

    }
}
