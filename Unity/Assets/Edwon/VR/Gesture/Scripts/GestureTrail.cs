﻿using UnityEngine;
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

        bool currentlyInUse = false;

        // Use this for initialization
        void Start()
        {
            currentlyInUse = true;
            displayLine = new List<Vector3>();
            currentRenderer = CreateLineRenderer(Color.magenta, Color.magenta);
        }

        public GestureTrail Init(CaptureHand _hand)
        {
            registeredHand = _hand;
            SubscribeToEvents();
            return this;
        }

        void OnEnable()
        {
            if(registeredHand != null)
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
            Debug.Log("GESTURE TRAIL DESTROY");
            currentlyInUse = false;
        }

        LineRenderer CreateLineRenderer(Color c1, Color c2)
        {
            GameObject myGo = new GameObject("Trail Renderer");
            myGo.transform.parent = transform;
            myGo.transform.localPosition = Vector3.zero;

            LineRenderer lineRenderer = myGo.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetColors(c1, c2);
            lineRenderer.SetWidth(0.01F, 0.05F);
            lineRenderer.SetVertexCount(0);
            return lineRenderer;
        }

        public void RenderTrail(LineRenderer lineRenderer, List<Vector3> capturedLine)
        {
            if (capturedLine.Count == lengthOfLineRenderer)
            {
                lineRenderer.SetVertexCount(lengthOfLineRenderer);
                lineRenderer.SetPositions(capturedLine.ToArray());
            }
        }

        public void StartTrail()
        {
            currentRenderer.SetColors(Color.magenta, Color.magenta);
            displayLine.Clear();
        }

        public void CapturePoint(Vector3 rightHandPoint)
        {
            displayLine.Add(rightHandPoint);
            currentRenderer.SetVertexCount(displayLine.Count);
            currentRenderer.SetPositions(displayLine.ToArray());
        }

        public void CapturePoint(Vector3 myVector, List<Vector3> capturedLine, int maxLineLength)
        {
            if (capturedLine.Count >= maxLineLength)
            {
                capturedLine.RemoveAt(0);
            }
            capturedLine.Add(myVector);
        }

        public void StopTrail()
        {
            currentRenderer.SetColors(Color.blue, Color.cyan);
        }


        public void ClearTrail()
        {
            Debug.Log("clear trail");
            currentRenderer.SetVertexCount(0);
        }

        public bool UseCheck()
        {
            Debug.Log("Use Check: " + currentlyInUse);
            return currentlyInUse;
        }

        public void AssignHand(CaptureHand captureHand)
        {
            currentlyInUse = true;
            registeredHand = captureHand;
            SubscribeToEvents();

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
