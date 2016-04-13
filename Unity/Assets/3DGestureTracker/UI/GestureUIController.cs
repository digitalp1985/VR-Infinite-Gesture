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
    public GameObject gestureButtonPrefab;
    public float buttonHeight = 30f; // the distance between buttons
    [Tooltip("the label that tells people to pick a gesture in the gesture record menu")]
    public CanvasRenderer recordLabel;
    [Tooltip("the label that tells you what gesture your recording currently")]
    public Text nowRecordingGestureLabel;
    [Tooltip("the ui text that should be updated with a gesture detect log")]
    public Text detectLog;

    void Start()
    {
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
        AdjustRecordLabelPosition();
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
    }

    public void BeginDetectMode()
    {
        Debug.Log("begin detect mode");
    }

    public void BeginRecordGesture(string gestureName)
    {
        Debug.Log("begin record gesture of type " + gestureName);
        nowRecordingGestureLabel.text = gestureName;
    }

    // generate UI elements

    void GenerateRecordMenuButtons()
    {
        List<string> gestureList = lineCapturer.gestureList;
        for (int i = 0; i < gestureList.Count; i++)
        {
            // instantiate the button
            GameObject button = GameObject.Instantiate(gestureButtonPrefab);
            button.transform.parent = recordMenu.transform;
            button.transform.position = Vector3.zero;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.localScale = new Vector3(0.6666f, 1, 0.2f);
            button.transform.name = gestureList[i] + " Button";
            // set the button y position
            float totalHeight = gestureList.Count * buttonHeight;
            float y = 0f;
            if (i == 0)
            {
                y = totalHeight / 2;
            }
            y = (totalHeight/2) - (i * 30);
            rect.localPosition = new Vector3(0, y, 0);
            // set the button text
            Text buttonText = button.transform.GetComponentInChildren<Text>();
            buttonText.text = gestureList[i];
            // set the functions that the button will call when pressed
            string gestureName = gestureList[i];
            button.GetComponent<Button>().onClick.AddListener(() => panelManager.FocusPanel("Recording Menu"));
            button.GetComponent<Button>().onClick.AddListener(() => BeginRecordGesture(gestureName) ); 
        }
    }

    void AdjustRecordLabelPosition ()
    {
        if (recordLabel != null)
        {
            float totalHeight = lineCapturer.gestureList.Count * buttonHeight;
            float y = (totalHeight / 2) + buttonHeight;
            recordLabel.transform.localPosition = new Vector3(0, y, 0);
        }
    }

}
