using UnityEngine;

public class PenguinBehaviour : AnimalBehaviour
{
    private Rigidbody penguinRb;
    public float walkSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        penguinRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        PenguinBehaviour(penguinRb,walkSpeed);
    }
}
