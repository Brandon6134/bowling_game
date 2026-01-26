using MagicPigGames;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BowlingBallControl : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip[] pinHit; //is audioclip for oneshot
    private AudioSource ballRolling; //is audiosource to play continously
    private Rigidbody ballRb;
    public bool isBallFrozen=false;
    public bool isBallPastPins=false;
    private PlayerController playerControllerScript;
    private CameraControl cameraControlScript;
    [SerializeField] public GameObject verticalProgressBar;
    private VerticalProgressBar verticalProgressBarScript;
    public GameObject fireVFX;
    //private DecalProjector decalP;
    public Material material;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        cameraControlScript = GameObject.Find("Main Camera").GetComponent<CameraControl>();

        //if is prefab that cant assign verticalprogressbar, ignore
        if (verticalProgressBar != null)
            verticalProgressBarScript = verticalProgressBar.GetComponent<VerticalProgressBar>();

        ballRb = GetComponent<Rigidbody>();
        //require 2 audio sources for handling each audiosource/clip
        audioSource = GetComponents<AudioSource>()[0]; //use this audiosource to play the oneshot AudioClip of pinHit
        ballRolling = GetComponents<AudioSource>()[1]; //use this audiosource to continously play the ballRolling audio
        
        // if (verticalProgressBar == null)
        // {   
        //     decalP = gameObject.AddComponent<DecalProjector>();
        //     decalP.transform.position = new Vector3(-20f,0.5f,0.35f);
        //     decalP.transform.rotation = new Quaternion (90f,0f,0f,0f);
        //     decalP.material = material;
        // }
    }
        

    void FixedUpdate()
    {
        float hookStrength = -0.8f;
        float spinY = ballRb.angularVelocity.y;
        ballRb.AddForce(spinY*Vector3.forward*hookStrength,ForceMode.Impulse);

        //if is ball in hand, rise and lower scale of fire vfx
        if (verticalProgressBar != null)
            ControlFireVFX();
        //else is the thrown ball, set the fire scale to be same as ball in h and
        else
            fireVFX.transform.localScale = StaticData.fireScale;
    }

    void OnCollisionEnter(Collision collision)
    {
        //if bowling ball is touching the ground, sfx isnt already playing, and isn't past the pin area, play the rolling sfx
        if (collision.gameObject.CompareTag("Ground") && !ballRolling.isPlaying && !isBallPastPins)
        {
            ballRolling.Play();
        }
        //if bowling ball hits a pin and isn't past the pin area, play oneshot of pinHit and shake camera for impact
        else if (collision.gameObject.CompareTag("Bowling Pin") && !isBallPastPins)
        {
            int index = Random.Range(0,5);
            audioSource.PlayOneShot(pinHit[index]);
            StartCoroutine(cameraControlScript.ShakeCamera());
            //StartCoroutine(cameraControlScript.ShakeCamera());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        ballRolling.Stop(); //if ball isnt touching ground, stop playing sfx
    }

    void OnTriggerEnter(Collider other)
    {   
        //if collides with the freeze layer, freeze the pins movements so they don't move endlessely
        if (other.CompareTag("Freeze Layer"))
        {
            ballRb.constraints = RigidbodyConstraints.FreezeAll;
            isBallFrozen = true;
        }
        else if (other.CompareTag("Past Pins Point"))
        {
            isBallPastPins=true;

            //if past pins, set angular damping on ball to high so it will stop moving faster, allowing resets to happen faster
            ballRb.angularDamping = 10;
        }
    }

    public void ControlFireVFX()
    {
        float progress = Mathf.Abs(verticalProgressBarScript.Progress-1f)/2;
        fireVFX.transform.localScale = StaticData.fireScale = new Vector3(progress,progress,progress);
    }

    // void OnCollisionStay(Collision collision)
    // {
    //     if (!collision.gameObject.CompareTag("Ground")) return;

    //     ContactPoint contact = collision.contacts[0];
    //     decalP.size = contact.point - new Vector3(-20f,0.5f,0.35f);
    // }
}
