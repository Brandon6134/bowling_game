using UnityEngine;

public class PenguinBehaviour : AnimalBehaviour
{
    private Rigidbody penguinRb;
    private Animator penguinAnim;
    public float walkSpeed = 1f;
    private string isHitBoolName = "isHit";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        penguinRb = GetComponent<Rigidbody>();
        penguinAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //while not hit, continue walking.
        if (!penguinAnim.GetBool(isHitBoolName))
        {
            PenguinBehaviour(penguinRb,walkSpeed);
        }
        
    }

    public void StopHitAnimation()
    {
        AnimalEndAnimation(penguinAnim,isHitBoolName);
    }
}
