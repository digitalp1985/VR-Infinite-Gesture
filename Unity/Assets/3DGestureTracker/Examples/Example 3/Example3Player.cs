using UnityEngine;
using System.Collections;
using WinterMute;

public class Example3Player : MonoBehaviour
{
    public GameObject fire;
    public GameObject earth;
    public GameObject ice;
    public GameObject air;

    VRAvatar myAvatar;
    IInput input;

    Transform playerHead;
    Transform playerHandL;
    Transform playerHandR;

    void Start ()
    {
        myAvatar = PlayerManager.GetPlayerAvatar(0);

        playerHead = myAvatar.headTF;
        playerHandR = myAvatar.vrRigAnchors.rHandAnchor;
        playerHandL = myAvatar.vrRigAnchors.lHandAnchor;

        if (Config.handedness == Config.Handedness.Right)
        {
            input = myAvatar.GetInput(VROptions.Handedness.Right);
        }
        else if (Config.handedness == Config.Handedness.Left)
        {
            input = myAvatar.GetInput(VROptions.Handedness.Left);
        }
    }
	
	void Update ()
    {
	    
	}

    void OnEnable ()
    {
        VRGestureManager.GestureDetectedEvent += OnGestureDetected;
    }

    void OnDisable ()
    {
        VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
    }

    void OnGestureDetected (string gestureName, double confidence)
    {
        string confidenceString = confidence.ToString().Substring(0, 4);
        Debug.Log("detected gesture: " + gestureName + " with confidence: " + confidenceString);

        switch (gestureName)
        {
            case "Fire":
                DoFire();
                break;
            case "Earth":
                DoEarth();
                break;
            case "Ice":
                DoIce();
                break;
            case "Air":
                DoAir();
                break;
            case "Gravity":
                DoGravity();
                break;
            case "Pull":
                DoPull();
                break;
        }
    }

    void DoFire ()
    {
        Quaternion rotation = Quaternion.LookRotation(playerHandR.forward, Vector3.up);
        Vector3 betweenHandsPos = (playerHandL.position + playerHandR.position) / 2;
        GameObject.Instantiate(fire, betweenHandsPos, rotation);
    }

    void DoEarth ()
    {
        float explosionForce = 1000f;

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        float floorY = 2.65f;
        Vector3 earthSpawnPosition = new Vector3(playerHandR.position.x, floorY, playerHandR.position.z);
        GameObject.Instantiate(earth, earthSpawnPosition, rotation);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemies)
        {

            // if it's a ragdoll make non-kinematic
            if (enemy.GetComponent<Ragdoll>() != null)
            {
                Ragdoll ragdoll = enemy.GetComponent<Ragdoll>();
                ragdoll.TriggerWarning();
                foreach(Rigidbody rb in ragdoll.myParts)
                {
                    rb.AddExplosionForce(explosionForce, earthSpawnPosition, 100000f);
                }
            }

            else if (enemy.GetComponent<Rigidbody>() != null)
            {
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                rb.AddExplosionForce(explosionForce, earthSpawnPosition, 100000f);
            }
        }
    }

    void DoIce()
    {
        GameObject.Instantiate(ice, playerHandR.position, playerHandR.rotation);
    }

    void DoAir()
    {
        float explosionForce = 6f;

        Ray headRay = new Ray(playerHead.position, playerHead.forward);
        float sphereCastRadius = .5f;
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(headRay, sphereCastRadius);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Enemy"))
            {
                Transform enemy = hit.collider.transform;
                // spawn the explosion effect
                Vector3 airSpawnPosition = enemy.position;
                GameObject.Instantiate(air, airSpawnPosition, Quaternion.identity);

                // shoot the enemy up into the air

                // if it's a ragdoll make non-kinematic
                if (enemy.GetComponent<Ragdoll>() != null)
                {
                    Ragdoll ragdoll = enemy.GetComponent<Ragdoll>();
                    foreach (Rigidbody rb in ragdoll.myParts)
                    {
                        rb.AddForce(new Vector3(.3f, explosionForce * 2, .1f), ForceMode.Impulse);
                        StartCoroutine(IEDoAir(rb));
                    }
                }
                else if (enemy.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody rb = enemy.GetComponent<Rigidbody>();
                    rb.AddForce(new Vector3(.3f, explosionForce, .1f), ForceMode.Impulse);
                    StartCoroutine(IEDoAir(rb));
                }
            }
        }
    }

    void DoPull()
    {

    }

    IEnumerator IEDoAir (Rigidbody rb)
    {
        yield return new WaitForSeconds(.3f);
        rb.useGravity = false;
    }

    void DoGravity()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemies)
        {
            if (enemy.GetComponent<Rigidbody>() != null)
                enemy.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    IEnumerator AnimateShape (GameObject shape)
    {
        yield return null;
    }
}
