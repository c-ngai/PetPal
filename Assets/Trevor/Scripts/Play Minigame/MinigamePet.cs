using UnityEngine;

public class MiniGamePet : MonoBehaviour
{
    public float jumpForce = 10f;
    public float gravityMultiplier = 2.5f;
    public PlayMiniGameManager gameManager;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure gravity feels "snappy" like a platformer
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);
    }

    // This method is now public and triggered by your PetController
    public void Jump()
    {
        if (isGrounded)
        {
            // Note: Fixed a small typo from your previous script (ForceForceMode -> ForceMode)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            gameManager.EndGame();
        }
    }

    private void OnTriggerEnter(Collider wildlife)
    {
        // Use a trigger zone behind the pet to detect a successful jump
        if (wildlife.CompareTag("Obstacle"))
        {
            gameManager.AddBonusScore(50f);
        }
    }
}