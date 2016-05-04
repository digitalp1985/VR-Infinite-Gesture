using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WinterMute
{

    public class Trainer
    {
        //This class will create and train a NeuralNetwork.
        //maybe we pass in a Neural Network
        int numInput;
        int numHidden;
        int numOutput;

        //maybe the trainer is where we need an output of gestures
        List<string> outputs;
        string currentlyTraining;
        string recognizerName;

        NeuralNetwork neuralNetwork;

        string filePath = "Assets/3DGestureTracker/TrainingData/";
        string examplesFileName = "trainingExamples.txt";
        //WriteLines is my initial training file.

        public Trainer(List<string> gestureList, string name)
        {
            numInput = 33;
            numHidden = 10;
            numOutput = 3;
            recognizerName = name;

            outputs = gestureList;
        }

        //Just Capture Data
        //Pass in an array for data points.
        public void AddGestureToTrainingExamples(string gestureName, List<Vector3> capturedLine)
        {
            if (capturedLine.Count >= 11)
            {
                capturedLine = Utils.Instance.SubDivideLine(capturedLine);
                capturedLine = Utils.Instance.DownResLine(capturedLine);

                GestureExample saveMe = new GestureExample();
                saveMe.name = gestureName;
                saveMe.data = capturedLine;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath + examplesFileName, true))
                {
                    file.WriteLine(JsonUtility.ToJson(saveMe));
                }
            }
        }

        //Then Actually Train
        public void TrainRecognizer()
        {
            //Based on out list of outputs
            numOutput = outputs.Count;
            int seed = 1; // gives nice demo

            double[][] allData = ReadAllData(numInput, numHidden, numOutput, seed);

            double[][] trainData;
            double[][] testData;
            SplitTrainTest(allData, 0.80, seed, out trainData, out testData);

            neuralNetwork = new NeuralNetwork(numInput, numHidden, numOutput);

            int maxEpochs = 1000;
            double learnRate = 0.05;
            double momentum = 0.01;

            //Does this still weight properly if I train A SINGLE example at a time.
            double[] weights = neuralNetwork.Train(trainData, maxEpochs, learnRate, momentum);
            SaveNeuralNetwork(weights);
        }


        public double[][] ReadAllData(int numInput, int numHidden, int numOutput, int seed)
        {
            //read in the file

            string[] lines = System.IO.File.ReadAllLines(filePath + examplesFileName);

            double[][] readData = new double[lines.Length][];
            List<double[]> tmpAllData = new List<double[]>();

            foreach (string currentLine in lines)
            {
                GestureExample myObject = JsonUtility.FromJson<GestureExample>(currentLine);
                List<double> tmpLine = new List<double>();
                tmpLine.AddRange(myObject.GetAsArray());
                tmpLine.AddRange(CalculateOutputVector(myObject.name));

                tmpAllData.Add(tmpLine.ToArray());
            }

            return tmpAllData.ToArray();
        }


        public double[] CalculateOutputVector(string gestureName)
        {
            //Find index of gestureName;
            int gestureIndex = outputs.IndexOf(gestureName);
            
            //Create output of length numOutputs, zero it out.
            double[] output = new double[outputs.Count];
            for(int i=0; i< output.Length; i++)
            {
                output[i] = 0.0;
            }

            output[gestureIndex] = 1.0;

            return output;
        }

        void SplitTrainTest(double[][] allData, double trainPct, int seed, out double[][] trainData, out double[][] testData)
        {
            System.Random rnd = new System.Random(seed);
            int totRows = allData.Length;
            int numTrainRows = (int)(totRows * trainPct); // usually 0.80
            int numTestRows = totRows - numTrainRows;
            trainData = new double[numTrainRows][];
            testData = new double[numTestRows][];

            double[][] copy = new double[allData.Length][]; // ref copy of data
            for (int i = 0; i < copy.Length; ++i)
                copy[i] = allData[i];

            //This is duping rows. Not deep copy
            for (int i = 0; i < copy.Length; ++i) // scramble order
            {
                int r = rnd.Next(i, copy.Length); // use Fisher-Yates
                double[] tmp = copy[r];
                copy[r] = copy[i];
                copy[i] = tmp;
            }
            for (int i = 0; i < numTrainRows; ++i)
                trainData[i] = copy[i];

            for (int i = 0; i < numTestRows; ++i)
                testData[i] = copy[i + numTrainRows];
        } // SplitTrainTest

        public void SaveNeuralNetwork(double[] weights)
        {
            NeuralNetworkStub stub = new NeuralNetworkStub();
            stub.numInput = numInput;
            stub.numHidden = numHidden;
            stub.numOutput = numOutput;
            stub.gestures = outputs;
            stub.weights = weights;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath + recognizerName+".txt", true))
            {
                //file.WriteLine(dumbString);
                file.WriteLine(JsonUtility.ToJson(stub));
            }
            
        }


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}


