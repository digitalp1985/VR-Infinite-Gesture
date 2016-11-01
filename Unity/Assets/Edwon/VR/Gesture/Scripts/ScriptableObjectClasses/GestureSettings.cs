using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR;
using Edwon.VR.Input;
using System.IO;
using System;

namespace Edwon.VR.Gesture
{
    [CreateAssetMenu(fileName = "Settings", menuName = "VRInfiniteGesture/Settings", order = 1)]
    public class GestureSettings : ScriptableObject
    {

        public VRGestureRig rig;
        public int playerID = 0;
        public VRType vrType;

        [Header("VR Infinite Gesture")]
        [Tooltip("display default gesture trails")]
        public bool displayGestureTrail = true;
        [Tooltip("if true automatically spawn the VR Gesture UI when the scene starts")]
        public bool showVRUI = true;
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

        [Header("ACTIVE NETWORKS")]
        [Tooltip("the neural net that I am using")]
        public string currentNeuralNet;
        public string lastNeuralNet; // used to know when to refresh gesture bank
        public List<string> neuralNets;
        private List<Gesture> gestures;  // list of gestures already trained in currentNeuralNet
        public List<Gesture> Gestures
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
        public List<Gesture> gestureBank; // list of recorded gesture for current neural net
        public List<int> gestureBankTotalExamples;

        public Trainer currentTrainer { get; set; }

        public VRGestureUIState state = VRGestureUIState.Idle;
        public VRGestureUIState stateInitial;

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
        //Drop Down list of NeuralNetworks.
        //List of Processed Gestures
        //List of New Gestures sitting in data.

        public void OnEnable()
        {
            rig = VRGestureRig.GetPlayerRig(playerID);
        }


        #region NEURAL NETWORK ACTIVE METHODS
        //This should be called directly from UIController via instance
        //Most of these should be moved into RIG as they are just editing vars in RIG.
        [ExecuteInEditMode]
        public void BeginTraining(Action<string> callback)
        {
            rig = VRGestureRig.GetPlayerRig(playerID);
            rig.state = VRGestureUIState.Training;
            rig.currentTrainer = new Trainer(currentNeuralNet, gestureBank);
            rig.currentTrainer.TrainRecognizer();
            // finish training
            rig.state = VRGestureUIState.Idle;
            callback(currentNeuralNet);
        }

        [ExecuteInEditMode]
        public void EndTraining(Action<string> callback)
        {
            rig = VRGestureRig.GetPlayerRig(playerID);
            rig.state = VRGestureUIState.Idle;
            callback(currentNeuralNet);
        }
        #endregion



        #region NEURAL NETWORK EDIT METHODS
        [ExecuteInEditMode]
        public bool CheckForDuplicateNeuralNetName(string neuralNetName)
        {
            // if neuralNetName already exists return true
            if (neuralNets.Contains(neuralNetName))
                return true;
            else
                return false;
        }

        // only called by VR UI when creating a new neural net in VR
        public void CreateNewNeuralNet()
        {
            int number = neuralNets.Count + 1;
            string newNeuralNetName = "Neural Net " + number;
            CreateNewNeuralNet(newNeuralNetName);
        }

        [ExecuteInEditMode]
        public void CreateNewNeuralNet(string neuralNetName)
        {
            // create new neural net folder
            Utils.CreateFolder(neuralNetName);
            // create a gestures folder
            Utils.CreateFolder(neuralNetName + "/Gestures/");

            neuralNets.Add(neuralNetName);
            gestures = new List<Gesture>();
            gestureBank = new List<Gesture>();
            gestureBankPreEdit = new List<Gesture>();
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
            //Debug.Log("REFRESH GESTURE BANK");

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

            gestureBankPreEdit = gestureBank.ConvertAll(gesture => gesture.Clone());
                gestureBankTotalExamples = Utils.GetGestureBankTotalExamples(gestureBank, currentNeuralNet);
            }
            else
            {
                gestureBank = new List<Gesture>();
                gestureBankPreEdit = new List<Gesture>();
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
        public void CreateGesture(string gestureName, bool isSynchronized = false)
        {
            Gesture newGesture = new Gesture();
            newGesture.name = gestureName;
            newGesture.hand = Handedness.Right;
            newGesture.isSynchronous = false;
            newGesture.exampleCount = 0;


            gestureBank.Add(newGesture);
            gestureBankTotalExamples.Add(0);
            Utils.CreateGestureFile(gestureName, currentNeuralNet);
            Utils.SaveGestureBank(gestureBank, currentNeuralNet);
        gestureBankPreEdit = gestureBank.ConvertAll(gesture => gesture.Clone());
        }

        public void CreateSingleGesture(string gestureName, Handedness hand, bool isSynchronous)
        {
            Gesture newGesture = new Gesture();
            newGesture.name = gestureName;
            newGesture.hand = hand;
            newGesture.isSynchronous = isSynchronous;
            newGesture.exampleCount = 0;


            gestureBank.Add(newGesture);
            gestureBankTotalExamples.Add(0);
            //Maybe name files based on isSync - Hand - name. i.e.: 1R-Helicopter 0B-Rainbow
            Utils.CreateGestureFile(gestureName, currentNeuralNet);
            Utils.SaveGestureBank(gestureBank, currentNeuralNet);
        gestureBankPreEdit = gestureBank.ConvertAll(gesture => gesture.Clone());
        }

        public void CreateSyncGesture(string gestureName)
        {
            CreateSingleGesture(gestureName, Handedness.Left, true);
            CreateSingleGesture(gestureName, Handedness.Right, true);
        }

        [ExecuteInEditMode]
        public Gesture FindGesture(string gestureName)
        {
            //int index = gestureBank.IndexOf(gestureName);
            Predicate<Gesture> gestureFinder = (Gesture g) => { return g.name == gestureName; };
            Gesture gest = gestureBank.Find(gestureFinder);
            return gest;
        }


        [ExecuteInEditMode]
        public void DeleteGesture(string gestureName)
        {
            Predicate<Gesture> gestureFinder = (Gesture g) => { return g.name == gestureName; };
            int index = gestureBank.FindIndex(gestureFinder);
            gestureBank.RemoveAt(index);
            gestureBankTotalExamples.RemoveAt(index);
            Utils.DeleteGestureFile(gestureName, currentNeuralNet);
            Utils.SaveGestureBank(gestureBank, currentNeuralNet);
        gestureBankPreEdit = gestureBank.ConvertAll(gesture => gesture.Clone());
        }

        List<Gesture> gestureBankPreEdit;

        bool CheckForDuplicateGestures(string newName)
        {
            bool dupeCheck = true;
            int dupeCount = -1;
            foreach (Gesture gesture in gestureBank)
            {
                if (newName == gesture.name)
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

#if UNITY_EDITOR
        [ExecuteInEditMode]
        public GestureSettingsWindow.VRGestureRenameState RenameGesture(int gestureIndex)
        {
        string newName = "";
        string oldName = "";

            //check to make sure the name has actually changed.
        if(gestureIndex < gestureBank.Count)
        {
            newName = gestureBank[gestureIndex].name;
            oldName = gestureBankPreEdit[gestureIndex].name;
        }else
        {
            Debug.LogError("Out of bounds");
        }

            GestureSettingsWindow.VRGestureRenameState renameState = GestureSettingsWindow.VRGestureRenameState.Good;

            if (oldName != newName)
            {
                if (CheckForDuplicateGestures(newName))
                {
                    //ACTUALLY RENAME THAT SHIZZ
                    Utils.RenameGestureFile(oldName, newName, currentNeuralNet);
                Utils.SaveGestureBank(gestureBank, currentNeuralNet);

                gestureBankPreEdit = gestureBank.ConvertAll(gesture => gesture.Clone());
                }
                else
                {
                    //reset gestureBank
                gestureBank = gestureBankPreEdit.ConvertAll(gesture => gesture.Clone());
                    renameState = GestureSettingsWindow.VRGestureRenameState.Duplicate;
                }
            }
            else
            {
                renameState = GestureSettingsWindow.VRGestureRenameState.NoChange;
            }

            return renameState;
        }
#endif

        #endregion

    }
}