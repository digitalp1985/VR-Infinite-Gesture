using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace WinterMute
{
    public class GestureRecognizer
    {
        List<string> outputs;
        NeuralNetwork neuralNet;
        //save the array of gestures

        public GestureRecognizer(int gestureLength, List<string> gestureList)
        {
            int numInputs = gestureLength * 3;
            outputs = gestureList;

            int numOutputs = outputs.Count;
        }

        //Load a SavedRecognizer from a file
        public void Load()
        {

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

            Debug.Log(outputs[maxIndex]+" : " + outputVector[maxIndex] * 100 + "%");
            return outputs[maxIndex];
        }


        public double[] ConvertGestureToVector(string gestureName)
        {
            int vectorIndex = outputs.IndexOf(gestureName);
            double[] outputVector = new double[outputs.Count];
            for(int i = 0; i < outputVector.Length; i++)
            {
                outputVector[i] = 0;
            }
            outputVector[vectorIndex] = 1;
            return outputVector;
        }

        public string ConvertVectorToGesture(double[] outputVector)
        {
            //Find maxIndex
            int maxIndex = 0;
            double maxValue = 0;
            for (int i = 0; i < outputVector.Length; i++)
            {
                if(outputVector[i] > maxValue)
                {
                    maxValue = outputVector[i];
                    maxIndex = i;
                }
            }
            return outputs[maxIndex];
        }

    }

}

