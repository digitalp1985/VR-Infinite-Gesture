using UnityEngine;
using System.Collections;

namespace VRMixedReality.Examples
{
    public class SavedTransform : MonoBehaviour
    {
        public string prefKey = "trans";

        public bool savePosition;
        public bool saveRotation;
        public bool saveScale;

        void OnEnable()
        {
            if (!PlayerPrefs.HasKey(prefKey))
                return;

            string data = PlayerPrefs.GetString(prefKey);
            string[] values = data.Split('~');
            int expectedCount = 0;
            if (savePosition) expectedCount += 3;
            if (saveRotation) expectedCount += 3;
            if (saveScale) expectedCount += 3;

            if (values.Length != expectedCount)
                return;

            int index = 0;
            if (savePosition)
            {
                float x = float.Parse(values[index++]);
                float y = float.Parse(values[index++]);
                float z = float.Parse(values[index++]);
                transform.localPosition = new Vector3(x, y, z);
            }
            if (saveRotation)
            {
                float x = float.Parse(values[index++]);
                float y = float.Parse(values[index++]);
                float z = float.Parse(values[index++]);
                transform.localEulerAngles = new Vector3(x, y, z);
            }
            if (saveScale)
            {
                float x = float.Parse(values[index++]);
                float y = float.Parse(values[index++]);
                float z = float.Parse(values[index++]);
                transform.localScale = new Vector3(x, y, z);
            }
        }

        void OnDisable()
        {
            int expectedCount = 0;
            if (savePosition) expectedCount += 3;
            if (saveRotation) expectedCount += 3;
            if (saveScale) expectedCount += 3;

            string[] values = new string[expectedCount];
            int index = 0;
            if (savePosition)
            {
                values[index++] = transform.localPosition.x.ToString();
                values[index++] = transform.localPosition.y.ToString();
                values[index++] = transform.localPosition.z.ToString();
            }
            if (saveRotation)
            {
                values[index++] = transform.localEulerAngles.x.ToString();
                values[index++] = transform.localEulerAngles.y.ToString();
                values[index++] = transform.localEulerAngles.z.ToString();
            }
            if (saveScale)
            {
                values[index++] = transform.localScale.x.ToString();
                values[index++] = transform.localScale.y.ToString();
                values[index++] = transform.localScale.z.ToString();
            }
            PlayerPrefs.SetString(prefKey, string.Join("~", values));
        }
    }
}