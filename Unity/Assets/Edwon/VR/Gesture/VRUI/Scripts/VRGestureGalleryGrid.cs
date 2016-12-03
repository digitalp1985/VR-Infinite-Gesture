using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Edwon.VR.Gesture
{
    public class VRGestureGalleryGrid : MonoBehaviour
    {
        public List<GestureExample> examples; // the actual gesture examples
        public List<VRGestureGalleryExample> galleryExamples; // the UI representations of the samples

        public VRGestureGallery gallery; // the vr gesture gallery that owns me

        public void Init(VRGestureGallery _gallery, List<GestureExample> _examples)
        {
            gallery = _gallery;
            examples = _examples;

            GenerateGestureGallery();
        }

        void GenerateGestureGallery()
        {
            // go through all the gesture examples and draw them in a grid
            for (int i = 0; i < examples.Count; i++)
            {
                GameObject galleryExampleGO = Instantiate(gallery.examplePrefab.gameObject) as GameObject;
                galleryExampleGO.transform.parent = transform;
                galleryExampleGO.transform.localPosition = Vector3.zero;
                galleryExampleGO.transform.localRotation = Quaternion.identity;
                galleryExampleGO.name = "Example " + i;

                VRGestureGalleryExample galleryExample = galleryExampleGO.GetComponent<VRGestureGalleryExample>();
                galleryExamples.Add(galleryExample);
                galleryExample.Init(this, examples[i], i);
            }

            gallery.galleryState = VRGestureGallery.GestureGalleryState.Visible;
        }

        //void SetTitleText()
        //{
        //    Text titleText = gallery.title.GetComponentInChildren<Text>();
        //    if (!gallery.currentGesture.isSynchronous)
        //    {
        //        titleText.text = gallery.currentGesture.name + "\nSingle Handed";
        //    }
        //    else
        //    {
        //        titleText.text = gallery.currentGesture.name + "\nDouble Handed";
        //    }
        //}

        public void DestroySelf()
        {
            Debug.Log("destroy this grid");
            Destroy(gameObject);
        }
    }
}