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

    public void FinishedSelectAnimation()
    {
        menuActionsScript.ReturnButton(menuActionsScript.characterSelectPanel);
    }
}
