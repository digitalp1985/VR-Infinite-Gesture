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
        #region AVATAR VARIABLES
        public VRGestureRig rig;
        #endregion

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

        void Init()
        {
            if (FindObjectOfType<VRGestureRig>() != null)
            {
                rig = FindObjectOfType<VRGestureRig>();
            }
        }

        void Start()
        {
            if (FindObjectOfType<VRGestureUI>() == null)
            {
                Debug.LogError("Cannot find VRGestureUI in scene. Please add it or select Begin In Detect Mode in the VR Gesture Manager Settings");
            }
            //rig.state = rig.stateInitial;
            //rig.stateLast = rig.state;
            //create a new Trainer
        }
        #endregion


    }
}