using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TipsManager : MonoBehaviour
{
    private List<GameObject> childrenList = new List<GameObject>();
    private List<GameObject> hideShowTextList = new List<GameObject>();
    private List<GameObject> tipsObjectChildrenList = new List<GameObject>();
    private List<GameObject> tipsList = new List<GameObject>();
    private GameObject tipsObjects;
    private GameObject hideShowTipsButton;
    private bool isTipsActive = true;
    private int tipIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetChildren(gameObject,childrenList);
        tipsObjects = childrenList[0].gameObject;
        hideShowTipsButton = childrenList[1].gameObject;

        //hideShowTextList[0] is "Hide Tips" and [1] is "Show Tips"
        GetChildren(hideShowTipsButton,hideShowTextList);

        //get children of tipsObjects
        GetChildren(tipsObjects,tipsObjectChildrenList);

        //tipsList holds all tip text gameobjects
        GetChildren(tipsObjectChildrenList[0],tipsList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideOrShowTips()
    {
        //set tips parent to active or not active
        isTipsActive = !isTipsActive;
        tipsObjects.SetActive(isTipsActive);

        //switch text to "Hide Text" or "Show Tips" correspondingly to if tips are active
        if (isTipsActive)
            SetHideOrShowTextActive(hideShowTextList[0],hideShowTextList[1]);
        else
            SetHideOrShowTextActive(hideShowTextList[1],hideShowTextList[0]);
    }

    //allows for traversion of tips, tipDirection being +/-1 indicating to go to next or previous tip
    public void TraverseTips(int tipDirection)
    {
        tipsList[tipIndex].SetActive(false); //set last tip text inactive
        tipIndex+= tipDirection; //increase or decrease tipIndex by one

        if (tipIndex == tipsList.Count) //if is last tip and is going to next tip, go back to first tip
            tipIndex = 0;
        else if (tipIndex == -1) //if is first tip and is going to previous tip, go to last tip
            tipIndex = tipsList.Count-1;

        tipsList[tipIndex].SetActive(true); //make new tip active
    }

    //sets one gameobject to active and the other to inactive
    private void SetHideOrShowTextActive(GameObject active, GameObject inactive)
    {
        active.SetActive(true);
        inactive.SetActive(false);
    }

    public void GetChildren(GameObject parent, List<GameObject> children)
    {
        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }
    }
}
