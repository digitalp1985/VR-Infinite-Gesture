using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class TutorialUI : MonoBehaviour
    {

        TutorialUIPanelManager panelManager;

        public int currentTutorialStep = 1;

        void Start()
        {
            panelManager = GetComponentInChildren<TutorialUIPanelManager>();

            StartCoroutine(IETutorialSequence());
        }

        IEnumerator IETutorialSequence()
        {
            panelManager.FocusPanel(2.ToString());
            currentTutorialStep = 2;

            yield break;
        }

        public void OnButtonNext()
        {
            currentTutorialStep += 1;
            panelManager.FocusPanel(currentTutorialStep.ToString());
        }

        public void OnButtonBack()
        {
            currentTutorialStep -= 1;
            panelManager.FocusPanel(currentTutorialStep.ToString());
        }

    }
}