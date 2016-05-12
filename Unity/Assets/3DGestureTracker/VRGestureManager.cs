using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WinterMute;
using VRDebugUI;
using UnityEngine.UI;

public class VRGestureManager : MonoBehaviour
{
    public Transform vrRigAnchors;
    VRAvatar myAvatar;
    IInput rightInput;

    public Transform playerHead;
    public Transform playerHand;

    GameObject rightGo;
    GameObject currentGo;

    int lengthOfLineRenderer = 50;
    LineRenderer rightLineRenderer;
    List<Vector3> rightCapturedLine;
    LineRenderer currentRenderer;
    List<Vector3> currentCapturedLine;

    [Tooltip ("the neural net that I am using")]
    public string currentNeuralNet;
    [SerializeField]
    public List<string> neuralNets;
    public List<string> gestures;
    [SerializeField]
    public Dictionary< string, List<string> > allGestures;

    Transform perpTransform;

    string recording;

    float nextRenderTime = 0;
    float renderRateLimit = 30;
    float nextTestTime = 0;
    float testRateLimit = 500;

    Trainer currentTrainer;
    GestureRecognizer currentRecognizer;

    // DEBUG
    public string debugString;

    void Start()
    {
        myAvatar = PlayerManager.GetPlayerAvatar(0);
        recording = "";

        playerHead = myAvatar.headTF;
        playerHand = myAvatar.vrRigAnchors.rHandAnchor;

        //create a new Trainer
        currentTrainer = new Trainer(gestures, "grobbler");

        //double[][] fart = myTrainer.ReadAllData();
        //Train different gestures.
        //Save it.
        //myRecognizer = new GestureRecognizer("puni");

        rightInput = myAvatar.GetInput(VROptions.Handedness.Right);

        rightCapturedLine = new List<Vector3>();
        currentCapturedLine = new List<Vector3>();

        perpTransform = new GameObject("Perpindicular Head").transform;
        perpTransform.parent = this.transform;

        rightLineRenderer = CreateLineRenderer(rightGo, Color.yellow, Color.red);
        currentRenderer = CreateLineRenderer(currentGo, Color.magenta, Color.magenta);
    }

    //IMPORTANT SET UP LISTENERS FOR UI
    void OnEnable()
    {
        Debug.Log("On Enable inside of VRGestureManager");
        EventManager.StartListening("Record", BeginRecord);
        EventManager.StartListening("Detect", BeginDetect);
        //load a trainor
        //load a recognizer
    }

    void OnDisable()
    {
        EventManager.StopListening("Record", BeginRecord);
        EventManager.StopListening("Detect", BeginDetect);
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

    public void LineCaught(List<Vector3> capturedLine)
    {
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
        currentTrainer.AddGestureToTrainingExamples(gesture, capturedLine);
        debugString = "trained : " + gesture;
    }

    public void TestNeural(List<Vector3> capturedLine)
    {
        double[] input = Utils.Instance.FormatLine(capturedLine);
        string gesture = currentRecognizer.GetGesture(input);
        debugString = gesture;
    }

    // Update is called once per frame
    void Update()
    {
        //get the position from the left anchor.
        //draw a point.
        if (myAvatar != null)
        {
            ////create a transform that will always rotate with the head but stay perp on the Y.
            //UpdatePerpTransform();
            ////UpdateWithButtons();
            //UpdateContinual();
        }
    }

    void UpdateWithButtons()
    {
        float trigger1 = rightInput.GetAxis1D(InputOptions.Axis1D.Trigger1);

        if (Time.time > nextRenderTime)
        {
            Vector3 rightHandPoint = playerHand.position;

            nextRenderTime = Time.time + renderRateLimit / 1000;
            CapturePoint(rightHandPoint, rightCapturedLine, lengthOfLineRenderer);
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
    }

    void UpdateContinual()
    {
        if (Time.time > nextRenderTime)
        {
            Vector3 rightHandPoint = playerHand.position;

            nextRenderTime = Time.time + renderRateLimit / 1000;
            CapturePoint(rightHandPoint, rightCapturedLine, lengthOfLineRenderer);

            //IF currentCapturedLine is length greater than renderRateLimit v testRateLimit
            //  30 / 1000 = every 0.03 seconds
            // 100 / 1000 = every 0.10 seconds this will have only logged 3 points of data.
            // 500 / 1000 = every 0.5 second this will always have 16 points of data. 
            int maxLineLength = (int)testRateLimit / (int)renderRateLimit;
            CapturePoint(getLocalizedPoint(rightHandPoint), currentCapturedLine, maxLineLength);
        }
        RenderTrail(rightLineRenderer, rightCapturedLine);

        //On Release
        //@TODO: fix this magic number 14.
        if (Time.time > nextTestTime && currentCapturedLine.Count > 14)
        {
            nextTestTime = Time.time + testRateLimit / 1000;
            LineCaught(currentCapturedLine);
            currentRenderer.SetVertexCount(currentCapturedLine.Count);
            currentRenderer.SetPositions(currentCapturedLine.ToArray());
        }

        
    }

    //Important
    //This will get points in relation to a users head.
    public Vector3 getLocalizedPoint(Vector3 myDumbPoint)
    {
        perpTransform.position = playerHead.position;
        perpTransform.rotation = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);
        return perpTransform.InverseTransformPoint(myDumbPoint);
    }

    public void CapturePoint(Vector3 myVector, List<Vector3> capturedLine, int maxLineLength)
    {
        if (capturedLine.Count >= maxLineLength)
        {
            capturedLine.RemoveAt(0);
        }
        capturedLine.Add(myVector);
    }

    //Render Trails, maybe this is an optional feature of a line capture.
    void RenderTrail(LineRenderer lineRenderer, List<Vector3> capturedLine)
    {
        //LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (capturedLine.Count == lengthOfLineRenderer)
        {
            lineRenderer.SetVertexCount(lengthOfLineRenderer);
            lineRenderer.SetPositions(capturedLine.ToArray());
        }
    }

    // below here is new custom editor stuff that edwon's making
    // mostly dummy stuff that doesn't do anything yet
    // needs connecting to real stuff by Tyler

    public bool readyToTrain
    {
        get
        {
            if (gestures.Count > 0)
                return true;
            else
                return false;
        }
    }

    public bool isTraining = false;

    void BeginRecord(string gesture)
    {
        Debug.Log("Actually Recording");
        recording = gesture;
    }

    void BeginDetect(string recognizer)
    {
        recording = "";
        currentRecognizer = new GestureRecognizer(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void SaveGestures()
    {
//		Debug.Log("save gestures");
    }

    [ExecuteInEditMode]
    public void BeginTraining (Action<string> callback)
    {
        Debug.Log("Begin Training " + currentNeuralNet );
        isTraining = true;
        currentTrainer = new Trainer(gestures, currentNeuralNet);
        currentTrainer.TrainRecognizer();
        callback(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void EndTraining (Action<string> callback)
    {
        Debug.Log("Quit Training " + currentNeuralNet );
        isTraining = false;
        callback(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public bool CheckForDuplicateNeuralNetName(string neuralNetName)
    {
        // if neuralNetName already exists return true
        if (neuralNets.Contains(neuralNetName))
            return true;
        else
            return false;
    }

    [ExecuteInEditMode]
    public void CreateNewNeuralNet(string neuralNetName)
    {
        neuralNets.Add(neuralNetName);
        AddGestureListToAllGestures(neuralNetName);
        Debug.Log("creating new neural net: " + neuralNetName);
    }

    [ExecuteInEditMode]
    public void DeleteNeuralNet(string neuralNetName)
    {
        neuralNets.Remove(neuralNetName);
        RemoveGestureListFromAllGestures(neuralNetName);
        if (allGestures == null)
            return;
        
        if (allGestures.ContainsKey(neuralNetName))
        {
            allGestures.Remove(neuralNetName);
            Debug.Log("deleting neural network: " + neuralNetName);
        }
    }

    [ExecuteInEditMode]
    public void SelectNeuralNet (string neuralNetName)
    {

        Debug.Log("selecting neural net: " + neuralNetName);
        // set the current neural net
        currentNeuralNet = neuralNetName;
        //WE LOAD OUT NEURAL NETWORK
        NeuralNetworkStub stub = Utils.Instance.ReadNeuralNetworkStub(neuralNetName);
        gestures = stub.gestures;

        // set the current gestures list
//		if (allGestures.ContainsKey(neuralNetName))
//		{
//			gestures = allGestures[neuralNetName];
//		}

    }

    // all gestures list utilities

    void AddGestureListToAllGestures (string neuralNetName)
    {
        if (allGestures == null)
            InitializeAllGestures(neuralNetName);
        else
            allGestures.Add(neuralNetName, new List<string>());
    }

    void RemoveGestureListFromAllGestures (string neuralnetName)
    {
//		allGestures.Remove(neuralnetName);
    }

    void InitializeAllGestures (string neuralNetName)
    {
        allGestures = new Dictionary<string, List<string>>()
        {
            {neuralNetName, new List<string>() }
        };
    }

}
