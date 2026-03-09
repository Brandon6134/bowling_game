using MagicPigGames;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BowlingBallControl : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioSource audioSourceFire;
    public AudioClip[] pinHit; //is audioclip for oneshot
    private AudioSource ballRolling; //is audiosource to play continously
    public AudioClip obstacleHit;
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
    public float hookStrength = 20f;
    
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
        audioSourceFire = GetComponents<AudioSource>()[2]; //use this audiosource to play flames sfx during bar charge up
    }
        

    void FixedUpdate()
    {
        ApplySpinForce(hookStrength);

        //if is ball in hand, scale fire vfx and sfx for ball according to progress bar
        if (verticalProgressBar != null)
            ControlFire();
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
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            audioSource.PlayOneShot(obstacleHit);
        }
        //else if the object has an audio source component, play their audio source when hit
        else if (collision.gameObject.GetComponent<AudioSource>())
        {
            //if sfx isnt playing already
            if (!collision.gameObject.GetComponents<AudioSource>()[0].isPlaying)
            {
                //play unique hit sfx
                collision.gameObject.GetComponents<AudioSource>()[0].Play();

                //play ball hit sfx 
                collision.gameObject.GetComponents<AudioSource>()[1].Play();
            }
            collision.gameObject.GetComponent<Animator>().SetBool("isHit",true);

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

    public void ControlFire()
    {
        float barProgress = Mathf.Abs(verticalProgressBarScript.Progress-1f);
        float scaleProgress = Mathf.Abs(verticalProgressBarScript.Progress-1f)/4;

        //set fire vfx scale size
        fireVFX.transform.localScale = StaticData.fireScale = new Vector3(scaleProgress,scaleProgress,scaleProgress);

        //set fire sfx volume to increase/decrease with progress
        audioSourceFire.volume = barProgress/2;

        //only continuosly play fire sfx if wasn't playing previously
        //fire sfx stops playing after reset because the ball is deleted (and thus sfx stops)
        if (!audioSourceFire.isPlaying)
        {
            audioSourceFire.Play();
        }
            
        
        
    }

    // applies the spin force onto the ball
    //hookStrength is how strong the hook effect is -> higher value means greater spin
    public void ApplySpinForce(float hookStrength)
    {
        // float hookStrength = -0.8f;
        // float spinY = ballRb.angularVelocity.y;
        // ballRb.AddForce(spinY*Vector3.forward*hookStrength,ForceMode.Force);
        
        Vector3 laneNormal = Vector3.up;

        // Get spin around the up axis
        float sideSpin = Vector3.Dot(ballRb.angularVelocity, laneNormal);

        // Sideways direction relative to ball motion
        Vector3 sideDir = Vector3.Cross(laneNormal, ballRb.linearVelocity.normalized);

        // Apply sideways force based on spin
        Vector3 hookForce = sideDir * sideSpin * hookStrength;

        // Clamp to avoid explosions
        hookForce = Vector3.ClampMagnitude(hookForce, 50f);
        hookForce.x = 0f;
        ballRb.AddForce(hookForce, ForceMode.Force);

        print(hookForce);

        //print("linear velocity: " + ballRb.linearVelocity + "   angular velocity: " + ballRb.angularVelocity);
    }

    // void OnCollisionStay(Collision collision)
    // {
    //     if (!collision.gameObject.CompareTag("Ground")) return;

    //     ContactPoint contact = collision.contacts[0];
    //     decalP.size = contact.point - new Vector3(-20f,0.5f,0.35f);
    // }
}
