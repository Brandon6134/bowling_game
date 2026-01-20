using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

public class SpawnManager : MonoBehaviour
{
    private CameraControl cameraControlScript;
    private UIManager UIManagerScript;
    private PlayerController playerControllerScript;
    public GameObject bowlingPinPrefab;
    public GameObject pauseMenu;
    public Dictionary<GameObject,Vector3> allPins;
    public Dictionary<GameObject,Vector3> currentPins;
    public Dictionary<GameObject,Vector3> roundStartPins;
    private GameObject player;
    private Vector3 playerPosition;

    private List<Tuple<int,int>> internalRoundTotalPinsDown = new List<Tuple<int, int>>();
    private List<Tuple<string,string>> displayRoundTotalPinsDown = new List<Tuple<string, string>>();
    private int[] internalLastFramePinsDown = new int [3];
    private int[] frameScores = new int[11];
    private int lastFrameRoundIndex = 0;
    private bool isThreeRoundLastFrame = false;
    private int numStrikes = 0;
    private bool isSpare = false;
    private bool isSpareForAnnounce = false;
    private bool isStrike = false;
    private int gameTotalPinsDown;
    private int roundOnePins = 0;
    private int roundTwoPins = 0;
    private int totalRoundPins = 0;
    public GameObject bowlingDisplay;

    public TextMeshPro[] roundScoreText;
    public TextMeshPro[] frameScoreText;
    private int frameIndex;
    //start off game being round 1
    private int globalRound = 1;
    public bool isGameActive = false;
    private bool pinsHaveMovedThisRound = false;
    private BowlingBallControl bowlingBallControlScript;
    public bool transitionCam_BallToPlayer=false;
    public bool callAnnounceScores;
    private bool resetInProgress = false;
    private bool allPinsSleepingLastFrame = false;
    public GameObject GameOverParent;
    public Rigidbody playerRb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGameActive = true;
        cameraControlScript = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        UIManagerScript = GameObject.Find("UI Manager").GetComponent<UIManager>();
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();

        Physics.gravity = new Vector3(0,-25f,0);

        //save initial player position for reset
        player = GameObject.Find("Player");
        playerPosition = player.transform.position;
        print("Start of game player position: " + playerPosition);

        playerRb = player.GetComponent<Rigidbody>();

        //save all initial bowling pin positions for reset
        allPins = TrackPins();
        currentPins = TrackPins();
        roundStartPins = TrackPins();

        frameIndex = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isGameActive)
        {
            AutomaticReset(globalRound);
        }
        
    }

    //hard resets all bowling pins, ball, and player position + rotation
    IEnumerator HardReset()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        currentPins = TrackPins();
        var keys = currentPins.Keys.ToList(); // make a list of keys for indexing
        int currentPinsDown=0;
        string text = "";

        //delete all currentPins and add score if they are moved
        for (int i = 0; i < keys.Count; i++)
        {
            GameObject pin = keys[i];
            Vector3 startPos = roundStartPins[pin];
            Vector3 currentPos = pin.transform.position;

            //if pins aren't in their original positions
            if (Vector3.Distance(currentPos, startPos) > 0.1f)
            {
                currentPinsDown++;
                gameTotalPinsDown++;
            }
            currentPins.Remove(pin); // remove the entry from the dictionary
            Destroy(pin);         // destroy the GameObject
        }

        //spawn in the original 10 allPins
        foreach (Vector3 pos in allPins.Values)
        {
            Instantiate(bowlingPinPrefab,pos,bowlingPinPrefab.transform.rotation);
        }
        
        //Destroy(pin) happens at the end of the frame, so wait for end of frame then TrackPins so is accurate and doesn't track to be deleted pins.
        yield return new WaitForEndOfFrame();
        roundStartPins = TrackPins();
        
        GameObject bowlingBall = GameObject.FindGameObjectWithTag("Bowling Ball");
        Destroy(bowlingBall);

        //if isn't last frame and 3rd round
        if (lastFrameRoundIndex!=2)
        {
            ScoreCalculator(2,currentPinsDown);
        }
        else
        {
            text = ThreeRoundLastFrame_ScoreCalculator(currentPinsDown);
        }
        UpdateScore(text);
        UIManagerScript.AnnounceScore(currentPinsDown,isSpareForAnnounce,isStrike);

        //set back the round to one so soft reset is called next round
        globalRound=1;

        //after the 10th frame, game is finished
        if (frameIndex == 10)
        {
            GameOver();
        }

        resetInProgress = false;
        pinsHaveMovedThisRound=false;

        //reset player controller script variables
        playerControllerScript.ResetPlayerControllVars();

        //reset UI velocity bar variables
        UIManagerScript.ResetUI();

        //reset player object and rigidbody rotations (reset both to make sure they're both synced and reset)
        player.transform.position = playerPosition;
        playerRb.position = playerPosition;
        playerRb.rotation = Quaternion.identity;
        player.transform.rotation = new Quaternion(0,0,0,0);

        //Physics.SyncTransforms();

        //print("post-reset player pos: " + player.transform.position);
        //print("rigid body location: "+playerRb.position);
        //print("startPosition: " + playerPosition);
    }    


    //soft reset: bowling pins that were knocked down are deleted, remaining pins remain in regular spots
    IEnumerator SoftReset()
    {
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        currentPins = TrackPins();
        var keys = currentPins.Keys.ToList(); // make a list of keys for indexing
        int currentPinsDown=0;
        string text="";

        for (int i = 0; i < keys.Count; i++)
        {
            GameObject pin = keys[i];
            Vector3 startPos = roundStartPins[pin];
            Vector3 currentPos = pin.transform.position;

            //if pins aren't in their original positions
            if (Vector3.Distance(currentPos, startPos) > 0.1f)
            {
                currentPins.Remove(pin); // remove the entry from the dictionary
                currentPinsDown++;
                gameTotalPinsDown++;
                Destroy(pin);         // destroy the GameObject
            }
        }

        //if strike, spawn in all pins again
        if (currentPinsDown==10)
        {
            foreach (Vector3 pos in allPins.Values)
            {
                Instantiate(bowlingPinPrefab,pos,bowlingPinPrefab.transform.rotation);
            }
        }
        else
        {
            //set it to round 2 if not a strike, so hard reset is called next. if is a strike, soft reset called again next.
            globalRound=2;
        }

        yield return new WaitForEndOfFrame();
        roundStartPins = TrackPins();

        Destroy(GameObject.FindGameObjectWithTag("Bowling Ball"));

        //if on last frame, 1st round was a strike, and now on 2nd round, call calcs as if round2
        if (frameIndex==9 && internalLastFramePinsDown[0]==10 && lastFrameRoundIndex==1)
        {
            ScoreCalculator(2,currentPinsDown);
        }
        //if isn't last frame and round 3
        else if (lastFrameRoundIndex!=2)
        {
            ScoreCalculator(1,currentPinsDown);
        }
        else
        {
            text = ThreeRoundLastFrame_ScoreCalculator(currentPinsDown);
        }

        UpdateScore(text);
        (isSpareForAnnounce, isStrike) = UIManagerScript.AnnounceScore(currentPinsDown,isSpareForAnnounce,isStrike);
        callAnnounceScores=true;
        resetInProgress = false;
        pinsHaveMovedThisRound=false;

        //reset UI velocity bar stuff
        UIManagerScript.ResetUI();

        //reset player controller script variables
        playerControllerScript.ResetPlayerControllVars();

        //reset player object and rigidbody rotations (reset both to make sure they're both synced and reset)
        player.transform.position = playerPosition;
        playerRb.position = playerPosition;
        playerRb.rotation = Quaternion.identity;
        player.transform.rotation = new Quaternion(0,0,0,0);

        //Physics.SyncTransforms();

        //print("post-reset player pos: " + player.transform.position);
        //print("rigid body location: "+playerRb.position);
        //print("startPosition: " + playerPosition);
    }

    //tracks all existing pin objects, then returns a dict of their gameObjects as keys and initial positions as values
    private Dictionary<GameObject,Vector3> TrackPins()
    {
        Dictionary<GameObject,Vector3> pinDict = new Dictionary<GameObject, Vector3>();

        GameObject[] pins = GameObject.FindGameObjectsWithTag("Bowling Pin");

        for (int i=0;i<pins.Length;i++)
        {
            pinDict[pins[i]] = pins[i].transform.position;
        }

        return pinDict;
    }

    //update the in game visual score display
    public void UpdateScore(string text="")
    {   
        //update all round scores
        for (int i=0;i<displayRoundTotalPinsDown.Count();i++)
        {
            roundScoreText[i].text = displayRoundTotalPinsDown[i].Item1 + " | " + displayRoundTotalPinsDown[i].Item2;
        }

        //update all frame scores
        for (int i=1;i<frameScores.Count();i++)
        {
            frameScoreText[i-1].text = frameScores[i].ToString();
        }

        if (isThreeRoundLastFrame && frameIndex==10 && lastFrameRoundIndex==2)
        {
            roundScoreText.Last().text = displayRoundTotalPinsDown.Last().Item1 + " | " + displayRoundTotalPinsDown.Last().Item2 + " | " + text;
            SetFrameVisible(-1);
        }
    }

    /*
    int round: the 1st or 2nd round in a frame
    int pinsHit: the number of pins hit in the current round
    */
    private void ScoreCalculator(int round, int pinsHit)
    {   
        //Debug.Log("FrameIndex at start of calc: " + frameIndex);
        //diff functions and calcs if is round 1 or 2 in a frame
        if (round==1)
        {
            internalRoundTotalPinsDown.Add(Tuple.Create(pinsHit,0));
            roundOnePins = pinsHit;

            //if last frame, save into the special size 3 array score var and increase its index
            if (frameIndex==9)
            {
                internalLastFramePinsDown[lastFrameRoundIndex] = pinsHit;
                lastFrameRoundIndex++;
            }

            //handle spare case
            if (isSpare)
            {
                frameScores[frameIndex] = 10 + pinsHit + frameScores[frameIndex-1];
                isSpare = false;
                isSpareForAnnounce = false;

                //make previous frame score visible
                SetFrameVisible(-1);
            }

            //calculate a previous frame's score on round one if at 2 or 3 strikes
            if (numStrikes>=2)
            {
                StrikeCalculator(round);
            }

            //handle display scores
            bool isRegularRound = DisplayScoreConverter(1,pinsHit,displayRoundTotalPinsDown);
            
            //if a strike and isn't last frame, go to next frame
            if (!isRegularRound && frameIndex!=9 && pinsHit==10)
            {
                frameIndex++;
            }
            //if is a strike on the last frame, set bool to true
            else if (!isRegularRound && pinsHit==10)
            {
                isThreeRoundLastFrame = true;
            }
        }
        else if (round==2)
        {
            //save 1st round score in the frame before removing the tuple and re-adding it with 2nd round score
            int firstRoundScore = internalRoundTotalPinsDown[frameIndex].Item1;

            internalRoundTotalPinsDown.RemoveAt(frameIndex);
            internalRoundTotalPinsDown.Add(Tuple.Create(firstRoundScore,pinsHit));

            //take each round pins and save into easy to manage variables
            roundOnePins = internalRoundTotalPinsDown[frameIndex].Item1;
            roundTwoPins = internalRoundTotalPinsDown[frameIndex].Item2;
            totalRoundPins = roundOnePins + roundTwoPins;

            if (frameIndex==9)
            {
                internalLastFramePinsDown[lastFrameRoundIndex] = pinsHit;
                lastFrameRoundIndex++;
            }

            //if on second round and numStrikes==1 (and not last frame), must mean scored 0-9 pins so calc score
            if (numStrikes==1)
            {
                StrikeCalculator(round);
            }

            //manage displaypins variable

            //save the first round string version before removing it from list (accounts for the '-')
            string displayRoundOnePins = displayRoundTotalPinsDown[frameIndex].Item1;
            displayRoundTotalPinsDown.RemoveAt(frameIndex);

            //handle display scores
            bool isRegularRound = DisplayScoreConverter(2,pinsHit,displayRoundTotalPinsDown,roundOnePins,displayRoundOnePins);

            //add the 2 round scores and the previous frame's score to get current frame score (first frame accounted for by having 11 framescores, not 10)
            frameScores[frameIndex+1] =  totalRoundPins+ frameScores[frameIndex];

            //if is regular round (no spare or strike from this frame) and isn't a three round last frame, make it's frame text object visible.
            if (isRegularRound && !isThreeRoundLastFrame)
            {
                //make current frame score visible
                SetFrameVisible(0);
            }
            //if is spare and last frame, then set bool to true
            else if (!isRegularRound && frameIndex==9)
            {
                isThreeRoundLastFrame = true;
            }

            //if not last frame or isn't three round last frame, go to next frame
            if (frameIndex!=9 || !isThreeRoundLastFrame)
            {
                frameIndex++;
            }
        }
    }

    private void StrikeCalculator(int round)
    {
        //if round=2 and StrikeCalculator() is called, then must be only 1 strike left.
        if (round==2)
        {
            frameScores[frameIndex] = 10 + totalRoundPins + frameScores[frameIndex-1];
            frameScores[frameIndex+1] = totalRoundPins + frameScores[frameIndex];

            //set previous frame visible
            SetFrameVisible(-1);

            //if a spare wasn't just scored and isnt a last frame 3 round, then set current frame visible
            if (totalRoundPins!=10 && !isThreeRoundLastFrame)
            {
                SetFrameVisible(0);
            }
        }
        //otherwise, calculate strike points for previous frame
        else
        {
            frameScores[frameIndex-numStrikes+1] = 20 + roundOnePins + frameScores[frameIndex-numStrikes];
            
            //set that previous frame calcualted visible
            SetFrameVisible(-numStrikes);
        }
        //remove number of strikes
        numStrikes--;
    }

    //display converter function for 0 to '-', spares to '/', strikes to 'X', and other numbers of pins as strings.
    private bool DisplayScoreConverter(int round,int pinsHit,List<Tuple<string,string>> displayList,int roundOnePins = -100,string displayRoundOnePins = "")
    {
        string displayText;

        //if is a regular round (no spare or strikes present), return true
        bool regularRound = false;

        if (pinsHit==0)
        {
            displayText = "-";
            regularRound = true;
        }
        //if spare, and the first round wasnt a 10 (e.g. on last frame, first round was a 10 (strike) and second round was a 0)
        else if (roundOnePins+pinsHit == 10 && roundOnePins!=10)
        {
            displayText = "/";
            isSpare=true;
            isSpareForAnnounce=true;
        }
        else if (pinsHit==10)
        {
            displayText = "X";
            isStrike=true;

            //if a strike on the last frame, don't add to numStrikes so no bonus point calcs are called on.
            if (frameIndex!=9)
            {
                numStrikes++;
            }
        }
        else
        {
            displayText = pinsHit.ToString();
            regularRound = true;
        }

        if (round==1)
        {
            displayList.Add(Tuple.Create(displayText,""));
        }
        else
        {
            displayList.Add(Tuple.Create(displayRoundOnePins,displayText));
        }
        return regularRound;
    }
    
    private string ThreeRoundLastFrame_ScoreCalculator(int pinsHit)
    {
        internalLastFramePinsDown[lastFrameRoundIndex] = pinsHit;
        int round2Pins= internalLastFramePinsDown[lastFrameRoundIndex-1];
        string text;
        
        if (pinsHit==0)
        {
            text = "-";
        }
        else if (pinsHit + round2Pins == 10 && round2Pins!=10)
        {
            text = "/";
            isSpareForAnnounce=true;
        }
        else if (pinsHit==10)
        {
            text = "X";
            isStrike=true;
        }
        else
        {
            text = pinsHit.ToString();
        }

        frameScores[frameIndex+1]+=pinsHit;
        frameIndex++;

        return text;
    }

    private void SetFrameVisible(int indexModifier)
    {
        frameScoreText[frameIndex + indexModifier].gameObject.SetActive(true);
    }

    //automatically check if pins have been hit, and when they stop moving. then calls upon soft or hard reset as neccessary.
    void AutomaticReset(int round)
    {
        
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Bowling Pin");
        GameObject bowlingBall = GameObject.FindGameObjectWithTag("Bowling Ball");
        
        int sleepCount=0;
        bool isRoundScoreEqualZero = false;
        
        foreach (GameObject pin in pins)
        {
            Rigidbody pinRb = pin.GetComponent<Rigidbody>();

            //if a pin has started moving or rotating, start tracking if they're asleep so reset can be called if they're all done moving (asleep)
            //increased min velocity to 0.5f, so that when round resets and pins drop to ground their speed is below 0.5f, and isnt detected as moving.
            if (pinRb.linearVelocity.magnitude>0.5f || pinRb.angularVelocity.magnitude>0.5f)
            {
                pinsHaveMovedThisRound = true;
            }
            if (pinRb.IsSleeping() && pinsHaveMovedThisRound)
            {
                sleepCount++;
            }
        }

        bool allPinsSleepingThisFrame = sleepCount == pins.Length;
        bool pinsJustStopped = allPinsSleepingThisFrame && !allPinsSleepingLastFrame;

        allPinsSleepingLastFrame = allPinsSleepingThisFrame;

        // if bowling ball exists (was thrown) and hasn't hit any pins
        if (bowlingBall && !pinsHaveMovedThisRound)
        {
            bowlingBallControlScript = GameObject.FindGameObjectWithTag("Bowling Ball").GetComponent<BowlingBallControl>();
            Rigidbody bowlingBallRb = bowlingBall.GetComponent<Rigidbody>();

            //if ball has nearly stopped moving and is past the pins (has velocity, and not didnt just spawn in w/ zero velocity)
            if ((bowlingBallRb.linearVelocity.magnitude<0.1f || bowlingBallRb.angularVelocity.magnitude<0.1f) && bowlingBallControlScript.isBallPastPins)
            {
                isRoundScoreEqualZero = true;
            }
        }

        //if all pins have stopped moving or zero scored was achieved that round (no pins hit)

        if ((pinsJustStopped || isRoundScoreEqualZero) && !resetInProgress)
        {
            resetInProgress = true;
            if (round==1 && lastFrameRoundIndex!=2)
            {
                //Debug.Log("soft reset called!");
                StartCoroutine(SoftReset());
            }
            else
            {
                //Debug.Log("hard reset called!");
                StartCoroutine(HardReset());
            }

            pinsHaveMovedThisRound=false;
            bowlingBallControlScript.isBallPastPins = false;
            transitionCam_BallToPlayer = true;
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        GameOverParent.SetActive(true);
        pauseMenu.SetActive(false);
    }
}