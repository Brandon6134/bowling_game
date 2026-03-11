using System;
using System.Collections;
using MagicPigGames;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuActions : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject howToPlayPanel2;
    public GameObject howToPlayPanel3;
    public GameObject customizePanel;
    public GameObject characterSelectPanel;
    public GameObject pauseButton;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterBackstoryText;
    //public GameObject velocityBar;
    //public RectTransform velocityBarRectTransform;
    public GameObject verticalProgressBar;
    private VerticalProgressBar verticalProgressBarScript;
    public Outline colorSelectedOutline;
    public Material defaultBallMaterial;
    public ScrollRect[] scrollRectCustomizeRows;
    public Transform charGameObject;
    public Vector3 originalPosition;
    private AudioSource audioSource;
    public AudioClip buttonPressSound;
    public AudioClip customizePressSound;
    public AudioClip confirmPressSound;
    public AudioClip switchModeSound;
    private float[] moddedBarSpeeds;
    private float t = 0f;
    private float minY = 0f;
    private float maxY = 1f;
    private float barSpeed = 0.5f;
    private bool stopMovingVelocityBar = false;
    private float[] barSpeedMultipliers = {1.5f,2f,3f,4.5f};
    public bool needToResetCharPreviewPos = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        howToPlayPanel.SetActive(false);
        Scene currentScene = SceneManager.GetActiveScene();
        verticalProgressBarScript = verticalProgressBar.GetComponent<VerticalProgressBar>();

        //if on main menu UI
        if (currentScene.buildIndex == 0)
        {
            mainMenuPanel.SetActive(true);
            customizePanel.SetActive(false);

            //track what ball color is selected and outline it (or apply default color if none selected)
            TrackBallColor();
            TrackCharacterSelected();
        }

        moddedBarSpeeds = new float[] {barSpeed,barSpeed,barSpeed,barSpeed};

        //set barSpeeds
        for (int i=0;i<moddedBarSpeeds.Length;i++)
        {
            moddedBarSpeeds[i]*=barSpeedMultipliers[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (howToPlayPanel2.activeInHierarchy)
        {
            //(t,minY,maxY) = VelocityBarChange(velocityBarRectTransform,t,minY,maxY);
            (t, minY, maxY,barSpeed,stopMovingVelocityBar) = AssetVelocityBarChange(t, minY, maxY, barSpeed, verticalProgressBarScript,stopMovingVelocityBar);
        }
    }

    public void LoadGameScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
        Time.timeScale = 1;
    }

    public void HowToPlayButton()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }


    //generic next button function, makes current gameobject inactive and the parameter nextPanelToShow active
    public void NextPageButton(GameObject nextPanelToShow)
    {
        GameObject clicked = EventSystem.current.currentSelectedGameObject;
        GameObject parentPage = clicked.transform.parent.gameObject;

        parentPage.SetActive(false);
        nextPanelToShow.SetActive(true);
    }

    public void ReturnButton(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void PauseMenuButton()
    {
        mainMenuPanel.SetActive(true);
        pauseButton.SetActive(false);

        //pause game and its time
        Time.timeScale = 0;
    }

    public void ResumeButton()
    {
        mainMenuPanel.SetActive(false);
        pauseButton.SetActive(true);

        //unpause game and its time
        Time.timeScale = 1;
    }

    public void CharacterSelectButton()
    {
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void CustomizeButton()
    {
        mainMenuPanel.SetActive(false);
        customizePanel.SetActive(true);
    }
    
    public void ToggleOutline(Outline outline)
    {
        //erase last button's outline
        colorSelectedOutline.effectDistance = new Vector2(0,0);

        //create current button's outline
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(10,10);

        //set the newly selected button as the current outline
        colorSelectedOutline = outline;
        StaticData.staticColorSelectedName = outline.transform.parent.name;
        
    }

    public void SelectBallColor(GameObject ball)
    {
        MeshRenderer ballMesh = ball.GetComponent<MeshRenderer>();
        Material ballColorMat = ballMesh.materials[0];
        StaticData.staticBallColorMat = ballColorMat;
    }

    public void TrackBallColor()
    {
            //if no current ball color selected, make default material blue
            if (StaticData.staticBallColorMat == null)
                StaticData.staticBallColorMat = defaultBallMaterial;

            //if no current ball color selected, grab the name of that object's parent name (eg 'blue ball')
            if (StaticData.staticColorSelectedName == null)
                StaticData.staticColorSelectedName = colorSelectedOutline.transform.parent.name;
            
            //find ball colour name in content row 1 or 2
            Transform obj = transform.Find("Change Ball Colour Panel/Scroll Area/Parent Content/Content Row 1/"+StaticData.staticColorSelectedName);
            if (obj==null)
                obj = transform.Find("Change Ball Colour Panel/Scroll Area/Parent Content/Content Row 2/"+StaticData.staticColorSelectedName);
            
            //find button outline
            colorSelectedOutline = obj.Find("Button").GetComponent<Outline>();
            
            //outline the current ball color
            ToggleOutline(colorSelectedOutline);
    }
    
    public void SelectCharacter(GameObject character)
    {
        charGameObject = transform.Find("Character Select Panel/Selected Character Preview/"+StaticData.characterSelectedName);

        //if player selected same character again, exit function
        if (character.name == charGameObject.gameObject.name)
            return;
        
        charGameObject.gameObject.SetActive(false); //set last character object inactive
        if (needToResetCharPreviewPos) // reset their position + rotation if confirm was pressed just previously
        {
            charGameObject.transform.position = originalPosition;
            charGameObject.rotation = Quaternion.identity;
            needToResetCharPreviewPos = false; // done reseting confirmed last character pos, dont reset chars
        }
        
        //set new character name variable and set active
        StaticData.characterSelectedName = characterNameText.text = character.name;
        charGameObject = transform.Find("Character Select Panel/Selected Character Preview/"+StaticData.characterSelectedName);
        charGameObject.gameObject.SetActive(true);
        
    }

    //handle fresh main menu load
    public void TrackCharacterSelected()
    {   
        //set default if no char selected
        if (StaticData.characterSelectedName == null)
            StaticData.characterSelectedName = "James";

        //set character name text and set prewview object active
        characterNameText.text = StaticData.characterSelectedName;
        charGameObject = transform.Find("Character Select Panel/Selected Character Preview/"+StaticData.characterSelectedName);
        charGameObject.gameObject.SetActive(true);
    }

    //when confirm button is clicked, start character preview animation
    public void ConfirmCharacter()
    {
        originalPosition = charGameObject.position;
        needToResetCharPreviewPos = true; //if player selects other characters, need to reset selected character pos + rotation after animation

        //play character animation
        Animator charAnim = charGameObject.gameObject.GetComponent<Animator>();
        charAnim.SetInteger("anim_index",0);

        //play character voiceline
        AudioSource voiceline = charGameObject.gameObject.GetComponent<AudioSource>();
        voiceline.PlayOneShot(voiceline.clip);
    }

    public void PlayClickButtonSound()
    {
        audioSource.PlayOneShot(buttonPressSound);
    }

    public void PlayClickCustomizerOptionSound()
    {
        audioSource.PlayOneShot(customizePressSound);
    }

    public void PlayConfirmButtonSound()
    {
        audioSource.PlayOneShot(confirmPressSound,0.3f);
    }

    public void PlaySwitchModeSound()
    {
        audioSource.PlayOneShot(switchModeSound,0.8f);
    }



    //simplified velocity bar func from UIManager, but it just goes up and down forever with no player input
    // public (float,float,float) VelocityBarChange(RectTransform rect, float tBar, float minY, float maxY)
    // {
    //     //change rectangle height
    //     rect.sizeDelta = new (100, Mathf.Lerp(minY,maxY,tBar));

    //     //increase t overtime, multiply by unscaledDeltaTime so even game is paused (time.timeScale=0) it still scales with time (exclusive for UI)
    //     tBar+=1.1f*Time.unscaledDeltaTime;

    //     //if t=1 then bar has reached max or min size, thus switch the min and max so it starts increasing or decreasing size appropriately
    //     if (tBar>=1)
    //     {
    //         (minY,maxY) = (maxY,minY);
    //         tBar=0f;
    //     }

    //     return (tBar,minY,maxY);
    // }


    public (float,float,float,float, bool) AssetVelocityBarChange(float t, float minY, float maxY, float barSpeed, VerticalProgressBar verticalProgressBarScript, bool stopMovingVelocityBar)
    {
        //set the progress bar value and increase t value
        verticalProgressBarScript.SetProgress(Mathf.Lerp(minY,maxY,t));

        //multiply by unscaledDeltaTime so even game is paused (time.timeScale=0) it still scales with time (exclusive for UI)
        t+=barSpeed*Time.unscaledDeltaTime;

        //if this is true, then bar has went up and down once already, so thus stop moving the velocity bar. reset t and barSpeed values;
        if (t<=0)
        {
            //stopMovingVelocityBar = true;
            barSpeed *= -1f;
            t=0;
        }
        //if t reaches 1 or more, set the bar speed to -1 so it begins decreasing
        else if (t>=1 && barSpeed>0)
        {   
            barSpeed *= -1f;
        }

        barSpeed = ChangeBarSpeed(barSpeed);

        return (t,minY,maxY,barSpeed,stopMovingVelocityBar);
    }

    public float ChangeBarSpeed(float barSpeed)
    {
        //set 5 diff bar speeds for higher bar values, to make harder to get higher speeds
        float[] benchmarks = {0.45f,0.6f,0.8f,0.9f};
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


    
}
