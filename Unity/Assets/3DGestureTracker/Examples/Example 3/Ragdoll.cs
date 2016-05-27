using UnityEngine;
using System.Collections;

public class Ragdoll : MonoBehaviour {

    bool isLimp = false;

    // Use this for initialization
	void Start () {
        //Turn off all isKinematics
        Rigidbody[] fart = GetComponentsInChildren<Rigidbody>();
        Collider[] poo = GetComponentsInChildren<Collider>();

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            //rb.isKinematic = true;
        }
        foreach(Collider bc in GetComponentsInChildren<Collider>())
        {
            //bc.isTrigger = true;
        }

        isLimp = false;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider c)
    {
        Debug.Log("I collided!!!");

        if (!isLimp)
        {
            foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }
            foreach (Collider bc in GetComponentsInChildren<Collider>())
            {
                bc.isTrigger = false;
            }
            isLimp = true;
        }

    }
}
