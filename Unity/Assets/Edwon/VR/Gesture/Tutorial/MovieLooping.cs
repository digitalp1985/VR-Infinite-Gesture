using UnityEngine;
using UnityEngine.UI;

namespace Edwon.VR.Gesture
{
    public class MovieLooping : MonoBehaviour
    {

        RawImage movieImage; // ui version
        MovieTexture movieTexture;

        void Start ()
        {
            PlayMovie();
        }
    
        void PlayMovie()
        {
            if (movieImage == null)
            {
                movieImage = GetComponent<RawImage>();
            }
            if (movieImage != null)
            {
                if (movieTexture == null)
                {
                    movieTexture = (MovieTexture)movieImage.texture;
                }
                else
                {
                    if (!movieTexture.isPlaying)
                    {
                        movieTexture.loop = true;
                        movieTexture.Play();
                    }
                }
            }
        }

        public void ToggleVisibility (bool enabled)
        {
            if (movieImage == null)
            {
                movieImage = GetComponent<RawImage>();
            }
            if (movieImage != null)
            {
                if (enabled)
                    movieImage.color = new Color(1, 1, 1, 1);
                else
                    movieImage.color = new Color(1, 1, 1, 0);
            }

            PlayMovie();
        }

    }
}