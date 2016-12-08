using UnityEngine;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class TutorialPanel : Panel
    {

        public override void TogglePanelVisibility(bool _visible)
        {
            base.TogglePanelVisibility(_visible);

            if (_visible == false)
            {
                ToggleOtherStuff(canvasGroup, false);
            }
            else
            {
                ToggleOtherStuff(canvasGroup, true);
            }
        }

        void ToggleOtherStuff(CanvasGroup cg, bool enabled)
        {
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
