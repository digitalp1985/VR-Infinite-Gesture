using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Edwon.VR.Gesture
{
    public class GestureRecognizer
    {
        public delegate void GestureDetected(string gestureName, double confidence, HandType hand);
        public static event GestureDetected GestureDetectedEvent;
        public delegate void GestureRejected(string error, string gestureName = null, double confidence = 0);
        public static event GestureRejected GestureRejectedEvent;



        string lastLeftGesture;
        DateTime lastLeftDetected;
        string lastRightGesture;
        DateTime lastRightDetected;

        public double confidenceThreshold = 0.98;
        public double currentConfidenceValue;
        public double minimumGestureAxisLength = 0.1;

        List<Gesture> outputs;
        NeuralNetwork neuralNet;
        //save the array of gestures
        //This should always require a name to load.
        public GestureRecognizer(string filename)
        {
            Load(filename);
        }

        //Load a SavedRecognizer from a file
        public void Load(string filename)
        {
            NeuralNetworkStub stub = Utils.ReadNeuralNetworkStub(filename);
            outputs = stub.gestures;
            neuralNet = new NeuralNetwork(stub.numInput, stub.numHidden, stub.numOutput);
            neuralNet.SetWeights(stub.weights);
        }

        //Almost all of this should get plugged into Recognizer
        public void RecognizeLine(List<Vector3> capturedLine, HandType hand, VRGestureRig sender)
        {
            if (IsGestureBigEnough(capturedLine))
            {
                //Detect if the captured line meets minimum gesture size requirements
                double[] networkInput = Utils.FormatLine(capturedLine, hand);
                string gesture = GetGesture(networkInput);
                string confidenceValue = currentConfidenceValue.ToString("N3");

                // broadcast gesture detected event
                if (currentConfidenceValue > confidenceThreshold)
                {
                    if (GestureDetectedEvent != null)
                    {
                        GestureDetectedEvent(gesture, currentConfidenceValue, hand);
                        //Check if the other hand has recently caught a gesture.
                        //CheckForSyncGestures(gesture, hand);
                        if (hand == HandType.Left)
                        {
                            //leftCapture.SetRecognizedGesture(gesture);
                            lastLeftGesture = gesture;
                            lastLeftDetected = DateTime.Now;
                        }
                        else if (hand == HandType.Right)
                        {
                            //rightCapture.SetRecognizedGesture(gesture);
                            lastRightGesture = gesture;
                            lastRightDetected = DateTime.Now;

                        }

                        if (CheckForSync(gesture))
                        {
                            GestureDetectedEvent("BOTH: " + gesture, 2.0, hand);
                        }
                    }

                }
                else
                {
                    if (GestureRejectedEvent != null)
                        GestureRejectedEvent("Confidence Too Low", gesture, currentConfidenceValue);
                }
            }
            else
            {
                //broadcast that a gesture is too small??
                if (GestureRejectedEvent != null)
                    GestureRejectedEvent("Gesture is too small");
            }
        }

        public bool IsGestureBigEnough(List<Vector3> capturedLine)
        {
            float check = Utils.FindMaxAxis(capturedLine);
            return (check > minimumGestureAxisLength);
        }



        public bool CheckForSync(string gesture)
        {
            //Check the diff in time between left and right timestamps.
            TimeSpan lapse = lastLeftDetected.Subtract(lastRightDetected).Duration();
            TimeSpan limit = new TimeSpan(0, 0, 0, 0, 500);

            //if gesture starts with an R or an L.
            string gestureA = lastLeftGesture;
            string gestureB = lastRightGesture;
            if (gesture.Contains("L--") || gesture.Contains("R--"))
            {
                //strip the gesture
                gestureA = lastLeftGesture.Substring(2);
                gestureB = lastRightGesture.Substring(2);
            }


            if (gestureA == gestureB && lapse.CompareTo(limit) <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
















        public string GetGesture(double[] input)
        {
            double[] output = neuralNet.ComputeOutputs(input);

            string actualDebugOutput = "[";
            for(int i=0; i<output.Length; i++)
            {
                actualDebugOutput += output[i];
                actualDebugOutput += ", ";
            }
            actualDebugOutput = actualDebugOutput.Substring(0, actualDebugOutput.Length - 2);
            actualDebugOutput += "]";

            //Debug.Log(actualDebugOutput);
            return GetGestureFromVector(output);
        }

        public string GetGestureFromVector(double[] outputVector)
        {
            //find max index
            int maxIndex = 0;
            double maxVal = 0;
            for (int i = 0; i < outputVector.Length; i++)
            {
                if (outputVector[i] > maxVal)
                {
                    maxIndex = i;
                    maxVal = outputVector[i];
                }
            }
            //maxVal is the confidence value.
            //Debug.Log("Confidence Value: "+ maxVal);
            currentConfidenceValue = maxVal;
            return outputs[maxIndex].name;
        }

        //public double[] ConvertGestureToVector(string gestureName)
        //{
        //    int vectorIndex = outputs.IndexOf(gestureName);
        //    double[] outputVector = new double[outputs.Count];
        //    for(int i = 0; i < outputVector.Length; i++)
        //    {
        //        outputVector[i] = 0;
        //    }
        //    outputVector[vectorIndex] = 1;
        //    return outputVector;
        //}

        //public string ConvertVectorToGesture(double[] outputVector)
        //{
        //    //Find maxIndex
        //    int maxIndex = 0;
        //    double maxValue = 0;
        //    for (int i = 0; i < outputVector.Length; i++)
        //    {
        //        if(outputVector[i] > maxValue)
        //        {
        //            maxValue = outputVector[i];
        //            maxIndex = i;
        //        }
        //    }
        //    return outputs[maxIndex];
        //}

    }

}

