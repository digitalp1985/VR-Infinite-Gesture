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

    public LineCapture lineCapturer; // the LineCapture script we want to interact with
    public RectTransform recordMenu; // the top level transform of the recordMenu where we will generate gesture buttons
    public RectTransform selectNeuralNetMenu; // the top level transform of the select neural net menu where we will generate buttons
    public GameObject buttonPrefab;
    [Tooltip("the label that tells people to pick a gesture in the gesture record menu")]
    public CanvasRenderer recordLabel;
    [Tooltip("the title of the gesture list on the record menu")]
    public CanvasRenderer gestureListTitle;
    public CanvasRenderer newGestureButton;
    [Tooltip("the label that tells you what gesture your recording currently")]
    public Text gestureTitle;
    [Tooltip("the ui text that should be updated with a gesture detect log")]
    public Text detectLog;
    [Tooltip("the panel of the Select Neural Net Menu")]
    public RectTransform neuralNetTitle;

    // default settings
    private Vector3 buttonRectScale; // new Vector3(0.6666f, 1, 0.2f);

    void Start()
    {
        buttonRectScale = new Vector3(0.6666f, 1, 0.2f);

        // get line capturer
        if (lineCapturer == null)
            lineCapturer = GameObject.FindObjectOfType<LineCapture>();

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
            detectLog.text = lineCapturer.debugString;
        else
            Debug.Log("please set detect log on GestureUIController");

        UpdateCurrentNeuralNetworkText();

    }

    // events called by buttons when pressed

    public void BeginDetectMode()
    {
        Debug.Log("begin detect mode");
        EventManager.TriggerEvent("Detect");
    }

    public void BeginRecordGesture(string gestureName)
    {
        Debug.Log("begin record gesture of type " + gestureName);
        gestureTitle.text = gestureName;
        EventManager.TriggerEvent("Record", gestureName);
    }

    public void SelectNeuralNet(string neuralNetName)
    {
        //Debug.Log("selected neural net " + neuralNetName);

        // set the neural net to use on the line capturer
        lineCapturer.neuralNetUsing = neuralNetName;
    }

    // generate UI elements

    void GenerateRecordMenuButtons()
    {
        float recordMenuButtonHeight = 30;

        List<Button> buttons = GenerateButtonsFromList(lineCapturer.gestureList, recordMenu.transform, buttonPrefab, recordMenuButtonHeight);

        // set the functions that the button will call when pressed
        for (int i = 0; i < buttons.Count; i++)
        {
            string gestureName = lineCapturer.gestureList[i];
            buttons[i].onClick.AddListener(() => panelManager.FocusPanel("Recording Menu"));
            buttons[i].onClick.AddListener(() => BeginRecordGesture(gestureName));
        }

        AdjustListTitlePosition(gestureListTitle.transform, buttons.Count, recordMenuButtonHeight);

        // adjust new gesture button position
        float totalHeight = buttons.Count * recordMenuButtonHeight;
        float y = -(totalHeight / 2);
        newGestureButton.transform.localPosition = new Vector3(0, y, 0);
        
    }

    void GenerateNeuralNetMenuButtons()
    {
        int neuralNetMenuButtonHeight = 30;

        List<Button> buttons = GenerateButtonsFromList(lineCapturer.neuralNetList, selectNeuralNetMenu.transform, buttonPrefab, neuralNetMenuButtonHeight);

        // set the functions that the button will call when pressed
        for (int i = 0; i < buttons.Count; i++)
        {
            string neuralNetName = lineCapturer.neuralNetList[i];
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
            button.transform.position = Vector3.zero;
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

    void PanelFocusChanged (string panelName)
    {
        //Debug.Log("panel focus changed to: " + panelName);
    }

    void UpdateCurrentNeuralNetworkText()
    {
        if (GetCurrentNeuralNetworkText() == null)
            return;

        Text title = GetCurrentNeuralNetworkText();
        title.text = lineCapturer.neuralNetUsing;
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
