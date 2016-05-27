using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace VRMixedReality.Examples.UI
{
    [RequireComponent(typeof(Dropdown))]
    public class DeviceDropdown : MonoBehaviour
    {
        public float pollCooldown = 1f;

        Dropdown dropdown;

        void Awake()
        {
            dropdown = GetComponent<Dropdown>();
            dropdown.ClearOptions();
        }
        void OnEnable()
        {
              
            InvokeRepeating("Poll", 0, pollCooldown);
            dropdown.onValueChanged.AddListener(HandleValueChanged);
        }
        void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(HandleValueChanged);
            CancelInvoke();
        }

        void Poll()
        {
            int oldCount = dropdown.options.Count;
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach( WebCamDevice device in WebCamTexture.devices )
            {
                options.Add(new Dropdown.OptionData(device.name));
            }
            dropdown.options = options;

            if (oldCount == 0 && options.Count > 0)
                HandleValueChanged(0);
        }

        void HandleValueChanged(int index)
        {
            if (MixedRealityController.Instance == null || index < 0 || index >= dropdown.options.Count)
                return;

            MixedRealityWebcamSource source = MixedRealityController.Instance.GetComponent<MixedRealityWebcamSource>();
            source.webcamName = dropdown.options[index].text;

            source.enabled = false;
            source.enabled = true;  //Refresh
        }
    }
}