using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineCapture : MonoBehaviour {

    public Transform vrRigAnchors;
    Avatar myAvatar;
    IInput rightInput;
    IInput leftInput;

    GameObject rightGo;
    GameObject leftGo;
    GameObject currentGo;

    public LineRenderer rightLineRenderer;
    public LineRenderer leftLineRenderer;
    public LineRenderer currentRenderer;

    public int lengthOfLineRenderer = 50;
    public int lastPos = 0;

    public List<Vector3> rightCapturedLine;
    public List<Vector3> leftCapturedLine;
    public List<Vector3> currentCapturedLine;

    public List<GameObject> renderPool;
    Transform perpTransform;

    bool isDrawing;

    float nextRenderTime = 0;
    float renderRateLimit = 10;

    TrainingDataFileWriter myDataDump;

    // Use this for initialization
    void Start () {

        myAvatar = PlayerManager.GetPlayerAvatar(0);

        myDataDump = new TrainingDataFileWriter();

        rightInput = myAvatar.GetInput( VROptions.Handedness.Right);
        leftInput = myAvatar.GetInput(VROptions.Handedness.Right);

        isDrawing = false;

        renderPool = new List<GameObject>();

        rightCapturedLine = new List<Vector3>();
        leftCapturedLine = new List<Vector3>();

        perpTransform = new GameObject("Perpindicular Head").transform;
        perpTransform.parent = this.transform;

        rightLineRenderer = CreateLineRenderer( rightGo, Color.yellow, Color.red);
        leftLineRenderer = CreateLineRenderer( leftGo,Color.cyan, Color.blue);
        currentRenderer = CreateLineRenderer(currentGo, Color.magenta, Color.magenta);
    }

    LineRenderer CreateLineRenderer(GameObject myGo, Color c1, Color c2)
    {
        myGo = new GameObject();
        myGo.transform.parent = transform;
        myGo.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = myGo.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(c1, c2);
        lineRenderer.SetWidth(0.01F, 0.05F);
        lineRenderer.SetVertexCount(0);
        //lineRenderer.SetPositions();
        return lineRenderer;
    }

    public void DrawLine(List<Vector3> capturedLine)
    {
        myDataDump.writeData(capturedLine);

        GameObject tmpObj = new GameObject();
        tmpObj.transform.parent = transform;
        tmpObj.transform.localPosition = Vector3.zero;
        renderPool.Add(tmpObj);

        LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.01F, 0.05F);
        lineRenderer.SetVertexCount(capturedLine.Count);
        lineRenderer.SetPositions(capturedLine.ToArray());
    }

    // Update is called once per frame
    void Update () {
        //get the position from the left anchor.
        //draw a point.
        if (myAvatar != null)
        {
            //Debug.Log(rightInput.GetAxis1D(InputOptions.Axis1D.Trigger1));
            //create a transform that will always rotate with the head but stay perp on the Y.
            Transform currentHeadTransform = myAvatar.headTF;
            perpTransform.position = currentHeadTransform.position;
            perpTransform.rotation = Quaternion.Euler(0, currentHeadTransform.eulerAngles.y, 0);

            Debug.DrawRay(perpTransform.position, perpTransform.up, Color.green);
            Debug.DrawRay(perpTransform.position, perpTransform.right, Color.red);
            Debug.DrawRay(perpTransform.position, perpTransform.forward, Color.blue);

            if (Time.time > nextRenderTime)
            {
                Vector3 rightHandPoint = myAvatar.vrRigAnchors.rHandAnchor.position;
                Vector3 leftHandPoint = myAvatar.vrRigAnchors.lHandAnchor.position;

                nextRenderTime = Time.time + renderRateLimit / 1000;
                CapturePoint(rightHandPoint, rightCapturedLine);
                CapturePoint(leftHandPoint, leftCapturedLine);
                if (rightInput.GetAxis1D(InputOptions.Axis1D.Trigger1) >= 0.5)
                {
                    //add check if currentLine is empty
                    Vector3 localizedPoint = getLocalizedPoint(rightHandPoint);
                    currentCapturedLine.Add(localizedPoint);
                    currentRenderer.SetVertexCount(currentCapturedLine.Count);
                    //currentRenderer.SetPositions(currentCapturedLine.ToArray());
                }
            }
  
            //On Release
            if ((rightInput.GetAxis1D(InputOptions.Axis1D.Trigger1) < 0.5) && (currentCapturedLine.Count > 0))
            {
                DrawLine(currentCapturedLine);
                currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
            }

            RenderTrail(rightLineRenderer, rightCapturedLine);
            RenderTrail(leftLineRenderer, leftCapturedLine);

        }

    }

    public Vector3 getLocalizedPoint(Vector3 myDumbPoint)
    {
        return perpTransform.InverseTransformPoint(myDumbPoint);
    }

    public void CapturePoint(Vector3 myVector, List<Vector3> capturedLine)
    {
        if (capturedLine.Count >= lengthOfLineRenderer)
        {
            capturedLine.RemoveAt(0);
        }
        capturedLine.Add(myVector);
    }

   

    void RenderTrail( LineRenderer lineRenderer, List<Vector3> capturedLine)
    {
        //LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if(capturedLine.Count == lengthOfLineRenderer)
        {
            lineRenderer.SetVertexCount(lengthOfLineRenderer);
            lineRenderer.SetPositions(capturedLine.ToArray());
        }
    }


}
