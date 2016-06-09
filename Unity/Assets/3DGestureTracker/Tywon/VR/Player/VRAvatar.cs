using UnityEngine;
using System.Collections;

public class VRAvatar : MonoBehaviour {

    public VRRigAnchors vrRigAnchors;
    public Transform headTF;
    public Transform lHandTF;
    public Rigidbody lHandRB;
    public Transform rHandTF;
    public Rigidbody rHandRB;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Transform GetHand(VROptions.Handedness handedness)
    {
        if(handedness == VROptions.Handedness.Left)
        {
            return lHandTF;
        }
        else
        {
            return rHandTF;
        }
    }

    public IInput GetInput(VROptions.Handedness handedness)
    {
        if (handedness == VROptions.Handedness.Left)
        {
            if (lHandTF != null)
                return lHandTF.GetComponent<IInput>();

            return null;
        }
        if (handedness == VROptions.Handedness.Right)
        {
            if (rHandTF != null)
                return rHandTF.GetComponent<IInput>();

            return null;
        }
        else
            return null;
    }
}
