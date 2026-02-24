using UnityEngine;

public class PenguinBehaviour : AnimalBehaviour
{
    private Rigidbody penguinRb;
    private Animator penguinAnim;
    private float walkSpeed;
    private string isHitBoolName = "isHit";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        penguinRb = GetRigidbody();
        penguinAnim = GetAnimator();

        walkSpeed = Random.Range(1f,1.5f);
        penguinAnim.speed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //while not hit, continue walking.
        if (!penguinAnim.GetBool(isHitBoolName))
        {
            MoveForwardRB(penguinRb,walkSpeed);
        }
        
    }

    public void StopHitAnimation()
    {
        AnimalEndAnimation(penguinAnim,isHitBoolName);
    }
}
