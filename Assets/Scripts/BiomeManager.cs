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
    private List<Transform> biomeList = new List<Transform>();
    public int currentBiomeIndex = 0; //global index that tracks the current active biome index
    void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        InitializeBiomeList(ref parentBiome);
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
        //wait 1 frame before setting previous biome inactive
        yield return null;

        //set last biome inactive
        biomeList[currentBiomeIndex].gameObject.SetActive(false);

        //resassign new biome index value and make biome active
        currentBiomeIndex = newBiomeIndex;
        biomeList[currentBiomeIndex].gameObject.SetActive(true);
        
        MovePortalAndPlayer();
        print("Biome Changed!");
    }

    public void MovePortalAndPlayer()
    {
        playerRb.MovePosition(playerRb.position + new Vector3(-15f,0f,0f));
        portalParent.transform.position += new Vector3(-15f,0f,0f);
        
    }
}
