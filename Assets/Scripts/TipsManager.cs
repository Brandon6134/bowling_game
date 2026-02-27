using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TipsManager : MonoBehaviour
{
    private List<GameObject> childrenList = new List<GameObject>();
    private List<GameObject> hideShowTextList = new List<GameObject>();
    private GameObject tipsObjects;
    private GameObject hideShowTipsButton;
    private bool isTipsActive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetChildren(gameObject,childrenList);
        tipsObjects = childrenList[0].gameObject;
        hideShowTipsButton = childrenList[1].gameObject;

        //hideShowTextList[0] is "Hide Tips" and [1] is "Show Tips"
        GetChildren(hideShowTipsButton,hideShowTextList);
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
