using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class TutorialPanel : Panel
    {

        VRGestureSettings gestureSettings;

        public override void Init()
        {
            base.Init();

            if (gestureSettings == null)
            {
                gestureSettings = Utils.GetGestureSettings();
            }
        }

        public override void TogglePanelVisibility(bool _visible)
        {
            //base.TogglePanelVisibility(_visible);

            if (_visible == false)
            {
                ToggleOtherStuff(canvasGroup, false);
                visible = false;
            }
            else
            {
                ToggleOtherStuff(canvasGroup, true);
                visible = true;
            }
        }

        void ToggleOtherStuff(CanvasGroup _canvasGroup, bool enabled)
        {
            Init();

            CanvasGroup[] cgs = _canvasGroup.GetComponentsInChildren<CanvasGroup>();
            foreach(CanvasGroup cg in cgs)
            {
                // if oculus ui, toggle that
                if (gestureSettings.vrType == VRType.OculusVR && cg.gameObject.name == "Oculus")
                {
                    Utils.ToggleCanvasGroup(cg, enabled);
                    ToggleChildMovies(cg.transform, enabled);
                }
                // if steam ui, toggle that
                else if (gestureSettings.vrType == VRType.SteamVR && cg.gameObject.name == "Steam")
                {
                    Utils.ToggleCanvasGroup(cg, enabled);
                    ToggleChildMovies(cg.transform, enabled);
                }
                // else toggle the whole thing
                else
                {
                    Utils.ToggleCanvasGroup(canvasGroup, enabled);
                    ToggleChildMovies(canvasGroup.transform, enabled);
                }
            }
        }

        void ToggleChildMovies(Transform parent, bool enabled)
        {
            MovieLooping[] movies = parent.GetComponentsInChildren<MovieLooping>();
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
