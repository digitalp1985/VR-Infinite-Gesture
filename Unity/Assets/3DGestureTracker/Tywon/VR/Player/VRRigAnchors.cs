using UnityEngine;
using System.Collections;

public class VRRigAnchors
{
    // anchors for various points on the VR rig
    public Transform lHandAnchor;
    public Transform rHandAnchor;
    public Transform mixedRealityCamAnchor;
    public Camera cam;
    public Transform centerEyeAnchor; // camera and centerEyeAnchor may not always be on the same transform
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
}

