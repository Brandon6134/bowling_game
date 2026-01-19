
using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    
    public GameObject player;
    public Vector3 playerOffset = new Vector3(-4,5,0);
    public Vector3 ballOffset = new Vector3(-8,5,0);
    private GameObject bowlingBall;
    private Vector3 lastCamPos;
    public float camBoundary = 16f;
    public bool camOnScores = false;
    public float camSwitchSpeed = 1f;
    public Vector3 camScorePos = new Vector3(-16.8f,6f,-10.8f);
    private float t;
    public bool camFinishedTransition = true;
    private bool camFinishedBallToPlayer = true;
    private bool newFrameInitialized = true;
    private bool onLastCamPos = false;

    //doTransition defines if an cam transition is to be played. false if no, true if yes.
    private bool doTransition = false;
    private SpawnManager spawnManagerScript;
    private PlayerController playerControllerScript;
    public AnimationCurve curve;
    public float duration = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if game is active, allow all the camera controls
        if (spawnManagerScript.isGameActive)
        {
            bowlingBall = GameObject.FindGameObjectWithTag("Bowling Ball");

            //if bowling ball is thrown/exists, is mid cam transition, is charging up velocity bar with space,
            // or the throw animation is active, then don't allow cam switch. otherwise allow switch upon pressing tab.
            //aka if player is just moving around in initial state
            if (Input.GetKeyDown(KeyCode.Tab) && !bowlingBall && camFinishedBallToPlayer && camFinishedTransition
            && !playerControllerScript.spacePressed && !playerControllerScript.throwAnimActive)
            {
                //if false, set true. if true, set false.
                //allows users to switch between the player cam and score cam
                camOnScores = !camOnScores;

                //game no longer just initialized
                newFrameInitialized = false;

                //reset t value to zero so next cam transition starts from t=0
                t=0;

                //start transition
                doTransition=true;
            }

            //if t>1 then cam transition is complete. thus stop doing any transitions.
            if (t>1f)
            {
                doTransition=false;
            }

            if (spawnManagerScript.transitionCam_BallToPlayer)
            {
                camOnScores=false;

                //reset t value if was over 1, so animation can start from t=0
                if (t>=1)
                {
                    t=0;
                }

                //transition cam from lastCam position (ball + pins view) back to player
                camFinishedBallToPlayer = SwitchCamera(lastCamPos,player.transform.position + playerOffset);

                //once camFinshedBallToPlayer is true (done transition), set the transitionCam bool in spawn manager to false
                spawnManagerScript.transitionCam_BallToPlayer = !camFinishedBallToPlayer;

                //once finished transition to from balls+pin to player, set new frame was initalized to true (so scoreboard to player transition doesnt happen immediately)
                if (camFinishedBallToPlayer)
                {
                    newFrameInitialized=true;
                }
            }

            //if camera is not on the scoreboard
            if (!camOnScores)
            {
                //always true unless a new frame was initialized, then dont allow cam to transition immediately.
                if (!newFrameInitialized && camFinishedBallToPlayer && doTransition)
                {
                    //switch cam back from the scoreboard to the current player's pos
                    camFinishedTransition = SwitchCamera(camScorePos,player.transform.position + playerOffset);
                }

                //if camera isn't mid transition (aka is finished transitioning), then allow camera to follow player / bowling ball
                if (camFinishedTransition && camFinishedBallToPlayer)
                {
                    //if the bowling ball reaches the camBoundary, grab a snapshot of that camera position
                    if (bowlingBall && Mathf.Round(bowlingBall.transform.position.x) == camBoundary)
                    {
                        lastCamPos = bowlingBall.transform.position + ballOffset;
                    }
                    
                    //if bowling ball exists and hasn't reached the camBoundary, camera follows bowling ball until the camera boundary
                    if (bowlingBall && bowlingBall.transform.position.x < camBoundary)
                    {
                        transform.position = bowlingBall.transform.position + ballOffset;
                    }
                    //if bowling ball is past the camBoundary, switch to static lastCamPos snapshot for final view
                    else if (bowlingBall)
                    {
                        //if already set to lastCamPos, exit so that it's only set once and allows for camera shaking when hitting pins
                        if (onLastCamPos)
                            return;
                        transform.position = lastCamPos;
                        onLastCamPos=true;
                    }
                    //if bowling ball doesn't exist, camera follows player.
                    else
                    {
                        transform.position = player.transform.position + playerOffset;

                        //reset on lastCamPos bool
                        onLastCamPos = false;
                    }
                }
            }
            //if camera supposed to be on scoreboard
            else if (doTransition)
            {
                camFinishedTransition = SwitchCamera(player.transform.position + playerOffset,camScorePos);
            }
        }
        //if game is done, switch camera from ball to scoreboard
        else
        {   
            //reset t=0 only once, before the final transition starts
            if (!doTransition)
            {
                t=0;
                doTransition=true;
            }
            SwitchCamera(lastCamPos,camScorePos);
        }
        

    }

    //SwitchCamera smoothly transitions the cam position from the starting position to the ending position.
    //Vector3 startPos: the starting cam position
    //Vector3 endPost: the ending cam position
    //returns bool: returns true if cam is done transitioning, false if not done transitioning.
    public bool SwitchCamera(Vector3 startPos, Vector3 endPos)
    {
        transform.position = new Vector3(Mathf.Lerp(startPos.x,endPos.x,t),Mathf.Lerp(startPos.y,endPos.y,t),Mathf.Lerp(startPos.z,endPos.z,t));
        
        //increase the t value over time, so when this function is called again in Update() Mathf.Lerp returns higher pos values, eventually reaching end pos
        t+=2f*Time.deltaTime;

        //if t>=1 then the camera has finished transitioning, otherwise if 0 <= t < 1, then isnt done transitioning.
        if (t>=1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator ShakeCamera()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime+= Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position = lastCamPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = lastCamPos;
    }
}
