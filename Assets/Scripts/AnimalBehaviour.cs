using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void PenguinBehaviour(Rigidbody penguinRb, float speed)
    {
        //penguin walk forwards (relative to their local forward direction)
        Vector3 newPos =  penguinRb.position + penguinRb.transform.forward * speed *  Time.deltaTime;
        penguinRb.MovePosition(newPos);
    }

    public void AnimalHit()
    {
        
    }
}
