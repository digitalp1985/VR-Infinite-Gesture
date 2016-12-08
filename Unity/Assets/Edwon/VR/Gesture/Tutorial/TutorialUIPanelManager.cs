using UnityEngine;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class TutorialUIPanelManager : PanelManager
    {
        public string initialPanel;

        new public void Awake ()
        {
            base.Awake();
            FocusPanel(initialPanel);
        }

    }
}
