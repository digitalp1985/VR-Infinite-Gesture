using UnityEngine;
using UnityEngine.UI;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class MovieLooping : MonoBehaviour
    {

        Renderer movieRenderer;
        RawImage movieImage; // ui version
        MovieTexture movieTexture;

        void Start ()
        {
            Init();
        }

        void Init()
        {
            if (movieRenderer == null)
            {
                movieRenderer = GetComponent<Renderer>();
            }
        }

        void OnRenderObject()
        {
            #region MOVIE RENDERER VERSION
            if (movieRenderer == null)
            {
                movieRenderer = GetComponent<Renderer>();
            }
            if (movieRenderer != null)
            {
                if (movieTexture == null)
                {
                    movieTexture = (MovieTexture)movieRenderer.sharedMaterial.mainTexture;
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
            #endregion

            #region RAW IMAGE (UI) VERSION
            if (movieRenderer == null)
            {
                if (movieImage == null)
                {
                    movieImage = GetComponent<RawImage>();
                }
                else
                {
                    if (movieTexture == null)
                    {
                        movieTexture = (MovieTexture)movieImage.material.mainTexture;
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
            #endregion
        }

        public void ToggleVisibility (bool enabled)
        {
            Init();

            if (movieRenderer != null)
            {
                movieRenderer.enabled = enabled;
            }
        }

    }
}