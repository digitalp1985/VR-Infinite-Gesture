using UnityEngine;
using System.Collections;
using System.IO;

namespace VRMixedReality
{
    [RequireComponent(typeof(MixedRealityController))]
    public class SavedMixedReality : MonoBehaviour
    {
        //Serialized structure is:
        //{cameraName}
        //{cameraFOV}
        //{cameraResolutionX},{cameraResolutionY}
        //{chromakeyColorR},{chromakeyColorG},{chromakeyColorB}
        //{chromakeyLimitH},{chromakeyLimitS},{chromakeyLimitV}
        //{chromakeySoftnessH},{chromakeySoftnessS},{chromakeySoftnessV}

        public string playerPrefKey = "mixedRealityParameters";

        void OnEnable()
        {
            if (!playerPrefKey.Contains(playerPrefKey))
                return;

            string str = PlayerPrefs.GetString(playerPrefKey);
            StringReader strReader = new StringReader(str);
            MixedRealityController controller = GetComponent<MixedRealityController>();

            MixedRealityWebcamSource src = GetComponent<MixedRealityWebcamSource>();

            if( src != null )
            {
                src.webcamName = strReader.ReadLine();
                controller.foregroundCamera.fieldOfView = controller.backgroundCamera.fieldOfView = float.Parse(strReader.ReadLine());

                string[] resStrs = strReader.ReadLine().Split(',');
                src.requestWidth = int.Parse(resStrs[0]);
                src.requestHeight = int.Parse(resStrs[1]);

                src.enabled = false;
                src.enabled = true;
            }

            Material chromaKeyMat = controller.cameraFeedMaterial;

            string[] colStrs = strReader.ReadLine().Split(',');
            chromaKeyMat.SetColor("_keyingColor", new Color(float.Parse(colStrs[0]), float.Parse(colStrs[1]), float.Parse(colStrs[2]), 1));

            string[] limStrs = strReader.ReadLine().Split(',');
            chromaKeyMat.SetVector("_channelLimits", new Vector4(float.Parse(limStrs[0]), float.Parse(limStrs[1]), float.Parse(limStrs[2]), 1));

            string[] softStrs = strReader.ReadLine().Split(',');
            chromaKeyMat.SetVector("_channelFeathers", new Vector4(float.Parse(softStrs[0]), float.Parse(softStrs[1]), float.Parse(softStrs[2]), 1));
        }
        void OnDisable()
        {
            StringWriter strWriter = new StringWriter();

            MixedRealityController controller = GetComponent<MixedRealityController>();

            MixedRealityWebcamSource src = GetComponent<MixedRealityWebcamSource>();

            if (src != null)
            {
                strWriter.WriteLine(src.webcamName);
                strWriter.WriteLine(controller.foregroundCamera.fieldOfView);

                strWriter.WriteLine(src.requestWidth + "," + src.requestHeight);
            }

            Material chromaKeyMat = controller.cameraFeedMaterial;

            Color keyCol = chromaKeyMat.GetColor("_keyingColor");
            strWriter.WriteLine(keyCol.r + "," + keyCol.g + "," + keyCol.b);

            Vector4 limits = chromaKeyMat.GetVector("_channelLimits");
            strWriter.WriteLine(limits.x + "," + limits.y + "," + limits.z);

            Vector4 softs = chromaKeyMat.GetVector("_channelFeathers");
            strWriter.WriteLine(softs.x + "," + softs.y + "," + softs.z);


            PlayerPrefs.SetString(playerPrefKey, strWriter.ToString());
        }
    }
}