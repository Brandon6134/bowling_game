using UnityEngine;

public class BearBehaviour : AnimalBehaviour
{
    private Animator bearAnim;
    public float attackInterval = 2.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bearAnim = GetAnimator();
        InvokeRepeating("SetAttackVar",0f,attackInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetAttackVar()
    {
        bearAnim.SetTrigger("Attack1");
    }
}
