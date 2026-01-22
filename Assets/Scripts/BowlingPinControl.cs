using System.Runtime.InteropServices;
using UnityEngine;

public class BowlingPinControl : MonoBehaviour
{
    private Rigidbody pinRb;
    private Collider pinColl;
    public float highAngularDrang = 1f;
    public float lowAngularDrag = 0.05f;
    public float breakAngle = 8f;
    public bool hasMoved = false;
    private PlayerController playerControllerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        pinRb = GetComponent<Rigidbody>();
        pinColl = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        Sleep();
        ConditionalAngularDamping();
        if (playerControllerScript.spaceReleased)
            FrictionScaler();
    }

    void OnTriggerEnter(Collider other)
    {   
        //if collides with the freeze layer, freeze the pins movements so they don't move endlessely
        if (other.CompareTag("Freeze Layer"))
        {
            pinRb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    //stop bowling pins from infinetly rotating and moving by sleeping (stop calculating physics and setting velocities to zero) them when their velocities get low
    void Sleep()
    {
        if (pinRb.linearVelocity.magnitude < 0.5f || pinRb.angularVelocity.magnitude < 0.5f)
        {
            //pinRb.Sleep();
            pinRb.linearVelocity = Vector3.zero;
            pinRb.angularVelocity = Vector3.zero;
            //print("Sleeping!");
        }
        else
        {
            pinRb.WakeUp();
        }
    }

    void ConditionalAngularDamping()
    {
        float tilt = Vector3.Angle(transform.up, Vector3.up);

        if (tilt < breakAngle && !hasMoved)
        {
            pinRb.angularDamping = highAngularDrang;
        }
        else
        {
            pinRb.angularDamping = lowAngularDrag;
            hasMoved = true;
            //print("hasMoved: " + hasMoved + gameObject.name);
            
        }
    }

    void FrictionScaler()
    {
        //assume speedRounded can be 10 - 50 km/h
        //lower speed -> higher friction
        //higher speed -> lower friction
        //(speedRounded - 10) / (50 - 10) = range from 0 to 1 based on range 10 - 50
        float max = 50f;
        float min = 10f;
        float percent = (playerControllerScript.speedRounded - min) / (max-min);
        percent = Mathf.Abs(percent-1f);
        pinColl.material.dynamicFriction = percent;
        pinColl.sharedMaterial.staticFriction = percent;
        print("Friction: " + percent);
    }
}
