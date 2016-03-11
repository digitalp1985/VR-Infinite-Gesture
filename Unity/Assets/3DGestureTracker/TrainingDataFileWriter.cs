using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrainingDataFileWriter {
    string filePath = "Assets/3DGestureTracker/TrainingData/";
    string fileName = "WriteLines.txt";

    public TrainingDataFileWriter ()
    {
        
    }

    public void DownResLine(List<Vector3> capturedLine)
    {
        //find min and max for X,Y,Z
        float minX =0, maxX =0, minY =0, maxY =0, minZ =0, maxZ = 0;


        foreach(Vector3 point in capturedLine)
        {
            if(point.x < minX)
            {
                minX = point.x;
            }
            if(point.x > maxX)
            {
                maxX = point.x;
            }

        }
    }

    //Pass in an array for data points.
    public void writeData(List<Vector3> capturedLine)
    {
        //[Serializable] public struct MyObjectArrayWrapper { public MyObject[] objects; }
        string dumbString = "[";
        foreach(Vector3 point in capturedLine)
        {
            dumbString += JsonUtility.ToJson(point);
            dumbString += ",";
        }
        dumbString.Substring(0, dumbString.Length - 1);
        dumbString += "]";

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath + fileName, true))
        {
            file.WriteLine(dumbString);
        }
    }
}
