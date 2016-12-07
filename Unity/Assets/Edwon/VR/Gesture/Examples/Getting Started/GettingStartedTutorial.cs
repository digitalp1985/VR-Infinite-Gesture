using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class GettingStartedTutorial : MonoBehaviour
{

    public Renderer movieRenderer;
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
    }

}
