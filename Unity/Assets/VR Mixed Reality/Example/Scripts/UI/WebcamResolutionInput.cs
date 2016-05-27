using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace VRMixedReality.Examples.UI
{
    [RequireComponent(typeof(InputField))]
    public class WebcamResolutionInput : MonoBehaviour
    {
        public enum Dimension
        {
            Width, Height
        }
        public Dimension dimension;

        InputField input;
        MixedRealityWebcamSource src;

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
            if (src == null && MixedRealityController.Instance != null)
                src = MixedRealityController.Instance.GetComponent<MixedRealityWebcamSource>();

            if (MixedRealityController.Instance != null && !input.isFocused)
            {
                int val;
                if (!int.TryParse(input.text, out val) || val != GetValue())
                    input.text = GetValue().ToString();
            }
        }

        private void HandleValueChanged(string text)
        {
            if (MixedRealityController.Instance == null || src == null)
                return;

            int val;
            if (!int.TryParse(text, out val))
                return;

            if (val == GetValue())
                return;

            if (dimension == Dimension.Width)
                src.requestWidth = val;
            else
                src.requestHeight = val;

            src.enabled = false;
            src.enabled = true;
        }

        int GetValue()
        {
            if (src == null)
                return 0;
            switch (dimension)
            {
                case Dimension.Width:
                    return src.requestWidth;
                case Dimension.Height:
                    return src.requestHeight;
                default:
                    return 0;
            }
        }
    }
}
