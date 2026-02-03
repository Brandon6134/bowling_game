using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject parentBiome;
    public GameObject player;
    private Rigidbody playerRb;
    public GameObject portalParent;
    private Portal_Controller portal_ControllerScript;
    private List<Transform> biomeList = new List<Transform>();
    public int currentBiomeIndex = 0; //global index that tracks the current active biome index
    public ParticleSystem portalSpawnVFX;
    private AudioSource laserAudioSource;
    public AudioClip laserSFX;
    public AudioClip portalWarpSFX;
    public bool isEnterPortalSequence = false;
    public bool exitedPortal = false;
    public bool enteredPortal = false;
    void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        portal_ControllerScript = portalParent.transform.GetChild(0).gameObject.GetComponent<Portal_Controller>();
        
        InitializeBiomeList(ref parentBiome);

        laserAudioSource = portalSpawnVFX.GetComponent<AudioSource>();
    }

    public void InitializeBiomeList(ref GameObject parent)
    {
        foreach(Transform child in parent.transform)
        {
            biomeList.Add(child);
        }
    }

    public IEnumerator ChangeBiome(int newBiomeIndex)
    {
        enteredPortal = true;
        laserAudioSource.PlayOneShot(portalWarpSFX);
        yield return new WaitForSeconds(2f);
        
        //wait 1 frame before setting previous biome inactive (can't set inactive same frame as collision)
        //yield return null;

        //set last biome inactive
        biomeList[currentBiomeIndex].gameObject.SetActive(false);

        //resassign new biome index value and make biome active
        currentBiomeIndex = newBiomeIndex;
        biomeList[currentBiomeIndex].gameObject.SetActive(true);

        enteredPortal = false;
        
        MovePortalAndPlayer();
        print("Biome Changed!");
    }

    public void MovePortalAndPlayer()
    {
        //move player and portal backwards after entering portal
        playerRb.position += new Vector3(-15f,0f,0f);
        portalParent.transform.position += new Vector3(-15f,0f,0f);
        Physics.SyncTransforms();

        //set bool stating that player has exited portal (so can delete portal after)
        exitedPortal = true;
    }

    public IEnumerator PortalSpawn()
    {
        isEnterPortalSequence = true;
        yield return new WaitForSeconds(1f); //wait 1 second after round reset (plus wait for cam to come back to player), then play beam vfx

        //portalParent.transform.GetChild(2).gameObject.SetActive(true);
        portalSpawnVFX.Play();
        laserAudioSource.PlayOneShot(laserSFX,0.2f);

        //wait 0.5 seconds after beam vfx to spawn portal
        yield return new WaitForSeconds(0.5f);
        portalParent.SetActive(true);
        portal_ControllerScript.TogglePortal(true);
    }

    //sets portal sequence bool to false, and sets portal parent to inactive
    public void PortalDisable()
    {
        isEnterPortalSequence = false;
        exitedPortal = false;
        portalParent.SetActive(false);
        print("disabling portal!");
    }
}
