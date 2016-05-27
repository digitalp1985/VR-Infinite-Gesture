using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace VRMixedReality.Examples.UI
{
    [RequireComponent(typeof(InputField))]
    public class WebcamFovInput : MonoBehaviour
    {
        InputField input;

        void OnEnable()
        {
            input = GetComponent<InputField>();
            input.onEndEdit.AddListener(HandleValueChanged);
            HandleValueChanged(input.text);
        }
        void OnDisable()
        {
            input.onEndEdit.RemoveListener(HandleValueChanged);
        }

        void Update()
        {
            if (MixedRealityController.Instance != null && !input.isFocused)
            {
                float val;
                if (!float.TryParse(input.text, out val) || !Mathf.Approximately(val,MixedRealityController.Instance.foregroundCamera.fieldOfView))
                    input.text = MixedRealityController.Instance.foregroundCamera.fieldOfView.ToString();
            }
        }

        private void HandleValueChanged(string text)
        {
            if (MixedRealityController.Instance == null)
                return;

            float val;
            if (!float.TryParse(text, out val))
                return;
            MixedRealityController.Instance.foregroundCamera.fieldOfView = val;
            MixedRealityController.Instance.backgroundCamera.fieldOfView = val;
        }
    }
}
