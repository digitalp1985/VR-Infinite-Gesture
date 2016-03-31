using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WinterMute
{
    public class Utils
    {
        private static Utils instance;
        string filePath = "Assets/3DGestureTracker/TrainingData/";

        //constructor
        private Utils() { }

        public static Utils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Utils();
                }
                return instance;
            }
        }


        public List<Vector3> DownResLine(List<Vector3> capturedLine)
        {
            //find min and max for X,Y,Z
            float minX, maxX, minY, maxY, minZ, maxZ;
            //init all defaults to first point.
            Vector3 firstPoint = capturedLine[0];
            minX = maxX = firstPoint.x;
            minY = maxY = firstPoint.y;
            minZ = maxZ = firstPoint.z;

            foreach (Vector3 point in capturedLine)
            {
                minX = getMin(minX, point.x);
                maxX = getMax(maxX, point.x);

                minY = getMin(minY, point.y);
                maxY = getMax(maxY, point.y);

                minZ = getMin(minZ, point.z);
                maxZ = getMax(maxZ, point.z);
            }

            //we now have all of our mins and max
            float distX = Mathf.Abs(maxX - minX);
            float distY = Mathf.Abs(maxY - minY);
            float distZ = Mathf.Abs(maxZ - minZ);

            //This should make all of our lowest points start at the origin.
            Matrix4x4 translate = Matrix4x4.identity;
            translate[0, 3] = -minX;
            translate[1, 3] = -minY;
            translate[2, 3] = -minZ;

            Matrix4x4 scale = Matrix4x4.identity;
            scale[0, 0] = 1 / distX;
            scale[1, 1] = 1 / distY;
            scale[2, 2] = 1 / distZ;


            List<Vector3> localizedLine = new List<Vector3>();
            foreach (Vector3 point in capturedLine)
            {
                //we translate, but maybe we also divide each by the dist?
                Vector3 newPoint = translate.MultiplyPoint3x4(point);
                newPoint = scale.MultiplyPoint3x4(newPoint);
                localizedLine.Add(newPoint);
            }
            //capture way less points
            return localizedLine;
            // ok now we need to create a matrix to move all of these points to a normalized space.
        }

        public float getMin(float min, float newMin)
        {
            if (newMin < min) { min = newMin;}
            return min;
        }

        public float getMax(float max, float newMax)
        {
            if (newMax > max) { max = newMax;}
            return max;
        }

        public List<Vector3> SubDivideLine(List<Vector3> capturedLine)
        {
            //Make sure list is longer than 11.
            int outputLength = 11;

            float intervalFloat = Mathf.Round((capturedLine.Count * 1f) / (outputLength * 1f));
            int interval = (int)intervalFloat;
            List<Vector3> output = new List<Vector3>();

            for (int i = capturedLine.Count - 1; output.Count < outputLength; i -= interval)
            {
                if (i > 0) { output.Add(capturedLine[i]);}
                else { output.Add(capturedLine[0]); }
            }
            return output;
        }


        //Format line for NeuralNetwork
        public double[] FormatLine(List<Vector3> capturedLine)
        {
            capturedLine = SubDivideLine(capturedLine);
            capturedLine = DownResLine(capturedLine);
            List<double> tmpLine = new List<double>();
            foreach (Vector3 cVector in capturedLine)
            {
                tmpLine.Add(cVector.x);
                tmpLine.Add(cVector.y);
                tmpLine.Add(cVector.z);
            }
            return tmpLine.ToArray();
        }

        public void ReadWeights()
        {

        }

        public void SaveFile()
        {

        }

        public double[] GetWeights()
        {
            string[] lines = System.IO.File.ReadAllLines(filePath + "Weights.txt");
            ////System.IO.File.
            string inputLine = lines[0];

            WeightWrap fool = JsonUtility.FromJson<WeightWrap>(inputLine);
            double[] weights = fool.data;
            return weights;

            //We should save the entire Neural Network, Inputs, hidden layer, outputs.
        }



    }

}


