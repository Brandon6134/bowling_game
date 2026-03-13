using System;
using System.Collections;
using MagicPigGames;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float horizontalInput;
    private float horizontalAxis;
    public float speed;
    public float zRange;
    public float rotateYRange = 0.5f;
    public float rotateSpeed;
    public GameObject bowlingBall;
    public float bowlingBallSpeed;
    private CameraControl cameraControlScript;
    private Vector3 camOffset;
    public Animator playerAnim;
    private SpawnManager spawnManagerScript;
    [SerializeField] private GameObject verticalProgressBar;
    private VerticalProgressBar verticalProgressBarScript;
    private UIManager UIManagerScript;
    public bool spacePressed = false;
    public bool spaceReleased = false;
    private Rigidbody playerRb;
    public float speedRounded = 100f;
    public bool throwAnimActive = false;
    public bool isStepForwardAnim = false;
    public float barPercent = 0f;
    public float usedPercent = 0f;
    public bool throwInProgress = false;
    private float spinStrength = 0f;
    public float spinStrengthModifier;
    private GameObject bowlingBallInHand;
    
    public GameObject BiomeManager;
    private BiomeManager biomeManagerScript;
    private AudioSource fireAudioSource;
    public AudioClip fireballSFX;
    private AudioSource footstepAudioSource;
    private Vector3 lastPos;
    private int newBiomeIndex = 0;
    [SerializeField] private GameObject dashedLine;
    public AudioClip tickSFX;
    private AudioSource tickAudioSource;
    private float tickTimer;
    private TipsManager tipsManagerScript;
    private bool isMoveMode = true; //if false, is rotate mode
    private bool isRampingInput = false;
    private MenuActions menuActionsScript;
    public bool isGameActive = false; //used to determine if start() and update() logic is ran
    private AudioSource barReleaseAudioSource;
    public AudioClip barReleaseSFX;
    private bool playerCanMove = false;
    private bool isWalkForward = false;
    private bool isSpaceConditionalDown = false;
    private bool isSpaceConditionalUp = false;

    void Start()
    {
        if (!isGameActive)
        {
            UIManagerScript = GameObject.Find("UI Script Object").GetComponent<UIManager>();
            menuActionsScript = GameObject.Find("Canvas").GetComponent<MenuActions>();
            return;
        }

        cameraControlScript = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        UIManagerScript = GameObject.Find("UI Manager").GetComponent<UIManager>();
        verticalProgressBarScript = verticalProgressBar.GetComponent<VerticalProgressBar>();
        biomeManagerScript = BiomeManager.GetComponent<BiomeManager>();
        tipsManagerScript = GameObject.Find("Tips").GetComponent<TipsManager>();
        menuActionsScript = GameObject.Find("Canvas").GetComponent<MenuActions>();
        

        if (StaticData.characterSelectedName != null)
            ChangeCharacterModel();
        
        //find ball on current character model
        bowlingBallInHand = FindDeepChild(gameObject.transform,"Bowling Ball");

        Collider playerColl = GetComponent<Collider>();
        Collider ballColl = bowlingBallInHand.GetComponent<Collider>();
        Physics.IgnoreCollision(playerColl,ballColl);

        //grab all animators from children objects
        Animator[] animators = gameObject.GetComponentsInChildren<Animator>();

        //set our animator to the first children object (animates actual player model, not parent object)
        playerAnim = animators[0];

        playerRb = GetComponent<Rigidbody>();
        fireAudioSource = GetComponents<AudioSource>()[0];
        footstepAudioSource = GetComponents<AudioSource>()[1];
        tickAudioSource = GetComponents<AudioSource>()[2];
        barReleaseAudioSource = GetComponents<AudioSource>()[3];

        //make bowling ball spawn at an offset so that camera transition between player and ball is smooth
        camOffset = cameraControlScript.playerOffset - cameraControlScript.ballOffset;

        ChangeBallColor(bowlingBallInHand);

        lastPos = playerRb.transform.position;
    }

    //handle all player input, UI, and state changes in update
    void Update()
    {
        HandleMenu();
        if (!isGameActive) //if is menu, quit func
            return;
            
        //if game is active, allow player control
        if (spawnManagerScript.isGameActive)
        {
            //if camera isn't on the scoreboard, allow player movement (when player exits scoreboard, can immediately move so feels nice and not restrictve)
            //also if isnt in the moveforward sequence or throwing animation, and isnt enter portal sequence, then allow player to move / rotate
            if (!cameraControlScript.camOnScores && !spacePressed && !throwInProgress && !biomeManagerScript.isEnterPortalSequence)
                playerCanMove = true;
            else
                playerCanMove = false;
                
            if (playerCanMove)
            {
                horizontalAxis = Input.GetAxis("Horizontal");

                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                    SwitchControlMode();

                //if switched modes and is no longer holding down left or right, set player movement back to 0
                if (isRampingInput && Input.GetAxisRaw("Horizontal")==0f)
                    horizontalInput = 0f;
                
                //if switched modes and player still holding down left/right, overtime ramp up their rotation/movement from 0 to 1/-1 or wtv value
                else if(isRampingInput) 
                    horizontalInput = Mathf.MoveTowards(horizontalInput,horizontalAxis,Time.deltaTime*4f);

                //default of just controlling movement/rotation with input
                else
                    horizontalInput = horizontalAxis;
                
                //if ramped up input equals the internal horizontal input, stop ramping input
                if (Mathf.Abs(horizontalAxis - horizontalInput) < 0.01f)
                    isRampingInput = false;
            }
            
            //camBackOnPlayer indicates if the camera is approximately behind the player, aka cam is at the end of the transition (not midway transition) and on player
            //used Distance() to approximate equallness cause there was a delay between the two vector3's values making them equal after some time, not always.
            bool camBackOnPlayer = Vector3.Distance(cameraControlScript.transform.position,transform.position + cameraControlScript.playerOffset) < 1f;

            // start move forward sequence if no balls exist currently (only can throw one ball at a time), if cam is on the player (not mid transition),
            // and space hasn't been pressed down or released this round yet (or else can keep manipulating velocity bar several times in one round)
            if (Input.GetKeyDown(KeyCode.Space) && !GameObject.FindGameObjectWithTag("Bowling Ball") && camBackOnPlayer && 
            !spacePressed && !spaceReleased && !biomeManagerScript.isEnterPortalSequence)
                isSpaceConditionalDown = true;
            else if(Input.GetKeyUp(KeyCode.Space) && isSpaceConditionalDown)
            {
                isSpaceConditionalUp = true;
                isSpaceConditionalDown = false;
            }

            if (isSpaceConditionalDown)
            {
                spacePressed=true;
                throwInProgress=true;
                //playerAnim.SetBool("isWalkForward",true);
                isWalkForward = true;
                UIManagerScript.verticalProgressBar.SetActive(true); //make velocity bar appear
                UIManagerScript.SetSwitchModeButtonActive(false);

                
            }

            //if entered moveforwardsequence, start bowling veloctiy bar UI + minigame
            else if(isSpaceConditionalUp)
            {
                //script.progress returns 0.1 if 90% of bar is filled, so invert progress value to get 0.9
                barPercent = Mathf.Abs(verticalProgressBarScript.Progress - 1f);

                //make the actual percent modifier range from 0.5 to 1 for speed balance
                usedPercent = 0.5f + barPercent/2;

                //width of one arrow in the x-axis
                float num = UIManagerScript.spinIndicatorPixelDistance;

                //subtract the original x position (middle) from the green indicator position.x, divide by width of an arrow to get a 
                //a percentage from -1 to 1, negative being spin left and positive being spin right. multiply by 10 or wtv for extra spin power
                spinStrength= (UIManagerScript.spinUI[0].transform.position.x - UIManagerScript.spinIndicatorBasePosition.x)/num * spinStrengthModifier;

                spacePressed = false;
                spaceReleased = true;

                //change animation speed based off throw speed
                float animSpeed = usedPercent+0.5f;
                playerAnim.speed= animSpeed;

                //begin throw ball animation
                //playerAnim.SetBool("isThrow",true);
                throwAnimActive = true;
                footstepAudioSource.Stop();

                //make dashed line dissappear
                SetDashedLineActive(false);

                //make all tip objects dissapear
                tipsManagerScript.SetAllTipObjectsActive(false);

                
            }

            AutoThrowBall();
            
        }
        
    }

    //handle all player movement and rotation in fixedUpdate
    void FixedUpdate()
    {
        if (!isGameActive || !spawnManagerScript.isGameActive)
            return;
        
        //allow users to switch between movement and rotation modes
        if (playerCanMove)
        {
            if (isMoveMode)
                horizontalMovement();
            else
                rotationalMovement();
        }
        else if(biomeManagerScript.isEnterPortalSequence)
                StartCoroutine(EnterPortalSequence());

        if (isSpaceConditionalDown)
        {
            playerAnim.SetBool("isWalkForward",true);
        }
            
        else if (isSpaceConditionalUp)
        {
            playerAnim.SetBool("isThrow",true);

            //play snap sfx
            barReleaseAudioSource.pitch = 0.9f + barPercent/5;
            barReleaseAudioSource.PlayOneShot(barReleaseSFX,0.5f + barPercent/2);

            isSpaceConditionalUp = false;
        }

        
         //move the player forward if walking forward or during throw animation
        if (spacePressed && isWalkForward)
            StartCoroutine(CallMoveForwardSequence(5f,true));

        //if the animation shows a step forward, move the player even more
        if (isStepForwardAnim)
            StartCoroutine(CallMoveForwardSequence(10f,false));
    }
    void horizontalMovement()
    {
        //don't allow player to move outside bowling lane
        if (transform.position.z < -zRange)
        {
            transform.position = new Vector3(transform.position.x,transform.position.y,-zRange);
        }

        if (transform.position.z > zRange)
        {
            transform.position = new Vector3(transform.position.x,transform.position.y,zRange);
        }
        
        //translate w/ respect to world, so can move left and right globally (not accounting for rotation)
        //transform.Translate(Vector3.back * speed * horizontalInput *  Time.deltaTime,Space.World);
        Vector3 newPos = transform.position + Vector3.back * speed * horizontalInput *  Time.deltaTime;
        playerRb.MovePosition(newPos);

        CalculatePlayerVelocity(newPos,ref lastPos, true);

        //set animation parameter walkSpeed to current input direction
        playerAnim.SetFloat("walkSpeed",horizontalInput);

        if (horizontalInput==0)
            playerAnim.SetBool("isIdle",true);
        else
            playerAnim.SetBool("isIdle",false);
    }

    void rotationalMovement()
    {

        //get current player rotation in eulerAngles (is a value from 0-360)
        float yAngle = playerRb.rotation.eulerAngles.y;

        //convert yAngle from 0-360 to -180 to 180 (so can account for -45 and +45 degree rotation ranges)
        if (yAngle > 180f)
            yAngle -= 360f;

        //use clamp to return value if within min and max range, otherwise return min or max (if value exceeds them)
        float clampedY = Mathf.Clamp(yAngle,-rotateYRange,rotateYRange);

        float resetAngle = rotateYRange - 0.1f;

        //if player angle reaches +/- maxAngle degrees, prevent them from rotating any further
        if (Mathf.Approximately(clampedY,rotateYRange))
        {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.MoveRotation(Quaternion.Euler(0,resetAngle,0));
        }

        else if (Mathf.Approximately(clampedY,-rotateYRange))
        {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.MoveRotation(Quaternion.Euler(0,-resetAngle,0));
        }

        //allow player to rotate with horizontal input
        playerRb.angularVelocity = new Vector3(0,-rotateSpeed*horizontalInput,0);

        if (Mathf.Abs(horizontalInput) > 0f)
            PlayTickSFX();
        
        CalculatePlayerVelocity(transform.position,ref lastPos, true); //call this so footsteps stop when rotating
    }

    public void CalculatePlayerVelocity(Vector3 newPos, ref Vector3 lastPos, bool playFootstepSFX)
    {
        Vector3 velocity = (newPos - lastPos) / Time.fixedDeltaTime;
        float speed = velocity.magnitude;

        lastPos = newPos;
        
        //old value -> 0.00001f
        float speedThreshold = 0.2f;

        //if player isnt moving, set idle animation boolean
        if (speed <= speedThreshold)
        {
            //print("speed is less than 0.2f at : "+speed);
            if (Mathf.Abs(velocity.x) <= speedThreshold)
            {
                playerAnim.SetBool("isWalkForward",false);
                playerAnim.SetBool("isIdle",true);
                //print("setting walkisofward to false!");
            }   

            if (footstepAudioSource.isPlaying && playFootstepSFX)
            {
                footstepAudioSource.Stop();
                //print("stopping footsteps!");
            }
                
        }
        else 
        {
            if (Mathf.Abs(velocity.x) >= speedThreshold)
            {
                playerAnim.SetBool("isWalkForward",true);
                playerAnim.SetBool("isIdle",false);
            }

            if (!footstepAudioSource.isPlaying && playFootstepSFX)
            {
                footstepAudioSource.Play();
                //print("playing footsteps!");
            }
        }
    }

    private void MoveForwardSequence(float speed, bool playFootstepSFX)
    {
        Vector3 newPos = transform.position + Vector3.right * speed *  Time.deltaTime;
        playerRb.MovePosition(newPos);
        //print(newPos);
    
        CalculatePlayerVelocity(newPos,ref lastPos, playFootstepSFX);
        
        //playerAnim.speed=0.4f;
        //print("moving forward with speed of "+speed);
        UIManagerScript.helpText.enabled=false;
    }

    private IEnumerator CallMoveForwardSequence(float speed, bool playFootstepSFX)
    {
        yield return new WaitForEndOfFrame();
        MoveForwardSequence(speed,playFootstepSFX);
    }

    public void CreateAndMoveBall(float percentModifier, float spinStrength)
    {
        GameObject bowlingClone = Instantiate(bowlingBall,gameObject.transform.position + camOffset,bowlingBall.transform.rotation);
        Rigidbody bowlingRb = bowlingClone.GetComponent<Rigidbody>();
        ChangeBallColor(bowlingClone);

        //add force to ball, with a mulitplier from the percentage of bar filled
        Vector3 force = transform.right * bowlingBallSpeed * percentModifier;
        bowlingRb.AddForce(force,ForceMode.Impulse);
        
        //add torque to ball, with multiplier based on spinStrength
        Vector3 torqueForce = Vector3.up * spinStrength;
        bowlingRb.AddTorque(torqueForce,ForceMode.Impulse);

        //set speed text active
        UIManagerScript.ballSpeedText.enabled = true;
        UIManagerScript.torqueSpeedText.enabled = true;

        //set ball speed text
        speedRounded = Mathf.Round(force[0])/10;
        float speedText = Mathf.Round(barPercent*100f);
        UIManagerScript.ballSpeedText.text = speedText + " SPEED";
        
        //round and multiply to get nice spin numbers (~0-100)
        float torqueSpeedRounded = Mathf.Abs(Mathf.Round(torqueForce[1]*12.5f));
        torqueSpeedRounded = Mathf.Clamp(torqueSpeedRounded,0f,100f); //limit to 0 to 100 values (sometimes gets to 101-104 values)

        string spinDirection =
            torqueSpeedRounded > 0f ? "right" :
            torqueSpeedRounded < 0f ? "left" :
            "";

        UIManagerScript.torqueSpeedText.text = torqueSpeedRounded + " SPIN " + spinDirection;
    }
    
    //handle auto throw ball when progress bar reaches zero
    void AutoThrowBall()
    {
        //if throw animation isnt active, velocity bar reaches zero, and barspeed is negative (bar is coming back downwards)
        if (!throwAnimActive && Math.Abs(verticalProgressBarScript.Progress-1f)<=0.01f && UIManagerScript.barSpeed<0)
        {   
            //if didnt release spacebar at end, set usedPercent to mininum of 0.5, and other booleans
            usedPercent=0.5f;
            spacePressed = false;
            throwAnimActive = true;
            SetDashedLineActive(false);
            
            Debug.Log("velocity bar reached 0, auto-throwing ball!");
            playerAnim.SetBool("isThrow",true);
        }
    }

    public void SetHelpText()
    {
        if (speedRounded < 30f)
        {
            UIManagerScript.helpText.enabled = true;
        }
    }

    public void StepForwardAnim()
    {
        isStepForwardAnim = true;

        //scale fireball sfx with progress bar (multiplied by some amplifier and add min sound value)
        fireAudioSource.PlayOneShot(fireballSFX,Math.Abs(verticalProgressBarScript.Progress-1f)*1.3f+0.2f);
    }

    //once throw animation is complete, trigger this func and set throwAnimActive to false
    public void EndOfThrowAnim()
    {
        throwAnimActive=false;
        isStepForwardAnim=false;
        playerAnim.SetBool("isWalkForward",false);
        isWalkForward = false;

        CreateAndMoveBall(usedPercent,spinStrength);

        playerAnim.speed=1f;
    }

    public void ResetPlayerControllVars()
    {
        //enable help text if needed
        SetHelpText();
        
        playerAnim.SetBool("isThrow",false);

        //rest bool so velocity bar can move and controlled next round
        spaceReleased = false;

        //allow player horizontal movement again
        throwInProgress = false;

        //set this false as backup at end of round, sometimes bug occurs where it's not set false
        throwAnimActive=false;

        //make dashed line visible again
        SetDashedLineActive(true);

        isSpaceConditionalUp = false;
    }

    public void ChangeBallColor(GameObject ball)
    {
        MeshRenderer ballMeshRenderer = ball.GetComponent<MeshRenderer>();
        var ballMats = ballMeshRenderer.sharedMaterials;
        ballMats[0] = StaticData.staticBallColorMat;
        ballMeshRenderer.sharedMaterials = ballMats;
    }

    public void ChangeCharacterModel()
    {
        Transform characterObject = transform.Find(StaticData.characterSelectedName);
        Avatar characterAvatar = characterObject.GetComponent<Animator>().avatar;
        GetComponent<Animator>().avatar = characterAvatar;

        characterObject.gameObject.SetActive(true);
    }

    public IEnumerator EnterPortalSequence()
    {
        UIManagerScript.SetPlayerUIActive(false);
        
        //wait 3 seconds for portal, then start walking player forward
        yield return new WaitForSeconds(3f);

        //if portal sequence is over (aka hit exit portal line trigger) or player has exited the EnterPortal trigger, exit this func so player doesnt moveforward
        if (!biomeManagerScript.isEnterPortalSequence || biomeManagerScript.hasExitedPortalTrigger)
        {
            yield break;
        }
        //moves players forward, doesnt play footstep sfx if has entered portal and vice versa
        MoveForwardSequence(9f,!biomeManagerScript.hasEnteredPortal);
        //playerRb.freezeRotation = true;
    }

    void OnTriggerEnter(Collider other)
    {  
        //if enters portal during portal sequence
        if (other.CompareTag("Entered Portal") && biomeManagerScript.isEnterPortalSequence)
        {   
            newBiomeIndex+=1;
            StartCoroutine(biomeManagerScript.ChangeBiome(newBiomeIndex));
        }
        //if player has exited the portal, then allow disabling of portals and portal booleans
        else if (other.CompareTag("Exited Portal") && biomeManagerScript.isEnterPortalSequence && biomeManagerScript.exitedPortal)
        {
            biomeManagerScript.PortalDisable();
        }
    }
    void OnTriggerExit(Collider other)
    {
        //if exits portal line collider
        if (other.CompareTag("Entered Portal") && biomeManagerScript.isEnterPortalSequence)
        {   
            biomeManagerScript.hasExitedPortalTrigger = true;
        }
    }

    GameObject FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            // Skip inactive objects
            if (!child.gameObject.activeInHierarchy)
                continue;
            
            if (child.name == name)
                return child.gameObject;

            GameObject result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    public void SetDashedLineActive(bool boolean)
    {
        dashedLine.SetActive(boolean);
    }

    private void PlayTickSFX()
    {
        tickTimer+=Time.deltaTime;

        while(tickTimer >= 0.2f)
        {
            tickAudioSource.PlayOneShot(tickSFX);
            tickTimer = 0f;
        }
    }

    public void SwitchControlMode()
    {
        isMoveMode = !isMoveMode; //change control mode
        horizontalInput = 0f; //reset input value
        isRampingInput = true; //start input ramping process

        UIManagerScript.SwitchModeButtonText(isMoveMode); // switch mode text
        menuActionsScript.PlaySwitchModeSound(); //play switch mode sfx

        //reset horizontal input so no input carryover between moving and rotating 
    }

    public void ResetSwitchModeToMove()
    {
        isMoveMode = true;
        UIManagerScript.SwitchModeButtonText(isMoveMode); // switch mode text
    }

    private void HandleMenu()
    {
        if (!isGameActive) //if is menu or non-player but want to use functions in this script
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                {
                    SwitchControlMode();
                }
        }
    }

    
}
