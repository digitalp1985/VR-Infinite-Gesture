using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GestureUIController : MonoBehaviour
{
    public VROptions.Handedness handedness;
    private PanelManager panelManager;
    VRHandBase uiHand;
    Camera uiCam;
    public float offsetZ;

    public LineCapture lineCapturer; // the LineCapture script we want to interact with
    public RectTransform recordMenu; // the top level transform of the recordMenu where we will generate gesture buttons
    public GameObject gestureButtonPrefab;
    public float buttonHeight = 30f; // the distance between buttons
    [Tooltip ("the label that tells people to pick a gesture in the gesture record menu")]
    public CanvasRenderer recordLabel;

	void Start ()
    {
        // get vr player hand and camera
        uiHand = PlayerManager.GetPlayerHand(0, handedness);
        uiCam = PlayerManager.GetPlayerCamera(0);

        panelManager = transform.GetComponentInChildren<PanelManager>();

        GenerateRecordMenuButtons();
        AdjustRecordLabelPosition();
    }
	
	void Update ()
    {
        Vector3 handToCamVector = uiCam.transform.position - uiHand.transform.position;
        transform.position = uiHand.transform.position + (offsetZ * handToCamVector);
        transform.rotation = Quaternion.LookRotation(transform.position - uiCam.transform.position);
    }

    // sends events to the gesture system ( LineCapture )

    public void BeginRecordMode ()
    {
        Debug.Log("begin record mode");
    }

    public void BeginDetectMode ()
    {
        Debug.Log("begin detect mode");
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
            // set the function that the button will call when pressed
            button.GetComponent<Button>().onClick.AddListener( () => { panelManager.FocusPanel("Recording Menu"); });
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
