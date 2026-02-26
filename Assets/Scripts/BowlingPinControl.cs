using System.Runtime.InteropServices;
using UnityEngine;

public class BowlingPinControl : MonoBehaviour
{
    private Rigidbody pinRb;
    private Collider pinColl;
    public float highDamping;
    public float lowDamping;
    public float breakAngle;
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
    void FixedUpdate()
    {
        ConditionalDamping();
        if (playerControllerScript.spaceReleased)
        {
            //Sleep();
            FrictionScaler();
        }
            
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
        if (pinRb.linearVelocity.magnitude < 0.2f && pinRb.angularVelocity.magnitude < 0.2f)
        {
            pinRb.linearVelocity = Vector3.zero;
            pinRb.angularVelocity = Vector3.zero;
        }
    }

    //set high damping if tilt is less than breakAngle (~8 degrees), aka standing upright
    //set low damping if tilt is more than breakAngle, aka has been hit
    //this makes pins harder to initially move, but when they are hit they are sent flying
    void ConditionalDamping()
    {
        float tilt = Vector3.Angle(transform.up, Vector3.up);

        if (tilt < breakAngle && !hasMoved)
        {
            pinRb.angularDamping = highDamping;
            pinRb.linearDamping = highDamping;
        }
        else
        {
            pinRb.angularDamping = lowDamping;
            pinRb.linearDamping = lowDamping;
            hasMoved = true;
            //print("hasMoved: " + hasMoved + gameObject.name);
            
        }
        //print(pinRb.linearDamping);
    }

    void FrictionScaler()
    {
        //assume speedRounded can be 10 - 50 km/h
        //lower ball speed -> higher friction, higher ball speed -> lower friction
        //(speedRounded - 10) / (50 - 10) = range from 0 to 1 based on range 10 - 50
        float max = 50f;
        float min = 10f;
        float percent = (playerControllerScript.speedRounded - min) / (max-min);

        //get inverse of percent so 0.9 bar filled -> 0.1 friction
        float friction = Mathf.Abs(percent-1f);
        pinColl.material.dynamicFriction = friction;
        pinColl.sharedMaterial.staticFriction = friction;
        //print("Friction: " + percent);
    }
}
