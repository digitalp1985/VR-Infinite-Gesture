using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WinterMute;

public class TrainingDataFileWriter {
    string filePath = "Assets/3DGestureTracker/TrainingData/";
    string fileName = "WriteLines.txt";
    Utils utilHelper;

    public TrainingDataFileWriter ()
    {
        utilHelper = Utils.Instance;
    }




    public double[] FormatLine(List<Vector3> capturedLine)
    {
        capturedLine = utilHelper.SubDivideLine(capturedLine);
        capturedLine = utilHelper.DownResLine(capturedLine);
        List<double> tmpLine = new List<double>();
        foreach(Vector3 cVector in capturedLine)
        {
            tmpLine.Add(cVector.x);
            tmpLine.Add(cVector.y);
            tmpLine.Add(cVector.z);
        }
        return tmpLine.ToArray();

    }


    //Pass in an array for data points.
    public void writeData(List<Vector3> capturedLine)
    {
        if (capturedLine.Count >= 11)
        {
            capturedLine = utilHelper.SubDivideLine(capturedLine);
            capturedLine = utilHelper.DownResLine(capturedLine);

            GestureExample test = new GestureExample();
            test.name = "line";
            test.data = capturedLine;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath + fileName, true))
            {
                //file.WriteLine(dumbString);
                file.WriteLine(JsonUtility.ToJson(test));
            }
        }
    }

    public List<GestureExample> getGallery()
    {
        //read in the file
        string[] lines = System.IO.File.ReadAllLines(filePath + fileName);
        List<GestureExample> gestures = new List<GestureExample>();
        foreach(string currentLine in lines)
        {
            gestures.Add(JsonUtility.FromJson<GestureExample>(currentLine));
        }
        return gestures;
    }

    public double[] GetWeights()
    {
        string[] lines = System.IO.File.ReadAllLines(filePath + "Weights.txt");
        ////System.IO.File.
        string inputLine = lines[0];

        WeightWrap fool = JsonUtility.FromJson<WeightWrap>(inputLine);
        double[] weights = fool.data;
        return weights;
    }

}





[Serializable]
public class GestureExample
{
    public string name;
    public List<Vector3> data;
}

[Serializable]
public class WeightWrap
{
    public double[] data;
}

[Serializable]
public class NeuralNetworkStub
{
    public int numInput;
    public int numHidden;
    public int numOutput;
    public double[] weights;
}
