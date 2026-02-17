using UnityEngine;

public class FishBehaviour : AnimalBehaviour
{
    private Animator fishAnim;
    private float swimSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fishAnim = GetAnimator();
    }

    // Update is called once per frame
    void Update()
    {   
        swimSpeed = Random.Range(1f,7f);
        fishAnim.speed = swimSpeed/2f;
        MoveForwardTransform(gameObject.transform,swimSpeed);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    // protected override void OnCollisionEnter(Collision other)
    // {
    //     base.OnCollisionEnter(other);
    // }
}
