using UnityEngine;

public class FishBehaviour : AnimalBehaviour
{
    public float swimSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveForwardTransform(gameObject.transform,swimSpeed);
    }
}
