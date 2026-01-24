using UnityEditor;
using UnityEngine;

public class CharacterPreviewAnimations : MonoBehaviour
{
    private MenuActions menuActionsScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuActionsScript = GameObject.Find("Canvas").GetComponent<MenuActions>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this func is called from a event key at the end of a preview animation duration
    public void FinishedSelectAnimation()
    {
        //once preview animation is done, close char panel and go back to main menu
        menuActionsScript.ReturnButton(menuActionsScript.characterSelectPanel);

        //reset character model's position and rotation
        menuActionsScript.charGameObject.position = menuActionsScript.originalPosition;
        menuActionsScript.charGameObject.rotation = Quaternion.identity;
    }
}
