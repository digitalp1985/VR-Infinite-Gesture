using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Edwon.VR.Input;
using UnityEditor;

namespace Edwon.VR.Gesture
{
    [RequireComponent(typeof (CanvasGroup))]
    public class VRGestureGallery : MonoBehaviour
    {
        public VRGestureGalleryGrid gridPrefab;
        public VRGestureGalleryExample examplePrefab;

        public List<VRGestureGalleryGrid> grids;
        public List<GestureExample> allExamples; // gesture examples for left and right hand


        public float distanceFromHead = 2f;
        public float gestureDrawSize = 0.1f; // world size of one gesture drawing
        public float gridUnitSize = 0.2f; // world size of one grid unit
        public int gridMaxColumns = 10;
        [HideInInspector]
        public Vector3 frameOffset;
        public float lineWidth = 0.01f;
        public Vector3 galleryPosition;
        private Vector3 galleryStartPosition;
        public float grabVelocity = 650f;

        public Gesture currentGesture;
        [HideInInspector]
        public string currentNeuralNet;

        public enum GestureGalleryState { Visible, NotVisible };
        [HideInInspector]
        public GestureGalleryState galleryState;

        Rigidbody galleryRB;

        Transform vrHand; // the hand to use to grab and move the gallery
        VRGestureRig rig;
        IInput vrHandInput;
        VRGestureUI vrGestureUI;

        [HideInInspector]
        public CanvasGroup canvasGroup;
        private GestureSettings gestureSettings;

        // INIT
        void Start()
        {
            gestureSettings = Utils.GetGestureSettings();

            galleryPosition = new Vector3(0, 90, 0);

            canvasGroup = GetComponent<CanvasGroup>();

            galleryStartPosition = transform.position;

            vrGestureUI = transform.parent.GetComponent<VRGestureUI>();

            galleryRB = GetComponent<Rigidbody>();

            galleryState = GestureGalleryState.NotVisible;

            frameOffset = new Vector3(0, gridUnitSize / 6 , -(gridUnitSize / 2));

            GetHands();
        }

        void GetHands()
        {
            //rig = VRGestureManager.Instance.rig;
            rig = VRGestureRig.GetPlayerRig(gestureSettings.playerID);
            vrHand = rig.GetHand(rig.mainHand);
            vrHandInput = rig.GetInput(rig.mainHand);
        }

        void RefreshGestureExamples()
        {
            allExamples = Utils.GetGestureExamples(currentGesture.name, currentNeuralNet);
            List<GestureExample> tmpList = new List<GestureExample>();
            foreach (GestureExample gesture in allExamples)
            {
                if (gesture.raw)
                {
                    gesture.data = Utils.SubDivideLine(gesture.data);
                    gesture.data = Utils.DownScaleLine(gesture.data);
                }

            }
        }

        void PositionGestureGallery()
        {
            // set position
            Vector3 forward = rig.head.forward;
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
            Vector3 position = rig.head.position + (forward * distanceFromHead);
            galleryRB.MovePosition( position );

            // set rotation
            Vector3 toHead = position - rig.head.position;
            Quaternion rotation = Quaternion.LookRotation(-toHead, Vector3.up);
            galleryRB.MoveRotation(rotation);
        }

        void DestroyGestureGallery()
        {
            // get all children
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);

            // destroy the rest
            children.ForEach(child => Destroy(child));

            galleryState = GestureGalleryState.Visible;
            galleryRB.MovePosition(galleryStartPosition);

            for (int i = grids.Count - 1; i >= 0; i++)
            {
                grids[i].DestroySelf();
            }

            grids.Clear();
        }

        void CreateGestureGalleryGrids()
        {
            // if double handed
            if (currentGesture.isSynchronous)
            {
                // create two grids for left and right
            }
            // if single handed
            else
            {
                // create one grid
                CreateGestureGalleryGrid(allExamples);
            }
        }

        void CreateGestureGalleryGrid(List<GestureExample> withExamples)
        {
            GameObject newGridGO = Instantiate(gridPrefab.gameObject);
            VRGestureGalleryGrid newGrid = newGridGO.GetComponent<VRGestureGalleryGrid>();

            newGridGO.transform.parent = transform;
            newGridGO.transform.position = transform.position;
            newGridGO.transform.rotation = transform.rotation;
            newGridGO.transform.localScale = Vector3.one;

            newGrid.Init(this, withExamples);
            grids.Add(newGrid);
        }

        public void DeleteGestureExample(GestureExample gestureExample, int lineNumber)
        {
            Utils.DeleteGestureExample(currentNeuralNet, gestureExample.name, lineNumber);
            allExamples.Remove(gestureExample);
            foreach(VRGestureGalleryGrid grid in grids)
            {
                // delete the gesture example in the grid
                if (grid.examples.Contains(gestureExample))
                {
                    int gestureExampleIndex = grid.examples.IndexOf(gestureExample);
                    grid.examples.RemoveAt(gestureExampleIndex);
                }

                // delete the gesture gallery example in the grid
                for (int i = grid.galleryExamples.Count - 1; i >= 0; i--)
                { 
                    if (grid.galleryExamples[i].example == gestureExample)
                    {
                        Destroy(grid.galleryExamples[i].gameObject);
                        grid.galleryExamples.RemoveAt(i);
                    }
                }
            }
        }

        // GRAB AND MOVE THE GALLERY

        void FixedUpdate()
        {
            FixedUpdateGrabAndMove();
        }

        Vector3 lastHandPos; // used to calculate velocity of the vrHand to move the gesture gallery
        
        void FixedUpdateGrabAndMove()
        {
            if (galleryState == GestureGalleryState.Visible)
            {
                if (vrHandInput.GetButton(InputOptions.Button.Trigger2))
                {
                    // ADD UP/DOWN/LEFT/RIGHT FORCE
                    Vector3 velocity = vrHand.position - lastHandPos;
                    Vector3 velocityFlat = Vector3.ProjectOnPlane(velocity, -transform.forward);
                    velocityFlat *= grabVelocity;
                    galleryRB.AddForce(velocityFlat);

                    // ADD Z SPACE FORCE
                    Vector3 zVelocity = Vector3.ProjectOnPlane(velocity, -transform.right); // flatten left/right
                    zVelocity = Vector3.ProjectOnPlane(zVelocity, transform.up); // flatten up/down
                    zVelocity *= grabVelocity; // multiply
                    galleryRB.AddForce(zVelocity);
                }
            }
            lastHandPos = vrHand.position;
        }

        // EVENTS

        void OnEnable()
        {
            VRGestureUIPanelManager.OnPanelFocusChanged += PanelFocusChanged;
            gestureSettings = Utils.GetGestureSettings();
        }

        void OnDisable()
        {
            VRGestureUIPanelManager.OnPanelFocusChanged -= PanelFocusChanged;
        }

        void PanelFocusChanged(string panelName)
        {
            if (panelName == "Editing Menu")
            {
                VRGestureUI.ToggleCanvasGroup(canvasGroup, true);
                currentGesture = rig.currentTrainer.CurrentGesture;
                currentNeuralNet = gestureSettings.currentNeuralNet;
                RefreshGestureExamples();
                PositionGestureGallery();
                CreateGestureGalleryGrids();
            }
            else if (panelName == "Edit Menu")
            {
                VRGestureUI.ToggleCanvasGroup(canvasGroup, false);
                DestroyGestureGallery();
            }

        }

    }
}
