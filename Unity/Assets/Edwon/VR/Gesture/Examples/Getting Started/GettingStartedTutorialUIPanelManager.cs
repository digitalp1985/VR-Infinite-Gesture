using UnityEngine;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class GettingStartedTutorialUIPanelManager : PanelManager
    {
        public string initialPanel;

        new public void Awake ()
        {
            Debug.Log("awake");
            base.Awake();
            FocusPanel(initialPanel);
        }

    }
}
