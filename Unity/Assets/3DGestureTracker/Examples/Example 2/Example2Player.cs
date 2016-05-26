using UnityEngine;
using System.Collections;
using WinterMute;

public class Example2Player : MonoBehaviour
{

    public GameObject circle;
    public GameObject triangle;
    public GameObject square;
    public GameObject push;
    public GameObject pull;

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}

    void OnEnable ()
    {
        VRGestureManager.GestureDetectedEvent += OnGestureDetected;
    }

    void OnDisable ()
    {
        VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
    }

    void OnGestureDetected (string gestureName, double confidence)
    {
        //string confidenceString = confidence.ToString().Substring(0, 4);
        //Debug.Log("detected gesture: " + gestureName + " with confidence: " + confidenceString);

        switch (gestureName)
        {
            case "Circle":
                StartCoroutine(AnimateShape(circle));
                break;
            case "Square":
                StartCoroutine(AnimateShape(square));
                break;
            case "Triangle":
                StartCoroutine(AnimateShape(triangle));
                break;
            case "Push":
                StartCoroutine(AnimateShape(push));
                break;
            case "Pull":
                StartCoroutine(AnimateShape(pull));
                break;
        }
    }

    IEnumerator AnimateShape (GameObject shape)
    {
        Renderer[] renderers = shape.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = Color.red;
        }

        yield return new WaitForSeconds(.6f);

        foreach (Renderer r in renderers)
        {
            r.material.color = Color.white;
        }
    }
}
