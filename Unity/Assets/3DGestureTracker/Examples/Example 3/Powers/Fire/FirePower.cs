using UnityEngine;
using System.Collections;

public class FirePower : MonoBehaviour
{
    public float speed;
    Rigidbody rb;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate ()
    {
        Vector3 force = new Vector3(0, 0, speed);
        rb.AddRelativeForce(force, ForceMode.Impulse);
	}

    void OnCollisionEnter ()
    {
        Debug.Log("hit something");
        //Destroy(this);
    }
}
