using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GestureUIController : MonoBehaviour
{
    public enum VRUIType { SteamVR, EdwonVR };
    public VRUIType vrUiType;

    public VROptions.Handedness handedness;
    private PanelManager panelManager;
    Transform uiHand;
    Transform uiCam;
    public float offsetZ;

    public VRGestureManager vrGestureManager; // the VRGestureManager script we want to interact with
    public RectTransform recordMenu; // the top level transform of the recordMenu where we will generate gesture buttons
    public RectTransform selectNeuralNetMenu; // the top level transform of the select neural net menu where we will generate buttons
    public GameObject buttonPrefab;

    // RECORD MENU
    private List<Button> gestureButtons;
    [Tooltip("the title of the gesture list on the record menu")]
    public CanvasRenderer gestureListTitle;
    public CanvasRenderer newGestureButton;

    // RECORDING MENU
    [Tooltip("the now recording indicator in the recording menu")]
    public Text nowRecordingLabel;
    public Image nowRecordingBackground;
    [Tooltip("the label that tells you what gesture your recording currently")]
    public Text gestureTitle;
    [Tooltip("the button that deletes gestures in the Recording Menu")]
    public Button deleteGestureButton;

    // DETECT MENU
    [Tooltip("the ui text that should be updated with a gesture detect log")]
    public Text detectLog;

    // SELECT NEURAL NET MENU
    [Tooltip("the panel of the Select Neural Net Menu")]
    public RectTransform neuralNetTitle;

    // TRAINING MENU
    [Tooltip("the text feedback for the currently training neural net")]
    public Text neuralNetTraining;

    // default settings
    private Vector3 buttonRectScale; // new Vector3(0.6666f, 1, 0.2f);

    void Start()
    {
        buttonRectScale = new Vector3(0.6666f, 1, 0.2f);

        // get line capturer
        if (vrGestureManager == null)
            vrGestureManager = GameObject.FindObjectOfType<VRGestureManager>();

        // get vr player hand and camera
        if (vrUiType == VRUIType.EdwonVR)
        {
            uiHand = PlayerManager.GetPlayerHand(0, handedness).transform;
            uiCam = PlayerManager.GetPlayerCamera(0).transform;
        }
        else if (vrUiType == VRUIType.SteamVR)
        {
            SteamVR_ControllerManager ControllerManager;
            ControllerManager = GameObject.FindObjectOfType<SteamVR_ControllerManager>();
            if (handedness == VROptions.Handedness.Left)
            {
                uiHand = ControllerManager.left.GetComponent<SteamVR_TrackedObject>().transform;
            }
            else
            {
                uiHand = ControllerManager.right.GetComponent<SteamVR_TrackedObject>().transform;
            }
            uiCam = GameObject.FindObjectOfType<SteamVR_Camera>().transform;
        }

        panelManager = transform.GetComponentInChildren<PanelManager>();

        GenerateRecordMenuButtons();
        GenerateNeuralNetMenuButtons();
    }

    void Update()
    {
        Vector3 handToCamVector = uiCam.position - uiHand.position;
        transform.position = uiHand.position + (offsetZ * handToCamVector);
        transform.rotation = Quaternion.LookRotation(transform.position - uiCam.position);

        // update detect log
        if (detectLog != null)
            detectLog.text = vrGestureManager.debugString;
        else
            Debug.Log("please set detect log on GestureUIController");

        UpdateCurrentNeuralNetworkText();
        UpdateNowRecordingStatus();
    }

    // events called by buttons when pressed

    public void BeginDetectMode()
    {
        //Debug.Log("begin detect mode");
        EventManager.TriggerEvent("BeginDetect");
    }

    public void BeingReadyToRecordGesture(string gestureName)
    {
        Debug.Log("begin ready to record gesture of type " + gestureName);
        gestureTitle.text = gestureName;
        deleteGestureButton.onClick.AddListener(() => DeleteGesture(gestureName) );
        deleteGestureButton.onClick.AddListener(() => panelManager.FocusPanel("Record Menu"));
        EventManager.TriggerEvent("ReadyToRecord", gestureName);
    }

    public void SelectNeuralNet(string neuralNetName)
    {
        vrGestureManager.SelectNeuralNet(neuralNetName);
    }

    public void CreateNewGesture()
    {
        Debug.Log("called create new gesture");
        string newGestureName = "Gesture " + (vrGestureManager.gestureBank.Count + 1);
        vrGestureManager.gestureBank.Add(newGestureName);
        GenerateRecordMenuButtons();
    }

    public void BeginTraining()
    {
        panelManager.FocusPanel("Training Menu");
        neuralNetTraining.text = vrGestureManager.currentNeuralNet;
        vrGestureManager.BeginTraining(OnFinishedTraining);
    }

    public void QuitTraining()
    {
        vrGestureManager.EndTraining(OnQuitTraining);
    }

    void OnFinishedTraining (string neuralNetName)
    {
        StartCoroutine(TrainingMenuDelay(1f));
    }

    void OnQuitTraining(string neuralNetName)
    {
        StartCoroutine(TrainingMenuDelay(1f));
    }

    public void DeleteGesture(string gestureName)
    {
        vrGestureManager.DeleteGesture(gestureName);
    }

    IEnumerator TrainingMenuDelay(float delay)
    {
        // after training complete and a short delay go back to main menu
        yield return new WaitForSeconds(delay);
        panelManager.FocusPanel("Main Menu");
    }

    // generate UI elements

    void GenerateRecordMenuButtons()
    {
        // first destroy the old gesture buttons if they are there
        if (gestureButtons != null)
        {
            if (gestureButtons.Count > 0)
            {
                foreach (Button button in gestureButtons)
                {
                    Destroy(button.gameObject);
                }
                gestureButtons.Clear();
            }
        }

        float recordMenuButtonHeight = 30;

        gestureButtons = GenerateButtonsFromList(vrGestureManager.gestureBank, recordMenu.transform, buttonPrefab, recordMenuButtonHeight);

        // set the functions that the button will call when pressed
        for (int i = 0; i < gestureButtons.Count; i++)
        {
            string gestureName = vrGestureManager.gestureBank[i];
            gestureButtons[i].onClick.AddListener(() => panelManager.FocusPanel("Recording Menu"));
            gestureButtons[i].onClick.AddListener(() => BeingReadyToRecordGesture(gestureName));
        }

        AdjustListTitlePosition(gestureListTitle.transform, gestureButtons.Count, recordMenuButtonHeight);

        // adjust new gesture button position
        float totalHeight = gestureButtons.Count * recordMenuButtonHeight;
        float y = -(totalHeight / 2);
        newGestureButton.transform.localPosition = new Vector3(0, y, 0);
        
    }

    void GenerateNeuralNetMenuButtons()
    {
        int neuralNetMenuButtonHeight = 30;

        List<Button> buttons = GenerateButtonsFromList(vrGestureManager.neuralNets, selectNeuralNetMenu.transform, buttonPrefab, neuralNetMenuButtonHeight);

        // set the functions that the button will call when pressed
        for (int i = 0; i < buttons.Count; i++)
        {
            string neuralNetName = vrGestureManager.neuralNets[i];
            buttons[i].onClick.AddListener(() => panelManager.FocusPanel("Main Menu"));
            buttons[i].onClick.AddListener(() => SelectNeuralNet(neuralNetName));
        }

        AdjustListTitlePosition(neuralNetTitle.transform, buttons.Count, neuralNetMenuButtonHeight);
    }

    List<Button> GenerateButtonsFromList(List<string> list, Transform parent, GameObject prefab, float buttonHeight)
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < list.Count; i++)
        {
            // instantiate the button
            GameObject button = GameObject.Instantiate(prefab);
            button.transform.parent = parent;
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.identity;
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.localScale = buttonRectScale;
            button.transform.name = list[i] + " Button";
            // set the button y position
            float totalHeight = list.Count * buttonHeight;
            float y = 0f;
            if (i == 0)
            {
                y = totalHeight / 2;
            }
            y = (totalHeight / 2) - (i * buttonHeight);
            buttonRect.localPosition = new Vector3(0, y, 0);
            // set the button text
            Text buttonText = button.transform.GetComponentInChildren<Text>();
            buttonText.text = list[i];
            buttons.Add(button.GetComponent<Button>());
        }
        return buttons;
    }

    void AdjustListTitlePosition(Transform title, int totalButtons, float buttonHeight)
    {
        if (title != null)
        {
            float totalHeight = totalButtons * buttonHeight;
            float y = (totalHeight / 2) + buttonHeight;
            title.localPosition = new Vector3(0, y, 0);
        }
        else
        {
            //Debug.Log("the title is null, can't adjust position");
        }
    }

    void OnEnable()
    {
        PanelManager.OnPanelFocusChanged += PanelFocusChanged;
    }

    void OnDisable()
    {
        PanelManager.OnPanelFocusChanged -= PanelFocusChanged;
    }

    void PanelFocusChanged(string panelName)
    {
        if (panelName == "Main Menu")
            vrGestureManager.state = VRGestureManagerState.Idle;
        if (panelName == "Record Menu")
        {
            vrGestureManager.state = VRGestureManagerState.Idle;
            GenerateRecordMenuButtons();
        }
        if (panelName == "Recording Menu")
        {
            vrGestureManager.state = VRGestureManagerState.ReadyToRecord;
        }
    }

    void UpdateCurrentNeuralNetworkText()
    {
        if (GetCurrentNeuralNetworkText() == null)
            return;

        Text title = GetCurrentNeuralNetworkText();
        title.text = vrGestureManager.currentNeuralNet;
    }

    void UpdateNowRecordingStatus()
    {
        if (vrGestureManager.state == VRGestureManagerState.ReadyToRecord)
        {
            nowRecordingBackground.color = Color.grey;
            nowRecordingLabel.text = "ready to record";
        }
        else if (vrGestureManager.state == VRGestureManagerState.Recording)
        {
            nowRecordingBackground.color = Color.red;
            nowRecordingLabel.text = "RECORDING";
        }
    }

    Text GetCurrentNeuralNetworkText()
    {
        // update current neural network name on each currentNeuralNetworkTitle UI thingy
        if (panelManager == null)
            return null;
        if (transform.Find("Panels") == null)
            return null;
        Transform panelsParent = transform.Find("Panels");
        if (panelsParent.Find(panelManager.currentPanel) == null)
            return null;
        Transform currentPanelParent = panelsParent.Find(panelManager.currentPanel);
        if (currentPanelParent == null)
            return null;
        Transform currentNeuralNetworkTitle = currentPanelParent.FindChild("Current Neural Network");
        if (currentNeuralNetworkTitle == null)
            return null;

        Text title = currentNeuralNetworkTitle.FindChild("neural network name").GetComponent<Text>();
        return title;
    }
}
