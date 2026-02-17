using UnityEngine;
using UnityEngine.UIElements;

public class SharkBehaviour : AnimalBehaviour
{
    private Rigidbody sharkRb;
    private Animator sharkAnim;
    public float swimSpeed = 5f;
    public float zRange;
    private float initialPosZ;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sharkAnim = GetAnimator();
        sharkRb = GetRigidbody();
        sharkAnim.speed = 1.5f;
        initialPosZ = sharkRb.position.z;
    }

    // Update is called once per frame
    void Update()
    {   
        //if shark reaches boundaries, flip 180 degrees
        if (initialPosZ - sharkRb.position.z > 0.5f || (initialPosZ+zRange) - sharkRb.position.z < -0.5f)
        {
            transform.Rotate(0f,180f,0f);
        }

        //shark swims forward
        MoveForwardRB(sharkRb,swimSpeed);
    }
}
