using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gallery : MonoBehaviour {

	List<GameObject> renderPool;

	string filePath = "Assets/3DGestureTracker/TrainingData/";
	string fileName = "WriteLines.txt";
	// Use this for initialization
	void Start () {
		renderPool = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public List<GestureExample> GetGallery()
	{
		//read in the file
		string[] lines = System.IO.File.ReadAllLines(filePath + fileName);
		List<GestureExample> gestures = new List<GestureExample>();
		foreach (string currentLine in lines)
		{
			gestures.Add(JsonUtility.FromJson<GestureExample>(currentLine));
		}
		return gestures;
	}


	//@deprecated
	public void buildGallery()
	{
		List<GestureExample> galleryList = GetGallery();

		float x = 0;
		foreach (GestureExample gesture in galleryList)
		{
			Vector3 hang = new Vector3(x, 1, x);
			DrawPortrait(gesture.data, hang);
			x += .2f;
		}
	}

	//@deprecated
	public void DrawPortrait(List<Vector3> capturedLine, Vector3 startCoords)
	{
		Debug.Log(startCoords);
		GameObject tmpObj = new GameObject();
		tmpObj.transform.SetParent(transform);
		tmpObj.transform.position = startCoords;

		List<Vector3> tmpArray = new List<Vector3>();
		foreach (Vector3 currentPoint in capturedLine)
		{
			tmpArray.Add(tmpObj.transform.InverseTransformPoint(currentPoint));
		}

		//renderPool.Add(tmpObj);

		LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors(Color.red, Color.red);
		lineRenderer.SetWidth(0.1F, 0.1F);
		lineRenderer.SetVertexCount(tmpArray.Count);
		lineRenderer.SetPositions(tmpArray.ToArray());

	}

	//@deprecated
	public void DrawLine(List<Vector3> capturedLine)
	{
		GameObject tmpObj = new GameObject();
		tmpObj.transform.parent = transform;
		tmpObj.transform.localPosition = Vector3.zero;
		renderPool.Add(tmpObj);

		LineRenderer lineRenderer = tmpObj.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors(Color.red, Color.red);
		lineRenderer.SetWidth(0.01F, 0.05F);
		lineRenderer.SetVertexCount(capturedLine.Count);
		lineRenderer.SetPositions(capturedLine.ToArray());
	}
}
