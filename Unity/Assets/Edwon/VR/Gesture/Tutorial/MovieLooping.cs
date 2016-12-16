using UnityEngine;
using UnityEngine.UI;

namespace Edwon.VR.Gesture
{
    public class MovieLooping : MonoBehaviour
    {

        public RawImage movieImage; // ui version
        MovieTexture movieTexture;

        void Start()
        {

        }

        public void PlayMovie()
        {
            if (movieTexture == null)
            {
                movieTexture = (MovieTexture)movieImage.texture;
            }

            if (gameObject.name == "Movie Test")
                Debug.Log("play Movie Test");

            movieTexture.Play();
        }

        public void StopMovie()
        {
            if (movieTexture == null)
            {
                movieTexture = (MovieTexture)movieImage.texture;
            }

            if (gameObject.name == "Movie Test")
                Debug.Log("stop Movie Test");

            movieTexture.Stop();
        }

        public void ToggleVisibility (bool enabled)
        {
            if (gameObject.name == "Movie Test")
                Debug.Log("toggle visibility of Movie Test " + enabled);

            if (movieImage != null)
            {
                if (enabled)
                {
                    movieImage.color = new Color(1, 1, 1, 1);
                    //movieImage.enabled = true;
                }
                else
                {
                    movieImage.color = new Color(1, 1, 1, 0);
                    //movieImage.enabled = false;
                }
            }
        }

    }
}