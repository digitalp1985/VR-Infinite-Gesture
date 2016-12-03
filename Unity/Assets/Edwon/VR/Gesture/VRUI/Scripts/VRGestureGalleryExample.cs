using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Edwon.VR.Gesture
{
    public class VRGestureGalleryExample : MonoBehaviour
    {

        public CanvasGroup trash;

        public VRGestureGalleryGrid grid;

        public Button button;

        public GestureExample example;
        public int lineNumber;

        public GameObject lineDrawing;

        public void Init(VRGestureGalleryGrid _grid, GestureExample _example, int _lineNumber)
        {
            grid = _grid;
            example = _example;
            lineNumber = _lineNumber;

            Vector3 localPos = Vector3.zero;

            // draw the gesture
            lineDrawing = DrawGesture(example.data, localPos, _lineNumber);

            button.onClick.AddListener(() => grid.gallery.DeleteGestureExample(example, lineNumber));

            // set the trash icon position
            RectTransform trashTF = (RectTransform)trash.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            trashTF.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, grid.gallery.gridUnitSize * 2);
            trashTF.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, grid.gallery.gridUnitSize * 2);
        }

        public void OnPointerEnter()
        {
            VRGestureUI.ToggleCanvasGroup(trash, false, 1);
        }

        public void OnPointerExit()
        {
            VRGestureUI.ToggleCanvasGroup(trash, false, 0);
        }

        public GameObject DrawGesture(List<Vector3> capturedLine, Vector3 startCoords, int gestureExampleNumber)
        {
            // create a game object
            //Debug.Log(startCoords);
            GameObject tmpObj = new GameObject();
            tmpObj.name = "Gesture Example " + gestureExampleNumber;
            tmpObj.transform.SetParent(transform);
            tmpObj.transform.localPosition = startCoords;
            tmpObj.transform.forward = -transform.forward;

            // get the list of points in capturedLine and modify positions based on gestureDrawSize
            List<Vector3> capturedLineAdjusted = new List<Vector3>();
            foreach (Vector3 point in capturedLine)
            {
                Vector3 pointScaled = point * grid.gallery.gestureDrawSize;
                capturedLineAdjusted.Add(pointScaled);
            }

            LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            //lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetColors(Color.blue, Color.green);
            //Add a taper to the line
            lineRenderer.SetWidth
                (
                    grid.gallery.lineWidth - (grid.gallery.lineWidth * .5f), 
                    grid.gallery.lineWidth + (grid.gallery.lineWidth * .5f)
                );
            lineRenderer.SetVertexCount(capturedLineAdjusted.Count);
            lineRenderer.SetPositions(capturedLineAdjusted.ToArray());

            return tmpObj;
        }

    }
}