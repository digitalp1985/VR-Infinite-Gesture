using UnityEngine;
using System.Collections;

public class ManagerSpawner : MonoBehaviour {


	public GameObject vrDebugManager;
	// Use this for initialization
	void Start () {
		Instantiate(vrDebugManager);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
