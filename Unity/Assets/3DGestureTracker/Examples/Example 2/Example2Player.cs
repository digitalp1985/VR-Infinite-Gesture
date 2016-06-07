using UnityEngine;
using System.Collections;
using WinterMute;

public class Example2Player : MonoBehaviour
{

    public GameObject circle;
    public GameObject triangle;
    public GameObject square;
    public GameObject push;
    public GameObject pushLeft;
    public GameObject pushRight;
    public GameObject pull;
    public GameObject nullGO;

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}

    void OnEnable ()
    {
        VRGestureManager.GestureDetectedEvent += OnGestureDetected;
        VRGestureManager.GestureNullEvent += OnGestureNull;
    }

    void OnDisable ()
    {
        VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
        VRGestureManager.GestureNullEvent -= OnGestureNull;
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
            case "Push Left":
                StartCoroutine(AnimateShape(pushLeft));
                break;
            case "Push Right":
                StartCoroutine(AnimateShape(pushRight));
                break;
            case "Pull":
                StartCoroutine(AnimateShape(pull));
                break;
        }
    }

    void OnGestureNull ()
    {
        StartCoroutine(AnimateShape(nullGO));
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
