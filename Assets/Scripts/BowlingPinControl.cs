using UnityEngine;

public class BowlingPinControl : MonoBehaviour
{
    private Rigidbody pinRb;
    public float highAngularDrang = 1f;
    public float lowAngularDrag = 0.05f;
    public float breakAngle = 8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pinRb = GetComponent<Rigidbody>();
        //pinRb.centerOfMass = new Vector3(0,-0.5f,0);
    }

    // Update is called once per frame
    void Update()
    {
        Sleep();

        float tilt = Vector3.Angle(transform.up, Vector3.up);

        if (tilt < breakAngle)
        {
            pinRb.angularDamping = highAngularDrang;
        }
        else
        {
            pinRb.angularDamping = lowAngularDrag;
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
}
