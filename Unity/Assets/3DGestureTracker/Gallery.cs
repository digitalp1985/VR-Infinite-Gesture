using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace WinterMute
{
    public class Gallery : MonoBehaviour
    {
        VRGestureManager vrGestureManager;

        public GameObject framePrefab;

        public float gestureDrawSize;
        public float gridUnitSize;
        public int gridMaxColumns;
        public Vector3 frameOffset;

        // Use this for initialization
        void Start()
        {
            vrGestureManager = FindObjectOfType<VRGestureManager>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                // this returns the amount of takes recorded for one gesture
                //List<GestureExample> examples = GetGallery();
                //foreach(GestureExample example in examples)
                //{
                //    Debug.Log(example.name);
                //}

                GenerateGestureGallery();
            }
        }

        public List<GestureExample> GetGestureExamples()
        {
            //read in the file
            string filePath = Config.SAVE_FILE_PATH + vrGestureManager.currentNeuralNet + "/Gestures/";
            string fileName = vrGestureManager.Gestures[0] + ".txt";
            string[] lines = System.IO.File.ReadAllLines(filePath + fileName);
            List<GestureExample> gestures = new List<GestureExample>();
            foreach (string currentLine in lines)
            {
                gestures.Add(JsonUtility.FromJson<GestureExample>(currentLine));
            }
            return gestures;
        }

        public void GenerateGestureGallery()
        {
            List<GestureExample> examples = GetGestureExamples();

            float xPos = 0;
            float yPos = 0;
            int column = 0;
            int row = 0;

            // go through all the gesture examples and draw them in a grid
            for (int i = 0; i < examples.Count; i++)
            {
                // draw gesture at position
                float gridStartPosX = (gridUnitSize * gridMaxColumns) / 2;
                int gridMaxRows = examples.Count / gridMaxColumns;
                float gridStartPosY = (gridUnitSize * gridMaxRows) / 2;

                // offset positions to center the transform
                xPos -= gridStartPosX;
                yPos += gridStartPosY;
                Vector3 localPos = new Vector3(xPos, yPos, 0);
                
                // draw the gesture
                DrawGesture(examples[i].data, localPos, i);

                // draw the frame
                Vector3 framePos = localPos + frameOffset;
                GameObject frame = GameObject.Instantiate(framePrefab, framePos, Quaternion.identity) as GameObject;
                frame.name = "Frame " + i;
                frame.transform.parent = transform;
                frame.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gridUnitSize);
                frame.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridUnitSize);

                // set the next position
                xPos = column * gridUnitSize;
                yPos = -row * gridUnitSize;

                // change column or row
                column += 1;
                if (column >= gridMaxColumns)
                {
                    column = 0;
                    row += 1;
                }
            }
        }

        public void DrawGesture(List<Vector3> capturedLine, Vector3 startCoords, int gestureExampleNumber)
        {
            // create a game object
            //Debug.Log(startCoords);
            GameObject tmpObj = new GameObject();
            tmpObj.name = "Gesture Example " + gestureExampleNumber;
            tmpObj.transform.SetParent(transform);
            tmpObj.transform.localPosition = startCoords;

            // get the list of points in capturedLine and modify positions based on gestureDrawSize
            List<Vector3> capturedLineAdjusted = new List<Vector3>();
            foreach (Vector3 point in capturedLine)
            {
                Vector3 pointScaled = point * gestureDrawSize;
                capturedLineAdjusted.Add(pointScaled);
            }

            LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetColors(Color.red, Color.red);
            lineRenderer.SetWidth(0.1F, 0.1F);
            lineRenderer.SetVertexCount(capturedLineAdjusted.Count);
            lineRenderer.SetPositions(capturedLineAdjusted.ToArray());

        }

    }
}
