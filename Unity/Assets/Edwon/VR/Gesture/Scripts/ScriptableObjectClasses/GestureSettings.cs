using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR;
using Edwon.VR.Input;
using Edwon.VR.Gesture;
using System.IO;
using System;

[CreateAssetMenu(fileName = "Settings", menuName = "VRInfiniteGesture/Settings", order = 1)]
public class GestureSettings : ScriptableObject {

    [Header("VR Infinite Gesture")]
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

    [Header("ACTIVE NETWORKS")]
    [Tooltip("the neural net that I am using")]
    public string currentNeuralNet;
    public string lastNeuralNet; // used to know when to refresh gesture bank
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







    public VRGestureManagerState state = VRGestureManagerState.Idle;
    public VRGestureManagerState stateInitial;
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
    //Drop Down list of NeuralNetworks.
    //List of Processed Gestures
    //List of New Gestures sitting in data.
















    #region HIGH LEVEL METHODS

    //This should be called directly from UIController via instance
    public void BeginReadyToRecord(string gesture)
    {
        //currentTrainer = new Trainer(gestureBank, currentNeuralNet);
        //currentTrainer.CurrentGesture = gesture;
        //state = VRGestureManagerState.ReadyToRecord;
        //leftCapture.state = VRGestureCaptureState.EnteringCapture;
        //rightCapture.state = VRGestureCaptureState.EnteringCapture;
    }

    public void BeginEditing(string gesture)
    {
        //currentTrainer.CurrentGesture = gesture;
    }

    public void BeginDetect(string ignoreThisString)
    {
        //state = VRGestureManagerState.ReadyToDetect;
        //currentRecognizer = new GestureRecognizer(currentNeuralNet);
    }

    [ExecuteInEditMode]
    public void BeginTraining(Action<string> callback)
    {
        //state = VRGestureManagerState.Training;
        //currentTrainer = new Trainer(gestureBank, currentNeuralNet);
        //currentTrainer.TrainRecognizer();
        //// finish training
        //state = VRGestureManagerState.Idle;
        //callback(currentNeuralNet);
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
        Debug.Log("IM GETTING CALLED ALL THE TIME");
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

#if UNITY_EDITOR
    [ExecuteInEditMode]
    public GestureSettingsEditor.VRGestureRenameState RenameGesture(int gestureIndex)
    {
        //check to make sure the name has actually changed.
        string newName = gestureBank[gestureIndex];
        string oldName = gestureBankPreEdit[gestureIndex];
        GestureSettingsEditor.VRGestureRenameState renameState = GestureSettingsEditor.VRGestureRenameState.Good;

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
                renameState = GestureSettingsEditor.VRGestureRenameState.Duplicate;
            }
        }
        else
        {
            renameState = GestureSettingsEditor.VRGestureRenameState.NoChange;
        }

        return renameState;
    }
#endif

    #endregion

}