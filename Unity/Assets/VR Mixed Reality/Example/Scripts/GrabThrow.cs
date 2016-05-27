using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRMixedReality.Examples
{
    [RequireComponent(typeof(SteamVR_TrackedObject))]
    public class GrabThrow : MonoBehaviour
    {
        public Rigidbody attachPoint;

        private List<GameObject> touchingObjects = new List<GameObject>();

        SteamVR_TrackedObject trackedObj;
        FixedJoint joint;

        void Awake()
        {
            trackedObj = GetComponent<SteamVR_TrackedObject>();
        }
        void OnTriggerEnter(Collider col)
        {

            //Add to list
            if (!touchingObjects.Contains(col.gameObject))
                touchingObjects.Add(col.gameObject);
        }

        void OnTriggerExit(Collider col)
        {
            touchingObjects.Remove(col.gameObject);
        }

        void FixedUpdate()
        {
            var device = SteamVR_Controller.Input((int)trackedObj.index);
            if (joint == null && device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && touchingObjects.Count > 0)
            {

                var go = touchingObjects[touchingObjects.Count - 1];

                //go.transform.position = attachPoint.transform.position;

                joint = go.AddComponent<FixedJoint>();
                joint.connectedBody = attachPoint;

            }
            else if (joint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                var go = joint.gameObject;
                var rigidbody = go.GetComponent<Rigidbody>();
                Object.DestroyImmediate(joint);
                joint = null;

                // We should probably apply the offset between trackedObj.transform.position
                // and device.transform.pos to insert into the physics sim at the correct
                // location, however, we would then want to predict ahead the visual representation
                // by the same amount we are predicting our render poses.

                var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
                if (origin != null)
                {
                    rigidbody.velocity = origin.TransformVector(device.velocity);
                    rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
                }
                else
                {
                    rigidbody.velocity = device.velocity;
                    rigidbody.angularVelocity = device.angularVelocity;
                }

                rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
            }
        }
    }
}