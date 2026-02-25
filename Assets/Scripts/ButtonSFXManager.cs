using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSFXManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip startGameSound;
    public AudioClip quitGameSound;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //add to dont destroy on load scene, so start and quit sounds play even after new scene is loaded
        DontDestroyOnLoad(gameObject);
    }

    public void CallPlayStartGameSound()
    {
        StartCoroutine(PlayStartGameSound());
    }

    // play button sound, wait till it's done playing, then delete object from the dontdestroyonload scene
    private IEnumerator PlayStartGameSound()
    {
        audioSource.PlayOneShot(startGameSound,2f);
        yield return new WaitForSeconds(startGameSound.length);
        Destroy(gameObject);
    }

    public void CallPlayQuitGameSound()
    {
        StartCoroutine(PlayQuitGameSound());
    }

    public IEnumerator PlayQuitGameSound()
    {
        audioSource.PlayOneShot(quitGameSound);
        yield return new WaitForSeconds(quitGameSound.length);
        Destroy(gameObject);
    }
}
