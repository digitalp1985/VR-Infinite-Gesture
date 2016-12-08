using UnityEngine;
using System.Collections;


namespace Edwon.VR.Gesture
{
    public class Panel : MonoBehaviour
    {
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

        public void TogglePanelVisibility(bool _visible)
        {
            Init();

            if (_visible == false)
            {
                Utils.ToggleCanvasGroup(canvasGroup, false);
                ToggleOtherStuff(canvasGroup, false);
                visible = false;
            }
            else
            {
                Utils.ToggleCanvasGroup(canvasGroup, true);
                ToggleOtherStuff(canvasGroup, true);
                visible = true;
            }
        }

        public void SoloPanelVisibility()
        {
            Init();

            panelManager.FocusPanel(gameObject.name);
        }

        void ToggleOtherStuff(CanvasGroup cg, bool enabled)
        {
            // toggle movies
            MovieLooping[] movies = cg.GetComponentsInChildren<MovieLooping>();
            if (movies != null)
            {
                foreach (MovieLooping movie in movies)
                {
                    movie.ToggleVisibility(enabled);
                }

            }
        }

    }
}
