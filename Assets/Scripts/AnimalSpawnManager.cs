using System.Collections;
using System.Collections.Generic;
using Ionic.Zip;
using UnityEngine;

public class AnimalSpawnManager : MonoBehaviour
{
    public List<GameObject> spawningObjects;
    public float spawnInterval;
    private Camera cam;
    private Vector3 bottomLeftOfScreen;
    private Vector3 topLeftOfScreen;
    private Vector3 bottomRightOfScreen;
    private Vector3 topRightOfScreen;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        float z = Mathf.Abs(cam.transform.position.z)+50f;

        bottomLeftOfScreen  = cam.ViewportToWorldPoint(new Vector3(0, 0.05f, z));
        topLeftOfScreen     = cam.ViewportToWorldPoint(new Vector3(0, 0.95f, z));
        bottomRightOfScreen = cam.ViewportToWorldPoint(new Vector3(1, 0.05f, z));
        topRightOfScreen    = cam.ViewportToWorldPoint(new Vector3(1, 0.95f, z));
        
        InvokeRepeating("SpawnController",0f,spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void SpawnController()
    {
        int randomIndex = Random.Range(0,spawningObjects.Count);
        
        float randomX = Random.Range(-15f,30f);
        float randomY = Random.Range(0f,47f);
        float randomZ = Random.value < 0.5f ? -50f : 50f;

        Vector3 randomSpawnPos = new Vector3(randomX,randomY,randomZ);

        SpawnInIntervals(spawningObjects[randomIndex],randomSpawnPos, 1f);
    }

    public void SpawnInIntervals(GameObject obj, Vector3 spawnPos, float rotationMod)
    {     
            float yRotation = spawnPos.z < 0 ? 0f : 180f;
            
            Instantiate(obj,spawnPos,Quaternion.Euler(0f,yRotation,0f));
    }
    

}
