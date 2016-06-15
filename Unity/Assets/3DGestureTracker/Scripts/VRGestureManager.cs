using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR.Input;

namespace Edwon.VR.Gesture
{

    public enum VRGestureManagerState { Idle, Edit, Editing, EnteringRecord, ReadyToRecord, Recording, Training, ReadyToDetect, Detecting };
    public enum VRGestureDetectType { Button, Continious };

    public class VRGestureManager : MonoBehaviour
    {

        // SINGLETON INSTANCE
        static VRGestureManager instance;

        #region SETTINGS
        // the type of vr to use
        public VRTYPE vrType;
        // which hand to track
        [Tooltip("which hand to track using the gesture")]
        public HandType gestureHand = HandType.Right; // the hand to track
        [Tooltip("the threshold over wich a gesture is considered correctly classified")]
        public double confidenceThreshold = 0.98;
        // whether to track when pressing trigger or all the time
        // continious mode is not supported yet
        // though you're welcome to try it out
        [HideInInspector]
        public VRGestureDetectType vrGestureDetectType;
        #endregion
        #region STATE
        [HideInInspector]
        public VRGestureManagerState state;
        [HideInInspector]
        public VRGestureManagerState stateInitial;
        private VRGestureManagerState stateLast;
        #endregion

        public VRGestureRig rig;
        IInput input;

        #region AVATAR
        Transform playerHead;
        Transform playerHand;
        Transform perpTransform;
        #endregion

        #region LINE RENDERER
        int lengthOfLineRenderer = 50;
        LineRenderer rightLineRenderer;
        List<Vector3> rightCapturedLine;
        List<Vector3> displayLine;
        LineRenderer currentRenderer;
        List<Vector3> currentCapturedLine;
        #endregion

        #region NEURAL NETS
        [Tooltip("the neural net that I am using")]
        [SerializeField]
        public string currentNeuralNet;
        [SerializeField]
        public List<string> neuralNets;
        private List<string> gestures;  // list of gestures already trained in currentNeuralNet
        public List<string> Gestures
        {
            get
            {
                NeuralNetworkStub stub = Utils.Instance.ReadNeuralNetworkStub(currentNeuralNet);
                return stub.gestures;
            }
            set
            {
                value = gestures;
            }
        }
        public List<string> gestureBank; // list of recorded gesture for current neural net
        #endregion

        public string gestureToRecord;

        float nextRenderTime = 0;
        float renderRateLimit = Config.CAPTURE_RATE;
        float nextTestTime = 0;
        float testRateLimit = 500;

        Trainer currentTrainer;
        GestureRecognizer currentRecognizer;

        // DEBUG
        public string debugString;

        // EVENTS
        public delegate void GestureDetected(string gestureName, double confidence);
        public static event GestureDetected GestureDetectedEvent;
        public delegate void GestureNull();
        public static event GestureNull GestureNullEvent;

        public static VRGestureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<VRGestureManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        instance = obj.AddComponent<VRGestureManager>();
                    }
                    instance.Init();
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            if (instance == null)
            {
                instance = this;
                instance.Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Init()
        {
            rig = FindObjectOfType<VRGestureRig>();
            playerHead = rig.headTF;
            playerHand = rig.rHandTF;
        }

        void Start()
        {
            if (stateInitial == VRGestureManagerState.ReadyToDetect)
                BeginDetect("");

            state = stateInitial;
            stateLast = state;
            gestureToRecord = "";

            //create a new Trainer
            currentTrainer = new Trainer(Gestures, currentNeuralNet);

            input = rig.GetInput(VRGestureManager.Instance.gestureHand);

            rightCapturedLine = new List<Vector3>();
            displayLine = new List<Vector3>();
            currentCapturedLine = new List<Vector3>();

            perpTransform = new GameObject("Perpindicular Head").transform;
            perpTransform.parent = this.transform;

            rightLineRenderer = CreateLineRenderer(Color.yellow, Color.red);
            currentRenderer = CreateLineRenderer(Color.magenta, Color.magenta);
        }

        LineRenderer CreateLineRenderer(Color c1, Color c2)
        {
            GameObject myGo = new GameObject();
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
            //Debug.Log("Line Caught");
            //Debug.Log(state);
            if (state == VRGestureManagerState.Recording || state == VRGestureManagerState.ReadyToRecord)
            {

                TrainLine(gestureToRecord, capturedLine);
            }
            else if (state == VRGestureManagerState.Detecting || state == VRGestureManagerState.ReadyToDetect)
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
            double[] networkInput = Utils.Instance.FormatLine(capturedLine);
            string gesture = currentRecognizer.GetGesture(networkInput);
            string confidenceValue = currentRecognizer.currentConfidenceValue.ToString().Substring(0, 4);

            // broadcast gesture detected event
            if (currentRecognizer.currentConfidenceValue > VRGestureManager.Instance.confidenceThreshold)
            {
                debugString = gesture + " " + confidenceValue;
                if (GestureDetectedEvent != null)
                    GestureDetectedEvent(gesture, currentRecognizer.currentConfidenceValue);
            }
            else
            {
                debugString = "Null \n" + gesture + " " + confidenceValue;
                if (GestureNullEvent != null)
                    GestureNullEvent();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (state != stateLast)
            {
                //Debug.Log(state);
            }
            stateLast = state;

            //get the position from the left anchor.
            //draw a point.
            if (rig != null)
            {
                if (state == VRGestureManagerState.ReadyToRecord ||
                    state == VRGestureManagerState.EnteringRecord ||
                    state == VRGestureManagerState.Recording)
                {
                    UpdateRecord();
                }
                else if (state == VRGestureManagerState.Detecting ||
                            state == VRGestureManagerState.ReadyToDetect)
                {
                    if (VRGestureManager.Instance.vrGestureDetectType == VRGestureDetectType.Continious)
                    {
                        UpdateContinual();
                    }
                    else
                    {
                        UpdateDetectWithButtons();
                    }
                }
            }
        }

        void UpdateRecord()
        {
            if (input.GetButtonUp(InputOptions.Button.Trigger1))
            {
                state = VRGestureManagerState.ReadyToRecord;
                StopRecording();
            }

            if (input.GetButtonDown(InputOptions.Button.Trigger1) && state == VRGestureManagerState.ReadyToRecord)
            {
                state = VRGestureManagerState.Recording;
                StartRecording();
            }

            if (state == VRGestureManagerState.Recording)
            {
                CapturePoint();
            }
        }

        void UpdateDetectWithButtons()
        {
            if (input.GetButtonUp(InputOptions.Button.Trigger1))
            {
                state = VRGestureManagerState.ReadyToDetect;
                StopRecording();
            }

            if (input.GetButtonDown(InputOptions.Button.Trigger1))
            {
                state = VRGestureManagerState.Detecting;
                StartRecording();
            }

            if (state == VRGestureManagerState.Detecting)
            {
                CapturePoint();
            }
        }

        void UpdateWithButtons()
        {
            float trigger1 = input.GetAxis1D(InputOptions.Axis1D.Trigger1);

            if (trigger1 < 0.5 && state == VRGestureManagerState.Recording)
            {
                state = VRGestureManagerState.ReadyToRecord;
                //StopRecording();
                if (currentCapturedLine.Count > 0)
                {
                    StopRecording();
                }
            }
            else if (trigger1 >= 0.5 && state == VRGestureManagerState.ReadyToRecord)
            {
                state = VRGestureManagerState.Recording;
                StartRecording();

            }

            if (Time.time > nextRenderTime)
            {
                nextRenderTime = Time.time + renderRateLimit / 1000;
                if (state == VRGestureManagerState.Recording)
                {
                    CapturePoint();
                }
            }
        }

        void StartRecording()
        {
            nextRenderTime = Time.time + renderRateLimit / 1000;
            currentRenderer.SetColors(Color.magenta, Color.magenta);
            displayLine.Clear();

            CapturePoint();

        }

        void CapturePoint()
        {
            Vector3 rightHandPoint = playerHand.position;
            Vector3 localizedPoint = getLocalizedPoint(rightHandPoint);
            currentCapturedLine.Add(localizedPoint);
            displayLine.Add(rightHandPoint);
            currentRenderer.SetVertexCount(displayLine.Count);
            currentRenderer.SetPositions(displayLine.ToArray());
        }

        void StopRecording()
        {

            if (currentCapturedLine.Count > 0)
            {
                LineCaught(currentCapturedLine);
                currentCapturedLine.RemoveRange(0, currentCapturedLine.Count);
                currentCapturedLine.Clear();

                currentRenderer.SetColors(Color.blue, Color.cyan);
            }

        }

        void UpdateContinual()
        {
            //		state = VRGestureManagerState.Detecting;
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

        //This should be called directly from UIController via instance
        public void BeginReadyToRecord(string gesture)
        {
            //Debug.Log("BeginReadyToRecord in VRGestureManager: " + gesture);
            //Put a one second delay on this.
            currentTrainer = new Trainer(gestureBank, currentNeuralNet);
            gestureToRecord = gesture;
            state = VRGestureManagerState.EnteringRecord;
        }

        public void BeginEditing(string gesture)
        {
            gestureToRecord = gesture;
        }

        public void BeginDetect(string ignoreThisString)
        {
            gestureToRecord = "";
            state = VRGestureManagerState.Detecting;
            currentRecognizer = new GestureRecognizer(currentNeuralNet);
        }

        [ExecuteInEditMode]
        public void BeginTraining(Action<string> callback)
        {
            //Debug.Log("Begin Training " + currentNeuralNet );
            state = VRGestureManagerState.Training;
            currentTrainer = new Trainer(gestureBank, currentNeuralNet);
            currentTrainer.TrainRecognizer();
            //Debug.Log("Done Training " + currentNeuralNet );
            // finish training
            state = VRGestureManagerState.Idle;
            callback(currentNeuralNet);
        }

        [ExecuteInEditMode]
        public void EndTraining(Action<string> callback)
        {
            Debug.Log("Quit Training " + currentNeuralNet);
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
        public void RefreshNeuralNetList()
        {
            neuralNets.Clear();
            string path = Config.SAVE_FILE_PATH;
            foreach (string directoryPath in System.IO.Directory.GetDirectories(path))
            {
                string directoryName = Path.GetFileName(directoryPath);
                if (!neuralNets.Contains(directoryName))
                {
                    neuralNets.Add(directoryName);
                }
            }
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
            //		Debug.Log("SELECT NET: " + neuralNetName);

            // Load the neural net and gestures into gesture bank
            //		Debug.Log("selecting neural net: " + neuralNetName);
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
            gestureBankPreEdit = new List<string>(gestureBank);
        }

        [ExecuteInEditMode]
        public void DeleteGesture(string gestureName)
        {
            Debug.Log("deleting gesture: " + gestureName);
            gestureBank.Remove(gestureName);
            Utils.Instance.DeleteGestureFile(gestureName, currentNeuralNet);
            gestureBankPreEdit = new List<string>(gestureBank);
        }

        //public void DeleteGestureExample(string gesture, int lineNumber)
        //{
        //    Utils.Instance.DeleteGestureExample(currentNeuralNet, gesture, lineNumber);
        //}

        List<string> gestureBankPreEdit;

        [ExecuteInEditMode]
        public void EditGestures()
        {
            Debug.Log("edit gestures");
            gestureBankPreEdit = new List<string>(gestureBank);
        }

        bool CheckForDuplicateGestures(string newName)
        {
            bool dupeCheck = true;
            int dupeCount = -1;
            foreach (string gesture in gestureBank)
            {
                if (newName == gesture)
                {
                    dupeCount++;
                }
            }
            if (dupeCount > 0)
            {
                dupeCheck = false;
            }

            return dupeCheck;
        }

        [ExecuteInEditMode]
        public VRGestureManagerEditor.VRGestureRenameState RenameGesture(int gestureIndex)
        {
            //check to make sure the name has actually changed.
            string newName = gestureBank[gestureIndex];
            string oldName = gestureBankPreEdit[gestureIndex];
            VRGestureManagerEditor.VRGestureRenameState renameState = VRGestureManagerEditor.VRGestureRenameState.Good;

            if (oldName != newName)
            {
                if (CheckForDuplicateGestures(newName))
                {
                    //ACTUALLY RENAME THAT SHIZZ
                    Utils.Instance.RenameGestureFile(oldName, newName, currentNeuralNet);
                    gestureBankPreEdit = new List<string>(gestureBank);

                }
                else
                {
                    //reset gestureBank
                    gestureBank = new List<string>(gestureBankPreEdit);
                    renameState = VRGestureManagerEditor.VRGestureRenameState.Duplicate;
                }
            }
            else
            {
                renameState = VRGestureManagerEditor.VRGestureRenameState.NoChange;
            }

            return renameState;
        }


    }
}
