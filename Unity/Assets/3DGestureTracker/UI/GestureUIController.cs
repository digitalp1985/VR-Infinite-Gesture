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
    [Tooltip("the label that tells you what gesture your recording currently")]
    public Text nowRecordingGestureLabel;
    [Tooltip("the ui text that should be updated with a gesture detect log")]
    public Text detectLog;

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
    }

    // events called by buttons when pressed

    public void BeginRecordMode()
    {
        Debug.Log("begin record mode");
        //EventManager.TriggerEvent("Record");
    }

    public void BeginDetectMode()
    {
        Debug.Log("begin detect mode");
        EventManager.TriggerEvent("Detect");
    }

    public void BeginRecordGesture(string gestureName)
    {
        Debug.Log("begin record gesture of type " + gestureName);
        nowRecordingGestureLabel.text = gestureName;
        EventManager.TriggerEvent("Record", gestureName);
    }

    // generate UI elements

    void GenerateRecordMenuButtons()
    {
        int recordMenuButtonHeight = 30;

        List<Button> buttons = GenerateButtonsFromList(lineCapturer.gestureList, recordMenu.transform, buttonPrefab, recordMenuButtonHeight);

        // set the functions that the button will call when pressed
        for (int i = 0; i < buttons.Count; i++)
        {
            string gestureName = lineCapturer.gestureList[i];
            buttons[i].onClick.AddListener(() => panelManager.FocusPanel("Recording Menu"));
            buttons[i].onClick.AddListener(() => BeginRecordGesture(gestureName));
        }

        AdjustRecordLabelPosition(recordMenuButtonHeight);
    }

    void GenerateNeuralNetMenuButtons()
    {
        int neuralNetMenuButtonHeight = 30;

        List<Button> buttons = GenerateButtonsFromList(lineCapturer.neuralNetList, selectNeuralNetMenu.transform, buttonPrefab, neuralNetMenuButtonHeight);


    }

    List<Button> GenerateButtonsFromList (List<string> list, Transform parent, GameObject prefab, int buttonHeight)
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

    void AdjustRecordLabelPosition (int buttonHeight)
    {
        if (recordLabel != null)
        {
            float totalHeight = lineCapturer.gestureList.Count * buttonHeight;
            float y = (totalHeight / 2) + buttonHeight;
            recordLabel.transform.localPosition = new Vector3(0, y, 0);
        }
    }

}
