using UnityEngine;
using System.Collections;
using WinterMute;

public class Example3Player : MonoBehaviour
{
    public GameObject fire;

    VRAvatar myAvatar;
    IInput input;

    Transform playerHead;
    Transform playerHandL;
    Transform playerHandR;

    void Start ()
    {
        myAvatar = PlayerManager.GetPlayerAvatar(0);

        playerHead = myAvatar.headTF;
        playerHandR = myAvatar.vrRigAnchors.rHandAnchor;
        playerHandL = myAvatar.vrRigAnchors.lHandAnchor;

        if (Config.handedness == Config.Handedness.Right)
        {
            input = myAvatar.GetInput(VROptions.Handedness.Right);
        }
        else if (Config.handedness == Config.Handedness.Left)
        {
            input = myAvatar.GetInput(VROptions.Handedness.Left);
        }
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
            case "Fire":
                DoFire();
                break;
            case "Earth":
                DoEarth();
                break;
            case "Ice":
                DoIce();
                break;
            case "Air":
                DoAir();
                break;
        }
    }

    void DoFire ()
    {
        Quaternion rotation = Quaternion.LookRotation(playerHandR.forward, Vector3.up);
        Vector3 betweenHandsPos = (playerHandL.position + playerHandR.position) / 2;
        GameObject.Instantiate(fire, betweenHandsPos, rotation);
    }

    void DoEarth ()
    {

    }

    void DoIce()
    {

    }

    void DoAir()
    {

    }

    IEnumerator AnimateShape (GameObject shape)
    {
        yield return null;
    }
}
