using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Edwon.VR.Gesture
{
    public class MovieLooping : MonoBehaviour
    {

        public RawImage movieImage; // ui version
        MovieTexture movieTexture;

        public void PlayMovie()
        {
            StartCoroutine(IEPlayMovieDelay(.1f));
        }

        IEnumerator IEPlayMovieDelay(float delay)
        {
            if (movieTexture == null)
            {
                movieTexture = (MovieTexture)movieImage.texture;
            }

            yield return new WaitForSeconds(delay);

            if (gameObject.name == "Movie Test")
                Debug.Log("play Movie Test");

            movieTexture.loop = true;
            movieTexture.Play();

            yield break;
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