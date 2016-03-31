using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WinterMute;
using VRDebugUI;

public class LineCapture : MonoBehaviour
{

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

    public List<string> gestureList;

    public List<GameObject> renderPool;
    Transform perpTransform;

    bool isDrawing;
    string recording;

    float nextRenderTime = 0;
    float renderRateLimit = 30;

    TrainingDataFileWriter myDataDump;

    int numInput = 33; // number features
    int numHidden = 10;
    int numOutput = 3; // number of classes for Y
    NeuralNetwork nn;

    Trainer myTrainer;
    GestureRecognizer myRecognizer;

    // DEBUG
    string debugString;

    // Use this for initialization
    void Start()
    {

        myAvatar = PlayerManager.GetPlayerAvatar(0);

        DebugMethods.RecordMode += BeginRecord;
        DebugMethods.DetectMode += BeginDetect;
        recording = "";


        List<string> gestureList = new List<string>();
        gestureList.Add("rainbow");
        gestureList.Add("line");
        gestureList.Add("horz");

        myDataDump = new TrainingDataFileWriter();
        myTrainer = new Trainer(gestureList);
        myRecognizer = new GestureRecognizer(11, gestureList);


        rightInput = myAvatar.GetInput(VROptions.Handedness.Right);
        leftInput = myAvatar.GetInput(VROptions.Handedness.Right);

        nn = new NeuralNetwork(numInput, numHidden, numOutput);
        double[] weights = myDataDump.GetWeights();
        nn.SetWeights(weights);

        isDrawing = false;

        renderPool = new List<GameObject>();

        rightCapturedLine = new List<Vector3>();
        leftCapturedLine = new List<Vector3>();

        perpTransform = new GameObject("Perpindicular Head").transform;
        perpTransform.parent = this.transform;

        rightLineRenderer = CreateLineRenderer(rightGo, Color.yellow, Color.red);
        leftLineRenderer = CreateLineRenderer(leftGo, Color.cyan, Color.blue);
        currentRenderer = CreateLineRenderer(currentGo, Color.magenta, Color.magenta);

        //buildGallery();
    }

    void BeginRecord(string gesture)
    {
        recording = gesture;
    }

    void BeginDetect(string gesture)
    {
        recording = "";
    }

    public void buildGallery()
    {
        List<GestureExample> galleryList = myDataDump.getGallery();

        float x = 0;
        foreach (GestureExample gesture in galleryList)
        {
            Vector3 hang = new Vector3(x, 1, x);
            DrawPortrait(gesture.data, hang);
            x += .2f;
        }
    }

    public void DrawPortrait(List<Vector3> capturedLine, Vector3 startCoords)
    {
        Debug.Log(startCoords);
        GameObject tmpObj = new GameObject();
        tmpObj.transform.SetParent(transform);
        tmpObj.transform.position = startCoords;

        List<Vector3> tmpArray = new List<Vector3>();
        foreach (Vector3 currentPoint in capturedLine)
        {
            tmpArray.Add(tmpObj.transform.InverseTransformPoint(currentPoint));
        }

        //renderPool.Add(tmpObj);

        LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.1F, 0.1F);
        lineRenderer.SetVertexCount(tmpArray.Count);
        lineRenderer.SetPositions(tmpArray.ToArray());

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


    public void TestNeural(List<Vector3> capturedLine)
    {
        //Get Weights
        double[] input = Utils.Instance.FormatLine(capturedLine); ;
        double[] output = nn.ComputeOutputs(input);

        string gesture = myRecognizer.GetGestureFromVector(output);
        debugString = gesture;
    }


    public void LineCaught(List<Vector3> capturedLine)
    {
        //DrawLine(capturedLine);
        if (recording != "")
        {
            TrainLine(recording, capturedLine);
        }
        else
        {
            TestNeural(capturedLine);
        }
    }

    public void TrainLine(string gesture, List<Vector3> capturedLine)
    {
        myTrainer.AddGestureToTrainingExamples(gesture, capturedLine);
        debugString = "trained : " + gesture;
    }

    public void DrawLine(List<Vector3> capturedLine)
    {
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
    void Update()
    {
        //get the position from the left anchor.
        //draw a point.
        if (myAvatar != null)
        {
            float trigger1 = rightInput.GetAxis1D(InputOptions.Axis1D.Trigger1);

            //create a transform that will always rotate with the head but stay perp on the Y.
            UpdatePerpTransform();

            if (Time.time > nextRenderTime)
            {
                Vector3 rightHandPoint = myAvatar.vrRigAnchors.rHandAnchor.position;
                Vector3 leftHandPoint = myAvatar.vrRigAnchors.lHandAnchor.position;

                nextRenderTime = Time.time + renderRateLimit / 1000;
                CapturePoint(rightHandPoint, rightCapturedLine);
                CapturePoint(leftHandPoint, leftCapturedLine);
                if (trigger1 >= 0.5)
                {
                    //add check if currentLine is empty
                    Vector3 localizedPoint = getLocalizedPoint(rightHandPoint);
                    currentCapturedLine.Add(localizedPoint);
                    currentRenderer.SetVertexCount(currentCapturedLine.Count);
                    currentRenderer.SetPositions(currentCapturedLine.ToArray());
                }
            }

            //On Release
            if ((trigger1 < 0.5) && (currentCapturedLine.Count > 0))
            {
                LineCaught(currentCapturedLine);
                currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
            }

            RenderTrail(rightLineRenderer, rightCapturedLine);
            RenderTrail(leftLineRenderer, leftCapturedLine);

        }

        // VR DEBUG UI
        DebugHud.Log(debugString);


    }

    public void UpdatePerpTransform()
    {
        Transform currentHeadTransform = myAvatar.headTF;
        perpTransform.position = currentHeadTransform.position;
        perpTransform.rotation = Quaternion.Euler(0, currentHeadTransform.eulerAngles.y, 0);

        Debug.DrawRay(perpTransform.position, perpTransform.up, Color.green);
        Debug.DrawRay(perpTransform.position, perpTransform.right, Color.red);
        Debug.DrawRay(perpTransform.position, perpTransform.forward, Color.blue);
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

    void RenderTrail(LineRenderer lineRenderer, List<Vector3> capturedLine)
    {
        //LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (capturedLine.Count == lengthOfLineRenderer)
        {
            lineRenderer.SetVertexCount(lengthOfLineRenderer);
            lineRenderer.SetPositions(capturedLine.ToArray());
        }
    }


}
