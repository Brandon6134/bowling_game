using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Unity.Collections;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject parentBiome;
    public GameObject player;
    private Rigidbody playerRb;
    public GameObject portalParent;
    public GameObject alleyLane;
    public GameObject playerGround;
    private Portal_Controller portal_ControllerScript;
    private List<Transform> biomeList = new List<Transform>(); //holds transforms of all biomes
    public int currentBiomeIndex = 0; //global index that tracks the current active biome index
    private List<Transform> currentBiomeObstacles = new List<Transform>();
    private int currentBiomeObstacleIndex = -1; //initialize as impossible index value
    public ParticleSystem portalSpawnVFX;
    private AudioSource laserAudioSource;
    public AudioClip laserSFX;
    public AudioClip portalWarpSFX;
    public bool isEnterPortalSequence = false;
    public bool exitedPortal = false;
    public bool hasExitedPortalTrigger = false;
    public bool hasEnteredPortal = false;
    private List<(float,float)> physicMods = new List<(float,float)>();

    void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        portal_ControllerScript = portalParent.transform.GetChild(0).gameObject.GetComponent<Portal_Controller>();
        
        InitializeTransformList(parentBiome,biomeList);
        InitializeBiomePhysicsValuesArray(physicMods);
        AssignNewBiome(0); //working on ___ biome for now

        laserAudioSource = portalSpawnVFX.GetComponent<AudioSource>();
    }

    public IEnumerator ChangeBiome(int newBiomeIndex)
    {
        //disables footstep sfx
        hasEnteredPortal = true;

        //play warp sfx upon entering portal
        laserAudioSource.PlayOneShot(portalWarpSFX);

        //wait 2 seconds befor portal transition (impact + let warp sfx finish)
        yield return new WaitForSeconds(2f);

        //set last biome inactive
        biomeList[currentBiomeIndex].gameObject.SetActive(false);

        //assign new biome active
        AssignNewBiome(newBiomeIndex);

        //choose initial obstacle
        CallBiomeObstacleChooser();
        
        StartCoroutine(MovePortalAndPlayer());
        print("Biome Changed!");
    }

    public IEnumerator MovePortalAndPlayer()
    {
        //move player and portal backwards after entering portal
        playerRb.position += new Vector3(-15f,0f,0f);
        portalParent.transform.position += new Vector3(-15f,0f,0f);

        //wait small amount of time before allowing player to moveforward again (prevent player stutter forward)
        yield return new WaitForSeconds(0.1f);
        hasExitedPortalTrigger = false;
        hasEnteredPortal = false;

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

        //move portal back to initial position
        portalParent.transform.position += new Vector3(15f,0f,0f);
        print("disabling portal!");
    }

    //perform all the functions travelling to the new biome
    public void AssignNewBiome(int newBiomeIndex)
    {   
        //set new biome index value and make active
        currentBiomeIndex = newBiomeIndex;
        biomeList[currentBiomeIndex].gameObject.SetActive(true);

        //set skybox, alley and player ground mats
        Material[] mats = biomeList[currentBiomeIndex].gameObject.GetComponent<MeshRenderer>().materials;
        RenderSettings.skybox = mats[0];
        playerGround.GetComponent<MeshRenderer>().material = mats[1];
        alleyLane.GetComponent<MeshRenderer>().material = mats[2];

        //set gravity and dynamic friction values
        (float,float) biomeMods = physicMods[newBiomeIndex];
        Physics.gravity = new Vector3(0,-biomeMods.Item1,0);
        alleyLane.GetComponent<MeshCollider>().material.dynamicFriction = biomeMods.Item2;

        //enable fog or not
        if (biomeList[currentBiomeIndex].name.Equals("Ocean") )
            RenderSettings.fog = true;
        else
            RenderSettings.fog = false;

        //initialize obstacles list
        InitializeTransformList(biomeList[currentBiomeIndex].Find("Obstacles").gameObject,currentBiomeObstacles);
    }

    private void ChooseBiomeObstacle(ref int oldRandomInt)
    {
        if (oldRandomInt!=-1) //if game was just initialized, dont make last obstacle inactive
        {
            //remove last frame obstacle from list and set inactive
            currentBiomeObstacles[oldRandomInt].gameObject.SetActive(false);
            currentBiomeObstacles.Remove(currentBiomeObstacles[oldRandomInt]);
        }
        
        //make new random biome obstacle active
        int newRandomInt = UnityEngine.Random.Range(0,currentBiomeObstacles.Count);
        currentBiomeObstacles[newRandomInt].gameObject.SetActive(true);

        print(currentBiomeObstacles[newRandomInt].name);

        oldRandomInt = newRandomInt; //changes oldRandomInt to the newRandomInt value
    }

    //public func that calls upon biome chooser, call from other funcs
    public void CallBiomeObstacleChooser()
    {
        ChooseBiomeObstacle(ref currentBiomeObstacleIndex);
    }

    //reusable func to grab all children from a gameobject parent, fill a list with its children
    public void InitializeTransformList(GameObject parent, List<Transform> list)
    {
        foreach(Transform child in parent.transform)
        {
            list.Add(child);
        }
    }

    //stores and sets gravity and dynamic friction (slipperyness) of alley lane
    private void InitializeBiomePhysicsValuesArray(List<(float,float)> list)
    {
        list.Add((25f,0.3f)); //classic
        list.Add((25f,0.5f)); //forest or jungle
        list.Add((25f,0.05f)); //arctic
        list.Add((15f,0.3f)); //ocean
        list.Add((0f,0.3f)); //outer space
        list.Add((25f,0.3f)); //prehistoric
    }
}
