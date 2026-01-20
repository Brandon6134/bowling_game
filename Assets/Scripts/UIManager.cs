using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor;
using MagicPigGames;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreAnnouncementText;
    private SpawnManager spawnManagerScript;
    private PlayerController playerControllerScript;
    [SerializeField] public GameObject verticalProgressBar;
    private VerticalProgressBar verticalProgressBarScript;
    private AudioSource audioSource;
    public AudioClip[] announcePinsHitSFX;
    public TextMeshProUGUI ballSpeedText;
    public TextMeshProUGUI helpText;
    public TextMeshProUGUI torqueSpeedText;
    public GameObject[] spinUI;
    private float t=0;
    public float tBar=0;
    private int minA = 0;
    private int maxA = 1;
    private float tMod = 3f;
    private bool fadeText = false;
    private bool pause = false;
    public float minYFixed = 0f;
    public float maxYFixed = 700f;
    public float minY = 0f;
    public float maxY = 1f;
    public bool stopMovingVelocityBar = false;
    public float barSpeed = 0.5f;
    public float barSpeedFixed = 0.5f;
    float[] moddedBarSpeeds;
    public float[] barSpeedMultipliers = {1.2f,1.5f,2f,2.5f};
    public float barMultipler = 0f;
    private float minSpinX = 100f+960f;
    private float maxSpinX = 930f+960f;
    public float speedOfSpinIndicator;
    public Vector3 spinIndicatorBasePosition;
    public float elapsedTime=0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        verticalProgressBarScript = verticalProgressBar.GetComponent<VerticalProgressBar>();
        audioSource = GetComponent<AudioSource>();

        moddedBarSpeeds = new float[] {barSpeed,barSpeed,barSpeed,barSpeed};

        //set barSpeeds
        for (int i=0;i<moddedBarSpeeds.Length;i++)
        {
            moddedBarSpeeds[i]*=barSpeedMultipliers[i];
        }
        
        ballSpeedText.enabled = false;
        torqueSpeedText.enabled=false;

        helpText.outlineColor = Color.black;
        helpText.outlineWidth = 0.15f;
        helpText.enabled = false;

        //set progress bar initial progress to 0 and make inactive
        verticalProgressBarScript.SetProgress(0f);
        verticalProgressBar.SetActive(false);

        spinIndicatorBasePosition = spinUI[0].transform.position;

        //set all spinUI objects inactive
        foreach (GameObject obj in spinUI)
        {
            obj.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //if space is pressed down, velocity bar should be moving still, and game isnt paused
        if (playerControllerScript.spacePressed && !stopMovingVelocityBar && Time.deltaTime!=0)
        {
            //(tBar, minY, maxY) = VelocityBarChange(velocityBarRectTransform, tBar, minY, maxY);
            (tBar, minY, maxY,barSpeed) = AssetVelocityBarChange(tBar, minY, maxY, barSpeed);
            SpinGaugeChange(spinUI,minSpinX,maxSpinX);
        }
        
        FadeOutAndStop();
    }

    public (float,float,float,float) AssetVelocityBarChange(float t, float minY, float maxY, float barSpeed)
    {
        //set the progress bar value and increase t value
        verticalProgressBarScript.SetProgress(Mathf.Lerp(minY,maxY,t));
        t+=barSpeed*Time.deltaTime; 

        //if this is true, then bar has went up and down once already, so thus stop moving the velocity bar. reset t and barSpeed values;
        if (t<=0)
        {
            stopMovingVelocityBar = true;
            t=0;
        }
        //if t reaches 1 or more, set the bar speed to -1 so it begins decreasing
        else if (t>=1 && barSpeed>0)
        {   
            barSpeed *= -1f;
        }

        barSpeed = ChangeBarSpeed(barSpeed);

        return (t,minY,maxY,barSpeed);
    }

    public float SpinGaugeChange(GameObject[] objarray, float min, float max)
    {
        foreach (GameObject obj in objarray)
        {
            obj.SetActive(true);
        }

        //don't allow player to move outside bowling lane
        if (objarray[0].transform.position.x < min)
        {
            objarray[0].transform.position = new Vector3(min,objarray[0].transform.position.y,objarray[0].transform.position.z);
        }

        if (objarray[0].transform.position.x > max)
        {
            objarray[0].transform.position = new Vector3(max,objarray[0].transform.position.y,objarray[0].transform.position.z);
        }
        
        float horizontalInput = Input.GetAxis("Horizontal");
        
        //translate green indicator left and right based on user horizontal input
        objarray[0].transform.Translate(Vector3.left * speedOfSpinIndicator * horizontalInput *  Time.deltaTime);


        return max;
    }

    //at the end of each round, announce your score e.g. "9 pins" or "Spare" and play the corresponding sfx with it
    public (bool,bool) AnnounceScore(int pinsHit, bool isSpare, bool isStrike)
    {
        string text="";

        if (isStrike)
        {
            text = "Strike!";
            audioSource.PlayOneShot(announcePinsHitSFX[2],1.25f);
            isStrike=false;
        }
        else if (isSpare)
        {
            text = "Spare!";
            audioSource.PlayOneShot(announcePinsHitSFX[1],1.25f);
            isSpare=false;
        }
        else if (pinsHit==0)
        {
            text = "Miss..";
            audioSource.PlayOneShot(announcePinsHitSFX[3],1.25f);
        }
        else
        {
            text = pinsHit.ToString() + " pins";
            audioSource.PlayOneShot(announcePinsHitSFX[0],1.25f);
        }

        scoreAnnouncementText.text = text;
        scoreAnnouncementText.outlineColor = Color.black;
        scoreAnnouncementText.outlineWidth = 0.15f;

        fadeText=true;

        return (isSpare,isStrike);
    }

    //fade's the given text's .a property and returns that and the t value
    public (Color,float) FadeText(TextMeshProUGUI text, float t, int min, int max,float tModifier)
    {
        //Debug.Log("fading text! t: " + t);
        Color c = text.color;
        c.a = Mathf.Lerp(min,max,t);

        //increase the t value over time
        t+=tModifier*Time.deltaTime;

        return (c,t);
    }

    private void FadeOutAndStop()
    {
        
        //if text is opaque (a>=1) and t>=1, then start the fadeout by switching min and max values and resetting t=0
        if (scoreAnnouncementText.color.a >= 1f && t>=1 && !pause )
        {
            //set pause to true, so the fadeout timer func is only called once
            pause = true;
            StartCoroutine(FadeOutScoreDisplayTimer(1f));
        }

        //if text is inivisible (a<=0) and the min and max values have been switched (has faded in and out already), stop the fade calls
        else if (scoreAnnouncementText.color.a <= 0f && minA > maxA && fadeText)
        {
            //Debug.Log("stop fade");
            fadeText = false;
            
            //reset t and set max and min to original values
            t=0;
            (maxA,minA) = (minA,maxA);
            pause = false;
        }

        //if we want to fadeText in or out, call func repeatedly in update() to adjust .a and t values to transition opacity levels
        if (fadeText)
        {
            (scoreAnnouncementText.color,t) = FadeText(scoreAnnouncementText,t,minA,maxA,tMod);
        }
    }

    //a timer that waits seconds, then sets certain variables to new values to begin announce score text to fade out
    IEnumerator FadeOutScoreDisplayTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        //Debug.Log("start fadeOut");
        (maxA,minA) = (minA,maxA);
        t=0;

    }

    public float ChangeBarSpeed(float barSpeed)
    {
        //set 5 diff bar speeds for no flame + 4 types of flames vfx
        //0 -> no flame
        //0.3 -> red flame
        //0.6 -> blue flame
        //0.9 -> green flame
        //0.95 -> purple flame
        float[] benchmarks = {0.3f,0.6f,0.9f,0.95f};
        float progress = Mathf.Abs(verticalProgressBarScript.Progress - 1f);
        float posNegMod = 1f;

        if(barSpeed<0)
        {
            posNegMod = -1f;
        }

        //loop through all benchmarks (start from greatest to least)
        for (int i=benchmarks.Length-1;i>=0;i--)
        {
            
            if (progress>=benchmarks[i])
            {
                barSpeed = moddedBarSpeeds[i] * posNegMod;
                break;
            }
        }
        return barSpeed;
    }

    public void ResetUI()
    {
        //reset velocity bar variables
        tBar = 0;
        ballSpeedText.enabled = false;
        maxY = maxYFixed;
        minY = minYFixed;
        stopMovingVelocityBar = false;
        barSpeed = barSpeedFixed;
        verticalProgressBarScript.SetProgress(0f);
        verticalProgressBar.SetActive(false);

        //reset UI spin gauge variable
        spinUI[0].transform.position = spinIndicatorBasePosition;
        foreach (GameObject obj in spinUI)
        {
            obj.SetActive(false);
        }
        torqueSpeedText.enabled = false;
    }
}
