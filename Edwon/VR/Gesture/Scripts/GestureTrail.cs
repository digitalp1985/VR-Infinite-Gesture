using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Edwon.VR.Gesture
{

    public class GestureTrail : MonoBehaviour
    {
        CaptureHand registeredHand;
        int lengthOfLineRenderer = 50;
        List<Vector3> displayLine;
        LineRenderer currentRenderer;

        Vector3 Hoffset;

        public bool listening = false;

        bool currentlyInUse = false;

        // Use this for initialization
        void Start()
        {
            currentlyInUse = true;
            displayLine = new List<Vector3>();
            currentRenderer = CreateLineRenderer(Color.magenta, Color.magenta);
        }

        void OnEnable()
        {
            if (registeredHand != null)
            {
                SubscribeToEvents();
            }
        }

        void SubscribeToEvents()
        {
            registeredHand.StartCaptureEvent += StartTrail;
            registeredHand.ContinueCaptureEvent += CapturePoint;
            registeredHand.StopCaptureEvent += StopTrail;
        }

        void OnDisable()
        {
            if (registeredHand != null)
            {
                UnsubscribeFromEvents();
            }
        }

        void UnsubscribeFromEvents()
        {
            registeredHand.StartCaptureEvent -= StartTrail;
            registeredHand.ContinueCaptureEvent -= CapturePoint;
            registeredHand.StopCaptureEvent -= StopTrail;
        }

        void UnsubscribeAll()
        {

        }

        void OnDestroy()
        {
            currentlyInUse = false;
        }

        public void setOffset(Vector3 input)
        {
            Hoffset = input;
        }

        LineRenderer CreateLineRenderer(Color c1, Color c2)
        {
            GameObject myGo = new GameObject("Trail Renderer");
            myGo.transform.parent = transform;

            //Modified to add h-offset- digitalp2k
            if (Hoffset == null)
            {
                myGo.transform.localPosition = Vector3.zero;
            }
            else { myGo.transform.localPosition = Hoffset; }
            LineRenderer lineRenderer = myGo.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            //lineRenderer.SetColors(c1, c2); -original code revised due to obsolete method -DP2K
            lineRenderer.startColor = c1;
            lineRenderer.endColor = c2;
            //lineRenderer.SetWidth(0.01F, 0.05F); -original code revised due to obsolete method -DP2K
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.05f;
            //lineRenderer.SetVertexCount(0); -original code revised due to obsolete method -DP2K
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false;
            return lineRenderer;
        }

        public void StartTrail()
        {
            //currentRenderer.SetColors(Color.magenta, Color.magenta); -original code revised due to obsolete method -DP2K
            currentRenderer.startColor = Color.magenta;
            currentRenderer.endColor = Color.magenta;
            displayLine.Clear();
            listening = true;
        }

        public void CapturePoint(Vector3 handPoint)
        {
            //display line appears to be made up of World Points instead of localized ones.
            displayLine.Add(handPoint);
            //currentRenderer.SetVertexCount(displayLine.Count); -original code revised due to obsolete method -DP2K
            currentRenderer.positionCount = displayLine.Count;
            currentRenderer.SetPositions(displayLine.ToArray());
        }

        public void StopTrail()
        {
            //currentRenderer.SetColors(Color.blue, Color.cyan); -original code revised due to obsolete method -DP2K
            currentRenderer.startColor = Color.blue;
            currentRenderer.endColor = Color.cyan;
            listening = false;
        }

        public void ClearTrail()
        {
            //currentRenderer.SetVertexCount(0); -original code revised due to obsolete method -DP2K
            currentRenderer.positionCount = 0;
        }

        public bool UseCheck()
        {
            return currentlyInUse;
        }

        public void AssignHand(CaptureHand captureHand)
        {
            currentlyInUse = true;
            registeredHand = captureHand;
            SubscribeToEvents();

        }

        //adding external hooks into trail so that there is a visual queue when a gesture is detected.

        public void SuccessIndicator()
        {
            Debug.Log("Indicator Initiated");
            currentRenderer.startColor = Color.yellow;
            currentRenderer.endColor = Color.yellow;
        }

        public void FailureIndicator()
        {
            currentRenderer.startColor = Color.red;
            currentRenderer.endColor = Color.red;

        }

    }
}
