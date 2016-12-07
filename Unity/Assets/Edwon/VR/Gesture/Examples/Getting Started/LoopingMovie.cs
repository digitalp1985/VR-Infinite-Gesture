using UnityEngine;

namespace Edwon.VR.Gesture
{
    [ExecuteInEditMode]
    public class LoopingMovie : MonoBehaviour
    {

        public Texture movieFile;
        Renderer movieRenderer;
        MovieTexture movieTexture;

        void OnEnable()
        {
            //EditorApplication.update += EditorUpdate;
        }

        static void EditorUpdate()
        {
            //Debug.Log("editor update");
        }

        void OnRenderObject()
        {
            //Debug.Log("on render object");
            if (movieRenderer == null)
            {
                movieRenderer = GetComponent<Renderer>();
            }
            if (movieRenderer != null)
            {
                if (movieFile == null)
                {
                    Debug.Log("add a movie file to " + gameObject.name + " please");
                }
                else
                {
                    movieRenderer.sharedMaterial.mainTexture = movieFile;
                }

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
        }

    }
}