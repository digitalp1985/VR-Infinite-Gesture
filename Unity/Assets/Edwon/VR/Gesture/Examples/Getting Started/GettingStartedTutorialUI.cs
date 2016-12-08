using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class GettingStartedTutorialUI : MonoBehaviour
    {

        GettingStartedTutorialUIPanelManager panelManager;

        void Start()
        {
            panelManager = GetComponentInChildren<GettingStartedTutorialUIPanelManager>();

            StartCoroutine(IETutorialSequence());
        }

        IEnumerator IETutorialSequence()
        {
            panelManager.FocusPanel("2");

            yield break;
        }

        void OnApplicationQuit()
        {
            Debug.Log("onquit");
        }
    }
}