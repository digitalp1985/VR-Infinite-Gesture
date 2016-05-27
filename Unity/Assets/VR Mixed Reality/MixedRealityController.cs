using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace VRMixedReality
{
    public class MixedRealityController : MonoBehaviour
    {
        public static MixedRealityController Instance { get; protected set; }

        [Header("Shaders")]
        public Shader bgColorClearShader;
        public Shader depthWriteShader;
        public Shader noFadeShadowShader;

        [Header("Game Parameters")]
        public Camera backgroundCamera;
        public Camera foregroundCamera;
        public float clippingPlaneOverlap = 0.01f;
        public int gameResolutionWidth = 1920, gameResolutionHeight = 1080;

        [Header("Camera Parameters")]
        public Texture cameraFeedTexture;
        public Material cameraFeedMaterial;
        public float cameraLagDelay = 0;
       
        [Header("Extras")]
        public LayerMask groundLayer;
        public bool useFootFix = true;
        public int renderFrequency = 3;     //Render at 90/3 FPS

        //Game view buffering
        private List<RenderTexture> backgroundBuffers = new List<RenderTexture>();
        private List<RenderTexture> foregroundBuffers = new List<RenderTexture>();
        private int bufferIndex = 0;

        //Blitting
        private Material blitBackgroundMat;
        private Material blitBackgroundWithoutAlpha;
        private Material blitForegroundMat;

        void OnEnable()
        {
            Instance = this;
        }
        void OnDisable()
        {
            if( Instance == this )
                Instance = null;
        }

        void Start()
        {
            blitBackgroundMat = new Material(Shader.Find("Unlit/Texture"));
            blitForegroundMat = new Material(Shader.Find("Unlit/Transparent"));
            blitBackgroundWithoutAlpha = new Material(bgColorClearShader);

            backgroundCamera.enabled = false;
            foregroundCamera.enabled = false;

            int targetFPS = 90;
            if (Application.targetFrameRate != -1)
                targetFPS = Application.targetFrameRate;
            int bufferFrameCount = 1 + Mathf.FloorToInt(cameraLagDelay * targetFPS / renderFrequency);
            for (int i = 0; i < bufferFrameCount; i++)
            {
                RenderTexture newBgTex = new RenderTexture(gameResolutionWidth, gameResolutionHeight, 24);
                newBgTex.generateMips = false;
                newBgTex.useMipMap = false;
                newBgTex.wrapMode = TextureWrapMode.Clamp;
                newBgTex.name = string.Format("BG Buffer [{0}]", i);
                backgroundBuffers.Add(newBgTex);

                RenderTexture newFgTex = new RenderTexture(gameResolutionWidth, gameResolutionHeight, 24);
                newFgTex.generateMips = false;
                newFgTex.useMipMap = false;
                newFgTex.wrapMode = TextureWrapMode.Clamp;
                newFgTex.name = string.Format("FG Buffer [{0}]", i);
                foregroundBuffers.Add(newFgTex);
            }
        }

        void OnPreRender()
        {
            UpdateCameraClipPlanes();

            DrawOldBuffersToScreen();

            if( Time.renderedFrameCount % 3 == 0 )
                RenderGameToBuffers();
        }

        void UpdateCameraClipPlanes()
        {
            //Calculate values for drawing/rendering later (floor height calculations for reference)
            if (Camera.main != null)
            {
                Vector3 playerPosition = Camera.main.transform.position;
                float distance = Vector3.Dot(foregroundCamera.transform.forward, playerPosition - foregroundCamera.transform.position);
                if (distance < 0.01f)
                    distance = 0.01f;
                foregroundCamera.farClipPlane = distance + clippingPlaneOverlap * 0.5f;
                backgroundCamera.nearClipPlane = distance - clippingPlaneOverlap * 0.5f;
            }
        }

        void DrawOldBuffersToScreen()
        {
            //start drawing to screen
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
            Rect fullScreenRect = new Rect(0, 0, Screen.width, Screen.height);

            //BACKGROUND
            Graphics.DrawTexture(fullScreenRect, backgroundBuffers[bufferIndex], blitBackgroundMat);

            Texture camTex = cameraFeedTexture;
            if (camTex != null)
            {
                float cameraAspect = (float)camTex.width / camTex.height;
                float screenCamWidth = cameraAspect * Screen.height;
                Graphics.DrawTexture(new Rect(0.5f * (Screen.width - screenCamWidth), 0, screenCamWidth, Screen.height), camTex, cameraFeedMaterial);
            }

            //FOREGROUND
            Graphics.DrawTexture(fullScreenRect, foregroundBuffers[bufferIndex], blitForegroundMat);

            GL.PopMatrix();
            //stop drawing to screen
        }

        void RenderGameToBuffers()
        {
            //Render game cameras to buffers
            backgroundCamera.targetTexture = backgroundBuffers[bufferIndex];
            backgroundCamera.Render();

            LayerMask oldFgMask = foregroundCamera.cullingMask;
            if (useFootFix)
            {
                //We also want the FG floor to live in the background buffer so that the real world stays above the floor.
                //To do this, we render the ground into the BG buffer twice, once with the BG clipping planes (3 lines up), and then again with the FG clipping planes. 
                foregroundCamera.targetTexture = backgroundBuffers[bufferIndex];
                foregroundCamera.cullingMask = groundLayer;
                foregroundCamera.clearFlags = CameraClearFlags.Depth;   //since depth buffer is normalized to clipping planes, depth doesn't translate to new camera. thankfully we don't need depth testing because we know FG>BG
                foregroundCamera.Render();
            }

            foregroundCamera.targetTexture = foregroundBuffers[bufferIndex];
            //Clear the foreground buffer with the background buffer's color and 0 alpha
            Graphics.SetRenderTarget(foregroundBuffers[bufferIndex]);
            UnityEngine.Rendering.CommandBuffer clearBuff = new UnityEngine.Rendering.CommandBuffer();
            clearBuff.ClearRenderTarget(true, true, new Color(0.5f, 0.5f, 0.5f, 0f));
            Graphics.ExecuteCommandBuffer(clearBuff);
            Graphics.Blit(backgroundBuffers[bufferIndex], foregroundBuffers[bufferIndex], blitBackgroundWithoutAlpha);

#if UNITY_5_4
            Shader originalScreenSpaceShadowShader = GraphicsSettings.GetCustomShader(UnityEngine.Rendering.BuiltinShaderType.ScreenSpaceShadows);
            GraphicsSettings.SetCustomShader(UnityEngine.Rendering.BuiltinShaderType.ScreenSpaceShadows, noFadeShadowShader);
#endif
            if (useFootFix)
            {
                //culling mask still ground from before
                foregroundCamera.clearFlags = CameraClearFlags.Depth;
                //We don't want to draw the ground in the foreground buffer, but we do want the area where the ground would have been drawn to show the BG through it, so we write to the depth buffer at the grounds position
                foregroundCamera.RenderWithShader(depthWriteShader, "");


                //Lastly, we let the foreground draw on top
                foregroundCamera.clearFlags = CameraClearFlags.Nothing;
                foregroundCamera.cullingMask = oldFgMask & ~groundLayer;
                foregroundCamera.Render();

                foregroundCamera.cullingMask = oldFgMask;
            }
            else
            {
                foregroundCamera.clearFlags = CameraClearFlags.Depth;
                foregroundCamera.cullingMask = oldFgMask;
                foregroundCamera.Render();
            }

#if UNITY_5_4
            GraphicsSettings.SetCustomShader(UnityEngine.Rendering.BuiltinShaderType.ScreenSpaceShadows, originalScreenSpaceShadowShader);
#endif

            bufferIndex++;
            if (bufferIndex >= backgroundBuffers.Count)
                bufferIndex = 0;
        }
    }
}