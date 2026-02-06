using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalBehaviour : MonoBehaviour
{
    public void PenguinBehaviour(Rigidbody penguinRb, float speed)
    {
        //penguin walk forwards (relative to their local forward direction)
        Vector3 newPos =  penguinRb.position + penguinRb.transform.forward * speed *  Time.deltaTime;
        penguinRb.MovePosition(newPos);
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
}
