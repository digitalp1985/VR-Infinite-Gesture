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

    public enum VRGestureManagerState { Idle, Edit, Editing, EnteringRecord, ReadyToRecord, Recording, Training, EnteringDetect, ReadyToDetect, Detecting };
    public enum VRGestureDetectType { Button, Continious };

    public class VRGestureManager : MonoBehaviour
    {
        #region SINGLETON
        static VRGestureManager instance;

        public static VRGestureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    //instance = FindObjectOfType<VRGestureManager>();
                    VRGestureManager[] instances = FindObjectsOfType<VRGestureManager>();
                    if (instances.Length == 1)
                    {
                        instance = instances[0];
                    }
                    else if (instances.Length == 0)
                    {
                        
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        instance = obj.AddComponent<VRGestureManager>();
                    }
                    else
                    {
                        Debug.LogError("There are too many VRGestureManagers added to your scene. VRGestureManager behaves as a signleton. Please remove any extra VRGestureManager components.");
                    }

                    instance.Init();
                }
                return instance;
            }
        }
        #endregion

        #region SETTINGS VARIABLES
        // which hand to track
        [SerializeField]
        [Tooltip("which hand to track using the gesture")]
        public HandType gestureHand = HandType.Right; // the hand to track
        [Tooltip("display default gesture trails")]
        public bool displayGestureTrail = true;
        [Tooltip("the button that triggers gesture recognition")]
        public InputOptions.Button gestureButton = InputOptions.Button.Trigger1;
        [Tooltip("the threshold over wich a gesture is considered correctly classified")]
        public double confidenceThreshold = 0.98;
        [Tooltip("Your gesture must have one axis longer than this length in world size")]
        public float minimumGestureAxisLength = 0.10f;
        [Tooltip("use this option for builds when you don't want users to see the VR UI from this plugin")]
        public bool beginInDetectMode = false;
        // whether to track when pressing trigger or all the time
        // continious mode is not supported yet
        // though you're welcome to try it out
        [HideInInspector]
        public VRGestureDetectType vrGestureDetectType;
        #endregion

        #region STATE VARIABLES
        public VRGestureManagerState state;
        [SerializeField]
        public VRGestureManagerState stateInitial;
        [SerializeField]
        public VRGestureManagerState stateLast;

        public bool readyToTrain
        {
            get
            {
                if (gestureBank != null)
                {
                    if (gestureBank.Count > 0)
                    {
                        foreach (int total in gestureBankTotalExamples)
                        {
                            if (total <= 0)
                                return false;
                        }
                        return true;
                    }
                    else
                        return false;
                }
                return false;
            }
        }

        #endregion

        #region AVATAR VARIABLES
        public VRGestureRig rig;
        IInput input;
        Transform playerHead;
        Transform playerHand;
        Transform perpTransform;
        CaptureHand leftCapture;
        CaptureHand rightCapture;
        #endregion

        #region LINE CAPTURE VARIABLES
        GestureTrail myTrail;
        List<Vector3> currentCapturedLine;
        public string gestureToRecord;

        float nextRenderTime = 0;
        float renderRateLimit = Config.CAPTURE_RATE;
        float nextTestTime = 0;
        float testRateLimit = 500;
        #endregion

        #region NEURAL NET VARIABLES

        [Tooltip("the neural net that I am using")]
        [SerializeField]
        public string currentNeuralNet;
        [SerializeField]
        public string lastNeuralNet; // used to know when to refresh gesture bank
        [SerializeField]
        public List<string> neuralNets;
        private List<string> gestures;  // list of gestures already trained in currentNeuralNet
        public List<string> Gestures
        {
            get
            {
                NeuralNetworkStub stub = Utils.ReadNeuralNetworkStub(currentNeuralNet);
                return stub.gestures;
            }
            set
            {
                value = gestures;
            }
        }
        public List<string> gestureBank; // list of recorded gesture for current neural net
        public List<int> gestureBankTotalExamples;

        Trainer currentTrainer;
        GestureRecognizer currentRecognizer;

        #endregion

        #region EVENTS VARIABLES
        public delegate void GestureDetected(string gestureName, double confidence, HandType hand);
        public static event GestureDetected GestureDetectedEvent;
        public delegate void GestureRejected(string error, string gestureName = null, double confidence = 0);
        public static event GestureRejected GestureRejectedEvent;

        #endregion

        #region DEBUG VARIABLES
        public string debugString;
        #endregion

        #region INITIALIZE

        public virtual void Awake()
        {

            DontDestroyOnLoad(this.gameObject);
            if (instance == null)
            {
                instance = this;
                instance.Init();
            }

        }

        public virtual void OnDestroy()
        {

        }

        void Init()
        {
            if (FindObjectOfType<VRGestureRig>() != null)
            {
                rig = FindObjectOfType<VRGestureRig>();
                playerHead = rig.head;
                playerHand = rig.GetHand(gestureHand);
                input = rig.GetInput(gestureHand);

                //maybe only init this if it does not exist.
                //Remove all game objects


                perpTransform = transform.Find("Perpindicular Head");
                if(perpTransform == null)
                {
                    Debug.Log("MAKING A NEW PERPS TRANSFORM");
                    perpTransform = new GameObject("Perpindicular Head").transform;
                    perpTransform.parent = this.transform;
                }


                //add a new capturehand to each of 
                //Check if capture hand component exists.
                //leftCapture = gameObject.AddComponent<CaptureHand>().Init(rig, perpTransform, HandType.Left);
                //rightCapture = gameObject.AddComponent<CaptureHand>().Init(rig, perpTransform, HandType.Right);
                leftCapture = new CaptureHand(rig, perpTransform, HandType.Left);
                rightCapture = new CaptureHand(rig, perpTransform, HandType.Right);
            }
        }

        void Start()
        {
            if (stateInitial == VRGestureManagerState.ReadyToDetect)
            {
                BeginDetect("");
            }
            else if (FindObjectOfType<VRGestureUI>() == null)
            {
                Debug.LogError("Cannot find VRGestureUI in scene. Please add it or select Begin In Detect Mode in the VR Gesture Manager Settings");
            }
            
            state = stateInitial;
            stateLast = state;
            gestureToRecord = "";

            input = rig.GetInput(gestureHand);

            //create a new Trainer
            currentTrainer = new Trainer(Gestures, currentNeuralNet);
            currentCapturedLine = new List<Vector3>();
        }

        void OnEnable()
        {
            if (leftCapture != null && rightCapture != null)
            {
                SubscribeToEvents();
            }
        }

        void SubscribeToEvents()
        {
            leftCapture.StartCaptureEvent += StartCapturing;
            leftCapture.StopCaptureEvent += StopCapturing;
            rightCapture.StartCaptureEvent += StartCapturing;
            rightCapture.StopCaptureEvent += StopCapturing;
        }

        void OnDisable()
        {
            leftCapture.StartCaptureEvent -= StartCapturing;
            leftCapture.StopCaptureEvent -= StopCapturing;
            rightCapture.StartCaptureEvent -= StartCapturing;
            rightCapture.StopCaptureEvent -= StopCapturing;
        }

        #endregion

        #region LINE CAPTURE

        public GestureTrail GetOrAddGestureTrail(CaptureHand captureHand)
        {
            GestureTrail[] results = GetComponents<GestureTrail>();
            GestureTrail thisOne = null;

            if (results.Length <= 1)
            {
                thisOne = gameObject.AddComponent<GestureTrail>();
            }
            else
            {
                foreach(GestureTrail myTrail in results)
                {
                    if (!myTrail.UseCheck())
                    {
                        Debug.Log("found trail");
                        thisOne = myTrail;
                        thisOne.AssignHand(captureHand);
                        break;
                    }
                }
            }
            return thisOne;
        }

        public void LineCaught(List<Vector3> capturedLine, HandType hand)
        {
            if (state == VRGestureManagerState.Recording || state == VRGestureManagerState.ReadyToRecord)
            {
                TrainLine(gestureToRecord, capturedLine, hand);
            }
            else if (state == VRGestureManagerState.Detecting || state == VRGestureManagerState.ReadyToDetect)
            {
                RecognizeLine(capturedLine, hand);
            }
        }

        public void TrainLine(string gesture, List<Vector3> capturedLine, HandType hand)
        {
            currentTrainer.AddGestureToTrainingExamples(gesture, capturedLine, hand);
            debugString = "trained : " + gesture;
        }

        public void RecognizeLine(List<Vector3> capturedLine, HandType hand)
        {
            if (IsGestureBigEnough(capturedLine))
            {
                //Detect if the captured line meets minimum gesture size requirements
                double[] networkInput = Utils.FormatLine(capturedLine, hand);
                string gesture = currentRecognizer.GetGesture(networkInput);
                string confidenceValue = currentRecognizer.currentConfidenceValue.ToString("N3");

                // broadcast gesture detected event
                if (currentRecognizer.currentConfidenceValue > VRGestureManager.Instance.confidenceThreshold)
                {
                    debugString = gesture + " " + confidenceValue;
                    if (VRGestureManager.GestureDetectedEvent != null)
                    {
                        GestureDetectedEvent(gesture, currentRecognizer.currentConfidenceValue, hand);
                        //Check if the other hand has recently caught a gesture.
                        //CheckForSyncGestures(gesture, hand);
                        if(hand == HandType.Left)
                        {
                            leftCapture.SetRecognizedGesture(gesture);
                            //FIRE BOTH GESTURE
                            if (rightCapture.CheckForSync(gesture))
                            {
                                GestureDetectedEvent("BOTH: "+ gesture, 2.0, hand);
                                debugString = "DOUBLE" + gesture;
                            }
                        }
                        else if (hand == HandType.Right)
                        {
                            rightCapture.SetRecognizedGesture(gesture);
                            //FIRE BOTH GESTURE
                            if (leftCapture.CheckForSync(gesture))
                            {
                                GestureDetectedEvent("BOTH: "+ gesture, 2.0, hand);
                                debugString = "DOUBLE" + gesture;
                            }
                        }
                    }
                        
                }
                else
                {
                    debugString = "Null \n" + gesture + " " + confidenceValue;
                    if (GestureRejectedEvent != null)
                        GestureRejectedEvent("Confidence Too Low", gesture, currentRecognizer.currentConfidenceValue);
                }
            }
            else
            {
                //broadcast that a gesture is too small??
                debugString = "Gesture is too small!";
                if (GestureRejectedEvent != null)
                    GestureRejectedEvent("Gesture is too small");
                
            }
        }

        public bool IsGestureBigEnough(List<Vector3> capturedLine)
        {
            float check = Utils.FindMaxAxis(capturedLine);
            return (check > minimumGestureAxisLength);
        }
		#endregion

		#region UPDATE

        void Update()
        {
            if(leftCapture != null)
            {
                leftCapture.Update();
            }
            if (rightCapture != null)
            {
                rightCapture.Update();
            }
        }

        void StartCapturing()
        {
            if(state == VRGestureManagerState.ReadyToRecord)
            {
                state = VRGestureManagerState.Recording;
            }
            else if(state == VRGestureManagerState.ReadyToDetect)
            {
                state = VRGestureManagerState.Detecting;
            }
        }

        void StopCapturing()
        {
            if(leftCapture.state == VRGestureCaptureState.Capturing || rightCapture.state == VRGestureCaptureState.Capturing)
            {
                //do nothing
            }
            else
            {
                //set state to READY
                if (state == VRGestureManagerState.Recording)
                {
                    state = VRGestureManagerState.ReadyToRecord;
                }
                else if (state == VRGestureManagerState.Detecting)
                {
                    state = VRGestureManagerState.ReadyToDetect;
                }
            }
        }

		#endregion

		#region HIGH LEVEL METHODS

        //This should be called directly from UIController via instance
        public void BeginReadyToRecord(string gesture)
        {
            currentTrainer = new Trainer(gestureBank, currentNeuralNet);
            gestureToRecord = gesture;
            state = VRGestureManagerState.ReadyToRecord;
            leftCapture.state = VRGestureCaptureState.EnteringCapture;
            rightCapture.state = VRGestureCaptureState.EnteringCapture;
        }

        public void BeginEditing(string gesture)
        {
            gestureToRecord = gesture;
        }

        public void BeginDetect(string ignoreThisString)
        {
            gestureToRecord = "";
            state = VRGestureManagerState.ReadyToDetect;
            currentRecognizer = new GestureRecognizer(currentNeuralNet);
        }

        [ExecuteInEditMode]
        public void BeginTraining(Action<string> callback)
        {
            state = VRGestureManagerState.Training;
            currentTrainer = new Trainer(gestureBank, currentNeuralNet);
            currentTrainer.TrainRecognizer();
            // finish training
            state = VRGestureManagerState.Idle;
            callback(currentNeuralNet);
        }

        [ExecuteInEditMode]
        public void EndTraining(Action<string> callback)
        {
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
            Utils.CreateFolder(neuralNetName);
            // create a gestures folder
            Utils.CreateFolder(neuralNetName + "/Gestures/");

            neuralNets.Add(neuralNetName);
            gestures = new List<string>();
            gestureBank = new List<string>();
            gestureBankPreEdit = new List<string>();
            gestureBankTotalExamples = new List<int>();

            // select the new neural net
            SelectNeuralNet(neuralNetName);
        }

        [ExecuteInEditMode]
        public void RefreshNeuralNetList()
        {
            neuralNets = new List<string>();
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
        public void RefreshGestureBank(bool checkNeuralNetChanged)
        {
            if (checkNeuralNetChanged)
            {
                if (currentNeuralNet == lastNeuralNet)
                {
                    return;
                }
            }
            if (currentNeuralNet != null && currentNeuralNet != "" && Utils.GetGestureBank(currentNeuralNet) != null)
            {
                gestureBank = Utils.GetGestureBank(currentNeuralNet);
                gestureBankPreEdit = new List<string>(gestureBank);
                gestureBankTotalExamples = Utils.GetGestureBankTotalExamples(gestureBank, currentNeuralNet);
            }
            else
            {
                gestureBank = new List<string>();
                gestureBankPreEdit = new List<string>();
                gestureBankTotalExamples = new List<int>();
            }
        }

        [ExecuteInEditMode]
        public void DeleteNeuralNet(string neuralNetName)
        {
            // get this neural nets index so we know which net to select next
            int deletedNetIndex = neuralNets.IndexOf(neuralNetName);

            // delete the net and gestures
            neuralNets.Remove(neuralNetName); // remove from list
            gestureBank.Clear(); // clear the gestures list
            gestureBankPreEdit.Clear();
			gestureBankTotalExamples.Clear();
            Utils.DeleteNeuralNetFiles(neuralNetName); // delete all the files

            if (neuralNets.Count > 0)
                SelectNeuralNet(neuralNets[0]);
        }

        [ExecuteInEditMode]
        public void SelectNeuralNet(string neuralNetName)
        {
            lastNeuralNet = currentNeuralNet;
            currentNeuralNet = neuralNetName;
            RefreshGestureBank(true);
        }

        [ExecuteInEditMode]
        public void CreateGesture(string gestureName)
        {
            gestureBank.Add(gestureName);
			gestureBankTotalExamples.Add(0);
            Utils.CreateGestureFile(gestureName, currentNeuralNet);
            gestureBankPreEdit = new List<string>(gestureBank);
        }

        [ExecuteInEditMode]
        public void DeleteGesture(string gestureName)
        {
			int index = gestureBank.IndexOf(gestureName);
            gestureBank.Remove(gestureName);
			gestureBankTotalExamples.RemoveAt(index);
            Utils.DeleteGestureFile(gestureName, currentNeuralNet);
            gestureBankPreEdit = new List<string>(gestureBank);
        }
			
        List<string> gestureBankPreEdit;

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

		#endregion


#if UNITY_EDITOR
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
                    Utils.RenameGestureFile(oldName, newName, currentNeuralNet);
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
#endif

    }
}
