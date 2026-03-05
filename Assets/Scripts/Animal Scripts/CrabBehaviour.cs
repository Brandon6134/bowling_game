using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
    public GameObject oceanBiomeParent;
    private AudioSource oceanBGM;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        oceanBGM = oceanBiomeParent.GetComponents<AudioSource>()[1];
        oceanBGM.Pause();
    }

    void OnDisable()
    {
        oceanBGM.UnPause();
    }
}
