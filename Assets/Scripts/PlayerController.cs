using System;
using System.Collections;
using MagicPigGames;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float horizontalInput;
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
    [SerializeField] private GameObject bowlingBallInHand;

    void Start()
    {
        cameraControlScript = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        UIManagerScript = GameObject.Find("UI Manager").GetComponent<UIManager>();
        verticalProgressBarScript = verticalProgressBar.GetComponent<VerticalProgressBar>();

        Collider playerColl = GetComponent<Collider>();
        Collider ballColl = bowlingBallInHand.GetComponent<Collider>();
        Physics.IgnoreCollision(playerColl,ballColl);

        if (StaticData.characterSelectedName != null)
            ChangeCharacterModel();

        //grab all animators from children objects
        Animator[] animators = gameObject.GetComponentsInChildren<Animator>();

        //set our animator to the first children object (animates actual player model, not parent object)
        playerAnim = animators[0];

        playerRb = GetComponent<Rigidbody>();

        //make bowling ball spawn at an offset so that camera transition between player and ball is smooth
        camOffset = cameraControlScript.playerOffset - cameraControlScript.ballOffset;

        ChangeBallColor(bowlingBallInHand);
        
    }

    // Update is called once per frame
    void Update()
    {
        //if game is active, allow player control
        if (spawnManagerScript.isGameActive)
        {
            
            //if camera isn't on the scoreboard, allow player movement (when player exits scoreboard, can immediately move so feels nice and not restrictve)
            //also if isnt in the moveforward sequence or throwing animation
            if (!cameraControlScript.camOnScores && !spacePressed && !throwInProgress)
            {
                horizontalMovement();
                rotationalMovement();
                //EnterPortalSequence();
            }

            //camBackOnPlayer indicates if the camera is approximately behind the player, aka cam is at the end of the transition (not midway transition) and on player
            //used Distance() to approximate equallness cause there was a delay between the two vector3's values making them equal after some time, not always.
            bool camBackOnPlayer = Vector3.Distance(cameraControlScript.transform.position,transform.position + cameraControlScript.playerOffset) < 0.1f;

            // start move forward sequence if no balls exist currently (only can throw one ball at a time), if cam is on the player (not mid transition),
            // and space hasn't been pressed down or released this round yet (or else can keep manipulating velocity bar several times in one round)
            if (Input.GetKeyDown(KeyCode.Space) && !GameObject.FindGameObjectWithTag("Bowling Ball") && camBackOnPlayer && !spacePressed && !spaceReleased)
            {
                spacePressed=true;
                throwInProgress=true;
                playerAnim.SetBool("isWalkForward",true);
                UIManagerScript.verticalProgressBar.SetActive(true);
            }
            //if entered moveforwardsequence, start bowling veloctiy bar UI + minigame
            else if(Input.GetKeyUp(KeyCode.Space) && !GameObject.FindGameObjectWithTag("Bowling Ball") && camBackOnPlayer && spacePressed && !throwAnimActive)
            {
                //script.progress returns 0.1 if 90% of bar is filled, so invert progress value to get 0.9
                barPercent = Mathf.Abs(verticalProgressBarScript.Progress - 1f);

                //make the actual percent modifier range from 0.5 to 1 for speed balance
                usedPercent = 0.5f + barPercent/2;

                //width of one arrow in the x-axis
                float num = 515f;

                //subtract the original x position (middle) from the green indicator position.x, divide by width of an arrow to get a 
                //a percentage from -1 to 1, negative being spin left and positive being spin right. multiply by 10 or wtv for extra spin power
                spinStrength= (UIManagerScript.spinUI[0].transform.position.x - UIManagerScript.spinIndicatorBasePosition.x)/num * spinStrengthModifier;

                spacePressed = false;
                spaceReleased = true;

                //change animation speed based off throw speed
                float animSpeed = usedPercent+0.5f;
                playerAnim.speed= animSpeed;

                //begin throw ball animation
                playerAnim.SetBool("isThrow",true);
                throwAnimActive = true;
            }
            
            //move the player forward if walking forward or during throw animation
            // || (spacePressed && throwAnimActive && playerAnim.GetBool("isThrow"))
            if ((spacePressed && playerAnim.GetBool("isWalkForward")) )
            {
                MoveForwardSequence(2f);
            }

            //if the animation shows a step forward, move the player even more
            if (isStepForwardAnim)
            {
                MoveForwardSequence(4f);
            }

            AutoThrowBall();
        }
        
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
        
        horizontalInput = Input.GetAxis("Horizontal");
        
        //translate w/ respect to world, so can move left and right globally (not accounting for rotation)
        //transform.Translate(Vector3.back * speed * horizontalInput *  Time.deltaTime,Space.World);
        playerRb.MovePosition(transform.position + Vector3.back * speed * horizontalInput *  Time.deltaTime);

        //set animation parameter walkSpeed to current input direction
        playerAnim.SetFloat("walkSpeed",horizontalInput);

        //if player isn't moving horizontally, set idle boolean to true for animation
        if (horizontalInput == 0f)
        {
            playerAnim.SetBool("isIdle",true);
        }
        else 
        {
            playerAnim.SetBool("isIdle",false);
        }
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

        //if player angle reaches +/- 45 degrees, prevent them from rotating any further
        if (Mathf.Approximately(clampedY,rotateYRange))
        {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.MoveRotation(Quaternion.Euler(0,44.5f,0));
        }

        else if (Mathf.Approximately(clampedY,-rotateYRange))
        {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.MoveRotation(Quaternion.Euler(0,-44.5f,0));
        }
    
        //allow player to rotate with Q and E buttons
        if (Input.GetKey(KeyCode.Q))
        {
            playerRb.AddTorque(0,-rotateSpeed,0,ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.E))
        {
            playerRb.AddTorque(0,rotateSpeed,0,ForceMode.Impulse);
        }
    }

    private void MoveForwardSequence(float speed)
    {
        playerRb.MovePosition(playerRb.position + Vector3.right * speed *  Time.deltaTime);
        
        //playerAnim.speed=0.4f;
        //print("moving forward with speed of "+speed);
        UIManagerScript.helpText.enabled=false;
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
        UIManagerScript.ballSpeedText.text = speedRounded + " km/h";
        
        //round and multiply by 50 to get nice "accurate" RPM numbers
        float torqueSpeedRounded = Mathf.Abs(Mathf.Round(torqueForce[1]*75));
        UIManagerScript.torqueSpeedText.text = torqueSpeedRounded + " RPM";
    }

    
    // void OnTriggerEnter(Collider other)
    // {  
    //     //if player reaches the start of alley and the throw animation isn't active, force ball throw with mininum ball speed mulitplier of 0.5
    //     if (other.CompareTag("Alley Starting Point") && !throwAnimActive)
    //     {   
    //         if (!throwAnimActive)
    //         {
    //             //if didnt release spacebar at end, set usedPercent to mininum of 0.5, and other booleans
    //             usedPercent=0.5f;
    //             spacePressed = false;
    //             throwAnimActive = true;
    //             //playerRb.constraints = RigidbodyConstraints.FreezePositionY;
    //         }
            
    //         Debug.Log("reached alley start point, start throw animation now!");
    //         playerAnim.SetBool("isThrow",true);
    //     }
    // }

    void AutoThrowBall()
    {
        if (!throwAnimActive && Math.Abs(verticalProgressBarScript.Progress-1f)<=0.01f && UIManagerScript.barSpeed<0)
        {   
            //if didnt release spacebar at end, set usedPercent to mininum of 0.5, and other booleans
            usedPercent=0.5f;
            spacePressed = false;
            throwAnimActive = true;
            
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
    }

    //once throw animation is complete, trigger this func and set throwAnimActive to false
    public void EndOfThrowAnim()
    {
        throwAnimActive=false;
        isStepForwardAnim=false;
        playerAnim.SetBool("isWalkForward",false);

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

    public void EnterPortalSequence()
    {
        playerAnim.SetBool("isWalkForward",true);
        MoveForwardSequence(2f);
    }
}
