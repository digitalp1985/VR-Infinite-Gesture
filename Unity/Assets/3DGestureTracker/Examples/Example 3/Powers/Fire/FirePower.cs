using UnityEngine;
using System.Collections;

public class FirePower : MonoBehaviour
{
    public float speed;
    Rigidbody rb;
    public float timeTillDeath;

    public GameObject fireExplosion;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 force = new Vector3(0, 0, speed);
        rb.AddRelativeForce(force, ForceMode.Impulse);
	}
	
	void FixedUpdate ()
    {

	}

    void OnCollisionEnter (Collision collision)
    {
        GameObject.Instantiate(fireExplosion, collision.contacts[0].point, Quaternion.identity);
        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(timeTillDeath);
        Destroy(gameObject);
    }

}
