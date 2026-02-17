using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalBehaviour : MonoBehaviour
{
    public Animator GetAnimator()
    {
        return GetComponent<Animator>();
    }

    public Rigidbody GetRigidbody()
    {
        return GetComponent<Rigidbody>();
    }
    
    public void MoveForwardRB(Rigidbody animalRb, float speed)
    {
        //animal walks forwards (relative to their local forward direction)
        Vector3 newPos =  animalRb.position + animalRb.transform.forward * speed *  Time.deltaTime;
        animalRb.MovePosition(newPos);
    }

    // public void AnimalHit(Animator anim, string isHitAnimBoolName)
    // {
    //     anim.SetBool(isHitAnimBoolName,true);
    // }

    //this func is called through an animation event at the end of a "is hit" animation for an animal
    public void AnimalEndAnimation(Animator anim, string isHitAnimBoolName)
    {
        //stop the "is hit" animation
        anim.SetBool(isHitAnimBoolName,false);
    }

    public void MoveForwardTransform(Transform tr, float speed)
    {
        tr.position += tr.forward * speed * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Delete Wall"))
        {
            Destroy(gameObject);
            //print("deleted fishy");
        }   
    }

    // protected virtual void OnCollisionEnter(Collision other) 
    // {
    //     if (other.CompareTag("Delete Wall"))
    //     Destroy(gameObject);
    //     print("deleted fishy333");
    // }
}
