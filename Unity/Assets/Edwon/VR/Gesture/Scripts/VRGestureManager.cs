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
        Transform perpTransform;
        CaptureHand leftCapture;
        CaptureHand rightCapture;
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

        public Trainer currentTrainer { get; set; }
        GestureRecognizer currentRecognizer;

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

                //maybe only init this if it does not exist.
                //Remove all game objects
                perpTransform = transform.Find("Perpindicular Head");
                if(perpTransform == null)
                {
                    perpTransform = new GameObject("Perpindicular Head").transform;
                    perpTransform.parent = this.transform;
                }

                //add a new capturehand to each of 
                //Check if capture hand component exists.
                GestureTrail leftTrail = null;
                GestureTrail rightTrail = null;

                if (displayGestureTrail)
                {
                    leftTrail = gameObject.AddComponent<GestureTrail>();
                    rightTrail = gameObject.AddComponent<GestureTrail>();
                }
                leftCapture = new CaptureHand(rig, perpTransform, HandType.Left, leftTrail);
                rightCapture = new CaptureHand(rig, perpTransform, HandType.Right, rightTrail);
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
            //create a new Trainer
            currentTrainer = new Trainer(Gestures, currentNeuralNet);
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
        public void LineCaught(List<Vector3> capturedLine, HandType hand)
        {
            if (state == VRGestureManagerState.Recording || state == VRGestureManagerState.ReadyToRecord)
            {
                currentTrainer.TrainLine(capturedLine, hand);
            }
            else if (state == VRGestureManagerState.Detecting || state == VRGestureManagerState.ReadyToDetect)
            {
                currentRecognizer.RecognizeLine(capturedLine, hand);
            }
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
            currentTrainer.CurrentGesture = gesture;
            state = VRGestureManagerState.ReadyToRecord;
            leftCapture.state = VRGestureCaptureState.EnteringCapture;
            rightCapture.state = VRGestureCaptureState.EnteringCapture;
        }

        public void BeginEditing(string gesture)
        {
            currentTrainer.CurrentGesture = gesture;
        }

        public void BeginDetect(string ignoreThisString)
        {
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
