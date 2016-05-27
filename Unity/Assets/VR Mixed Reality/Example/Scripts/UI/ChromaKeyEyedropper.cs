using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace VRMixedReality.Examples.UI
{
    public class ChromaKeyEyedropper : MonoBehaviour
    {
        public UnityEngine.UI.Toggle toggle;
        public UnityEngine.UI.Image swatch;

        public GameObject cursorAttachment;
        public UnityEngine.UI.Image cursorSwatch;

        public GameObject unsetSignal;
        public GameObject setSignal;
        

        private bool eyedropping;
        private bool set;
        private Color col = Color.white;

        private bool cummulative;
        private List<Color> cols = new List<Color>();
        private bool firstClick = true;

        void OnEnable()
        {
            toggle.onValueChanged.AddListener(HandleToggled);
            HandleToggled(toggle.isOn);
        }
        void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(HandleToggled);
        }

        private void HandleToggled(bool on)
        {
            cursorAttachment.SetActive(on);
            eyedropping = on;
        }

        public void StartCummulativeEyedrop()
        {
            if (cols.Count == 0 && set)
                cols.Add(col);

            toggle.isOn = true;
            cummulative = true;
            firstClick = true;
        }
        public void ResetColors()
        {
            cols.Clear();
            if( MixedRealityController.Instance != null && MixedRealityController.Instance.cameraFeedMaterial != null )
                MixedRealityController.Instance.cameraFeedMaterial.SetVector("_channelLimits", new Vector4(0, 0, 0, 1));

            set = false;
        }

        void LateUpdate()
        {
            if (eyedropping)
            {
                UpdateColor();

                cursorAttachment.transform.position = Input.mousePosition;
                if (cursorSwatch != null)
                    cursorSwatch.color = col;

                swatch.color = col;

                if (!cummulative)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ApplyColor();
                        toggle.isOn = false;
                        set = true;
                    }
                        
                }
                else
                {
                    if (Input.GetMouseButton(0) && !firstClick)
                    {
                        cols.Add(col);
                        set = true;
                        ApplyMultiSample();
                    }
                    if( Input.GetMouseButtonUp(0) )
                    {
                        if (firstClick)
                            firstClick = false;
                        else
                        {
                            cummulative = false;
                            toggle.isOn = false;
                        }
                    }
                    
                }
            }
            else if( MixedRealityController.Instance != null && MixedRealityController.Instance.cameraFeedMaterial != null )
                swatch.color = MixedRealityController.Instance.cameraFeedMaterial.GetColor("_keyingColor");

            if (unsetSignal != null)
                unsetSignal.SetActive(!set);
            if (setSignal != null)
                setSignal.SetActive(set);
        }

        private void UpdateColor()
        {
            if (MixedRealityController.Instance == null || MixedRealityController.Instance.cameraFeedTexture == null)
                return;

            Texture camTex = MixedRealityController.Instance.cameraFeedTexture;

            float scaleFactor = (float)camTex.height / Screen.height;
            Vector2 texSpacePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y) * scaleFactor;

            if (camTex is Texture2D)
                col = (camTex as Texture2D).GetPixelBilinear(Mathf.Clamp01(texSpacePos.x / camTex.width), Mathf.Clamp01(texSpacePos.y / camTex.height));
            else if (camTex is WebCamTexture)
                col = (camTex as WebCamTexture).GetPixel(Mathf.Clamp((int)texSpacePos.x, 0, camTex.width), Mathf.Clamp((int)texSpacePos.y, 0, camTex.height));
            col.a = 1;
        }

        void ApplyColor()
        {
            if (MixedRealityController.Instance != null && MixedRealityController.Instance.cameraFeedMaterial != null)
                MixedRealityController.Instance.cameraFeedMaterial.SetColor("_keyingColor", col);
        }

        
        void ApplyMultiSample()
        {
            Vector3 min, max;
            min = max = GetHSV(cols[0]);
            for( int i = 1; i < cols.Count; i++ )
            {
                Vector3 hsv = GetHSV(cols[i]);
                min.x = Mathf.Min(hsv.x, min.x);
                min.y = Mathf.Min(hsv.y, min.y);
                min.z = Mathf.Min(hsv.z, min.z);
                max.x = Mathf.Max(hsv.x, max.x);
                max.y = Mathf.Max(hsv.y, max.y);
                max.z = Mathf.Max(hsv.z, max.z);
            }

            Vector3 meanHsv = Vector3.Lerp(min, max, 0.5f);
            if (Mathf.Abs(max.x - 1 - min.x) < max.x - min.x)
                meanHsv.x = (max.x + Mathf.Abs(max.x - 1 - min.x) * 0.5f) % 1;
            Color meanColor = GetRGB(meanHsv);
            
            MixedRealityController.Instance.cameraFeedMaterial.SetColor("_keyingColor", meanColor);

            Vector3 tolerances = 0.5f * (max - min);
            MixedRealityController.Instance.cameraFeedMaterial.SetVector("_channelLimits", tolerances);


        }

        Vector3 GetHSV(Color c)
        {
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            return new Vector3(h, s, v);
        }
        Color GetRGB(Vector3 hsv)
        {
            return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        }
    }
}
