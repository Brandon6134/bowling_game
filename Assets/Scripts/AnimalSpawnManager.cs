using System.Collections;
using UnityEngine;

public class AnimalSpawnManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SpawnInIntervals(GameObject obj, Vector3 spawnPos, float interval)
    {
        yield return new WaitForSeconds(interval);
        Instantiate(obj,spawnPos,obj.transform.rotation);
    }
}
