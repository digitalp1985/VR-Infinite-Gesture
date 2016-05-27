using UnityEngine;
using System.Collections;

namespace VRMixedReality
{
    [RequireComponent(typeof(MixedRealityController))]
    public class MixedRealityWebcamSource : MonoBehaviour
    {
        public string webcamName = "myWebcamId";
        public int requestWidth = 1920, requestHeight = 1080;
        public int requestFps = 30;

        private WebCamTexture tex;

        void Update()
        {
            if (tex != null)
                return;

            foreach (WebCamDevice dev in WebCamTexture.devices)
            {
                if (dev.name == webcamName)
                {
                    MixedRealityController controller = GetComponent<MixedRealityController>();
                    tex = new WebCamTexture(dev.name, requestWidth, requestHeight, requestFps);
                    controller.cameraFeedTexture = tex;

                    tex.Play();
                }
            }
        }

        void OnDisable()
        {
            if( tex != null )
            {
                tex.Stop();
                tex = null;
            }
        }
    }
}