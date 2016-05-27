using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace VRMixedReality.Examples.UI
{
    [RequireComponent(typeof(Slider))]
    public class ChromaKeySoftnessSlider : MonoBehaviour
    {
        public enum Channel
        {
            Hue, Saturation, Value
        }

        public Channel channel;
        public Text valueLabel;
        public string formatStr = "{0}";

        Slider slider;

        void OnEnable()
        {
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(HandleValueChanged);
            HandleValueChanged(slider.value);
        }
        void OnDisable()
        {
            slider.onValueChanged.RemoveListener(HandleValueChanged);
        }

        void Update()
        {
            if (MixedRealityController.Instance != null && slider.value != GetValue())
                slider.value = GetValue();
        }

        private void HandleValueChanged(float val)
        {
            if (MixedRealityController.Instance == null)
                return;

            if(valueLabel != null)
                valueLabel.text = string.Format(formatStr, val);
            SetValue(val);
        }

        float GetValue()
        {
            if (MixedRealityController.Instance == null || MixedRealityController.Instance.cameraFeedMaterial == null)
                return 0;

            Vector4 feathers = MixedRealityController.Instance.cameraFeedMaterial.GetVector("_channelFeathers");
            switch (channel)
            {
                case Channel.Hue:
                    return feathers.x;
                case Channel.Saturation:
                    return feathers.y;
                case Channel.Value:
                    return feathers.z;
                default:
                    return 0;
            }
        }
        void SetValue(float val)
        {
            if (MixedRealityController.Instance == null || MixedRealityController.Instance.cameraFeedMaterial == null)
                return;

            Vector4 feathers = MixedRealityController.Instance.cameraFeedMaterial.GetVector("_channelFeathers");
            switch (channel)
            {
                case Channel.Hue:
                    feathers.x = val;
                    break;
                case Channel.Saturation:
                    feathers.y = val;
                    break;
                case Channel.Value:
                    feathers.z = val;
                    break;
                default:
                    return;
            }
            MixedRealityController.Instance.cameraFeedMaterial.SetVector("_channelFeathers", feathers);
        }
    }
}
