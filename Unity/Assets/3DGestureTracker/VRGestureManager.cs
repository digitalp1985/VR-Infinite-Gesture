using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using WinterMute;
using VRDebugUI;
using UnityEngine.UI;

public enum VRGestureManagerState { Idle, ReadyToRecord, Recording, Training, Detecting };

public class VRGestureManager : MonoBehaviour
{
    [HideInInspector]
    public VRGestureManagerState state;
    private VRGestureManagerState stateLast;

    public Transform vrRigAnchors;
    VRAvatar myAvatar;
    IInput rightInput;

    public Transform playerHead;
    public Transform playerHand;

    GameObject rightGo;
    GameObject currentGo;

    int lengthOfLineRenderer = 50;
    //LineRenderer rightLineRenderer;
    List<Vector3> rightCapturedLine;
    LineRenderer currentRenderer;
    List<Vector3> currentCapturedLine;

    [Tooltip ("the neural net that I am using")]
    [SerializeField]
    public string currentNeuralNet;
    [SerializeField]
    public List<string> neuralNets;
	private List<string> gestures; 	// list of gestures already trained in currentNeuralNet
	public List<string> Gestures
	{
		get
		{
			NeuralNetworkStub stub = Utils.Instance.ReadNeuralNetworkStub (currentNeuralNet);
			return stub.gestures;
		}
		set
		{
			value = gestures;
		}
	}
    public List<string> gestureBank; // list of recorded gesture for current neural net

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
        state = VRGestureManagerState.Idle;
        Debug.Log(state);
        stateLast = state;

        Debug.Log("CURRENT NEURAL NET IS: " + currentNeuralNet);
        // get current neural net from inspector
        //currentNeuralNet = 

        myAvatar = PlayerManager.GetPlayerAvatar(0);
        recording = "";

        playerHead = myAvatar.headTF;
        playerHand = myAvatar.vrRigAnchors.rHandAnchor;

        //create a new Trainer
        currentTrainer = new Trainer(Gestures, currentNeuralNet);



        //double[][] fart = myTrainer.ReadAllData();
        //Train different gestures.
        //Save it.
        //currentRecognizer = new GestureRecognizer("grobbler");
        //currentRecognizer = new GestureRecognizer(currentNeuralNet);

        rightInput = myAvatar.GetInput(VROptions.Handedness.Right);

        rightCapturedLine = new List<Vector3>();
        currentCapturedLine = new List<Vector3>();

        perpTransform = new GameObject("Perpindicular Head").transform;
        perpTransform.parent = this.transform;

        //rightLineRenderer = CreateLineRenderer(rightGo, Color.yellow, Color.red);
        currentRenderer = CreateLineRenderer(currentGo, Color.magenta, Color.magenta);
    }

    //IMPORTANT SET UP LISTENERS FOR UI
    void OnEnable()
    {
        Debug.Log("On Enable inside of VRGestureManager");
        EventManager.StartListening("ReadyToRecord", BeginReadyToRecord);
        EventManager.StartListening("BeginDetect", BeginDetect);
        //load a trainor
        //load a recognizer
    }

    void OnDisable()
    {
        EventManager.StopListening("Record", BeginReadyToRecord);
        EventManager.StopListening("BeginDetect", BeginDetect);
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
        //if (recording != "" && state == VRGestureManagerState.Recording)
        if (state == VRGestureManagerState.Recording)
        {
            TrainLine(recording, capturedLine);
        }
        else if (state == VRGestureManagerState.Detecting)
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
        if (state != stateLast)
        {
            Debug.Log(state);
        }
        stateLast = state;

        //get the position from the left anchor.
        //draw a point.
        if (myAvatar != null)
        {
            ////create a transform that will always rotate with the head but stay perp on the Y.
            //UpdatePerpTransform();
            if (state == VRGestureManagerState.ReadyToRecord || 
                state == VRGestureManagerState.Detecting ||
                state == VRGestureManagerState.Recording )
                UpdateWithButtons();
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
                if (state == VRGestureManagerState.ReadyToRecord)
                    state = VRGestureManagerState.Recording;

                //add check if currentLine is empty
                Vector3 localizedPoint = getLocalizedPoint(rightHandPoint);
                currentCapturedLine.Add(localizedPoint);
                currentRenderer.SetVertexCount(currentCapturedLine.Count);
                currentRenderer.SetPositions(currentCapturedLine.ToArray());
            }
        }
        //RenderTrail(rightLineRenderer, rightCapturedLine);

        //On Release
        if ((trigger1 < 0.5) && (currentCapturedLine.Count > 0))
        {


            LineCaught(currentCapturedLine);
            currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
            currentCapturedLine.Clear();

            if (state == VRGestureManagerState.Recording)
                state = VRGestureManagerState.ReadyToRecord;
        }

    }

    void UpdateContinual()
    {
        state = VRGestureManagerState.Recording;
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
        //RenderTrail(rightLineRenderer, rightCapturedLine);

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
            if (gestureBank.Count > 0)
                return true;
            else
                return false;
        }
    }

    public void BeginReadyToRecord(string gesture)
    {
        Debug.Log("BeginReadyToRecord in VRGestureManager: " + gesture);
        state = VRGestureManagerState.ReadyToRecord;

        recording = gesture;
    }

    public void BeginDetect(string ignoreThisString)
    {
		Debug.Log("begin detecting from this recognizer: " + currentNeuralNet);
        recording = "";
		state = VRGestureManagerState.Detecting;
        currentRecognizer = new GestureRecognizer(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void BeginTraining (Action<string> callback)
    {
        Debug.Log("Begin Training " + currentNeuralNet );
		state = VRGestureManagerState.Training;
        currentTrainer = new Trainer(gestureBank, currentNeuralNet);
        currentTrainer.TrainRecognizer();
		Debug.Log("Done Training " + currentNeuralNet );
        // finish training
		state = VRGestureManagerState.Idle;
        callback(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void EndTraining (Action<string> callback)
    {
        Debug.Log("Quit Training " + currentNeuralNet );
		state = VRGestureManagerState.Idle;
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
        // create new neural net folder
        //System.IO.Directory.CreateDirectory(neuralNetFolderLocation);
        Utils.Instance.CreateFolder(neuralNetName);
        // create a gestures folder
        Utils.Instance.CreateFolder(neuralNetName + "/Gestures/");

        neuralNets.Add(neuralNetName);
		gestures = new List<string>();
		gestureBank = new List<string>();

        // select the new neural net
		SelectNeuralNet(neuralNetName);
        Debug.Log("creating new neural net: " + neuralNetName);
    }

    [ExecuteInEditMode]
    public void DeleteNeuralNet(string neuralNetName)
    {
		// get this neural nets index so we know which net to select next
		int deletedNetIndex = neuralNets.IndexOf(neuralNetName);

		// delete the net and gestures
		Debug.Log("deleting neural net: " + neuralNetName);
        neuralNets.Remove(neuralNetName); // remove from list
        gestureBank.Clear(); // clear the gestures list
		Utils.Instance.DeleteNeuralNetFiles(neuralNetName); // delete all the files

		if (neuralNets.Count > 0)
			SelectNeuralNet(neuralNets[neuralNets.Count - 1]);
    }

	[ExecuteInEditMode]
	public void SelectNeuralNet(string neuralNetName)
	{
		Debug.Log("SELECT NET: " + neuralNetName);
		
		// Load the neural net and gestures into gesture bank
		Debug.Log("selecting neural net: " + neuralNetName);
		currentNeuralNet = neuralNetName;
		
		if (Utils.Instance.GetGestureBank(neuralNetName) != null)
		{
			gestureBank = Utils.Instance.GetGestureBank(neuralNetName);
		}
		else
		{
			gestureBank = new List<string>();
		}
		
	}

    [ExecuteInEditMode]
    public void CreateGesture(string gestureName)
    {
        Debug.Log("Create Gesture: " + gestureName);
        gestureBank.Add(gestureName);
        Utils.Instance.CreateGestureFile(gestureName, currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void DeleteGesture(string gestureName)
    {
        Debug.Log("deleting gesture: " + gestureName);
        gestureBank.Remove(gestureName);
		Utils.Instance.DeleteGestureFile(gestureName, currentNeuralNet);
    }

	List<string> gestureBankPreEdit;

	[ExecuteInEditMode]
	public void EditGestures()
	{
		Debug.Log("edit gestures");
		gestureBankPreEdit = new List<string>(gestureBank);
	}

    [ExecuteInEditMode]
    public void SaveGestures()
    {
		Debug.Log("save gestures");
		// make sorted lists of gestureFiles and gestureBank to make easier to compare
		List<string> gestureFilesSorted = new List<string>();
		gestureFilesSorted = Utils.Instance.GetGestureFiles(currentNeuralNet);
		gestureFilesSorted.Sort();
		List<string> gestureBankPostEdit = new List<string>(gestureBank);
//		gestureBankPostEdit.Sort();
//		gestureBankPreEdit.Sort();

		// if gestures were added to gestureBank
//		if (gestureBankPostEdit.Count > gestureBankPreEdit.Count)
//		{

		// compare the pre-edited list to the post-edited list
//		for(int i = 0; i < gestureBankPreEdit.Count; i++)
//		{
//			string gesture = gestureBankPreEdit[i];
//			// if the user has deleted a gesture or changed its name
//			if (!gestureBankPostEdit.Contains(gesture))
//			{
//				Debug.Log("doesn't contain");
//				// find the file to update
//				string fileToUpdate = Config.SAVE_FILE_PATH + currentNeuralNet + "/Gestures/" + gesture + ".txt";
//				// if file exists
//				if (System.IO.File.Exists(fileToUpdate))
//				{
//					// change its name
//					Utils.Instance.ChangeGestureName(gesture, gestureBankPostEdit[i], currentNeuralNet);
//				}
//				else // if file doesn't exist
//				{
//					Utils.Instance.DeleteGestureFile(gesture, currentNeuralNet);
//				}
//			}
//		}

		// if no gesture files create from list
		if (gestureFilesSorted.Count <= 0)
		{
			foreach (string gesture in gestureBankPostEdit)
			{
				Utils.Instance.CreateGestureFile(gesture, currentNeuralNet);
			}
		}

		// now compare the gesture files to the post-edited list
//		for(int i = 0; i < gestureFilesSorted.Count; i++)
//		{
//			string path = gestureFilesSorted[i];
//			string gestureFile = System.IO.Path.GetFileNameWithoutExtension(path);
//
//			// if the user has deleted a gesture or changed its name
//			if (!gestureBankPostEdit.Contains(gestureFile))
//			{
//				int index = gestureBankPostEdit.IndexOf(gestureFile);
//				Utils.Instance.ChangeGestureName(gestureFile, gestureBankPostEdit[index], currentNeuralNet);
//			}
//		}

		// now compare the post-edited list to the gesture files
		for (int i = 0; i < gestureBankPostEdit.Count; i++)
		{
			if (!gestureFilesSorted.Contains(gestureBankPostEdit[i]))
			{
				string gestureNamePreEdit = gestureBankPreEdit[i];
				string gestureNamePostEdit = gestureBankPostEdit[i];
				Utils.Instance.ChangeGestureName(gestureNamePreEdit, gestureNamePostEdit, currentNeuralNet);
			}
		}

		// if any gesture files are missing (compared to gestureBank) make them again
//		for (int i = 0; i < gestureBankPostEdit.Count; i++)
//		{
//			string path = Config.SAVE_FILE_PATH + currentNeuralNet + "/Gestures/" + gestureBankPostEdit[i] + ".txt";
//			// if file doesn't exist
//			if (!System.IO.File.Exists(path))
//			{
//				Utils.Instance.CreateGestureFile(gestureBankPostEdit[i], currentNeuralNet);
//			}
//		}

		// if a gesture was deleted from the gesture bank, delete the corresponding file
//		if (gestureBankPostEdit.Count < gestureFilesSorted.Count)
//		{
//			for (int i = 0; i < gestureFilesSorted.Count; i++)
//			{
//				string gestureInFile = System.IO.Path.GetFileNameWithoutExtension(gestureFilesSorted[i]);
//				// if the gesture file is not in the gesture bank
//				if (!gestureBankPostEdit.Contains(gestureInFile))
//				{
//					Utils.Instance.DeleteGestureFile(gestureInFile, currentNeuralNet);
//				}
//			}
//		}

		// if gesture bank name changed
        for (int i = 0; i < gestureFilesSorted.Count; i++)
        {
			// find the file and change it's name
//			Utils.Instance.ChangeGestureName(gestureNameOld, gestureNameNew, currentNeuralNet);
        }
    }


}
