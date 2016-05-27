using UnityEngine;
using System.Collections;

public class FirePowerExplosion : MonoBehaviour {

    public float timeTillDeath;

	void Start ()
    {
        StartCoroutine(DestroySelf());
	}
	
	void Update ()
    {
	
	}

    IEnumerator DestroySelf ()
    {
        yield return new WaitForSeconds(timeTillDeath);
        Destroy(gameObject);
    }
}
