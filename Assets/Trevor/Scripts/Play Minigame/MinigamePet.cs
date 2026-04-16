using UnityEngine;

public class MiniGamePet : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public float jumpForce = 10f;
    public float gravityMultiplier = 2.5f;
    public PlayMiniGameManager gameManager;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);

        // Apply the correct sprite handoff from the GameManager
        if (GameManager.Instance != null && GameManager.Instance.activeMinigameSprite != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = GameManager.Instance.activeMinigameSprite;
            }
        }
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            // EVENT TRIGGER: Broadcast that the pet jumped
            GameEvents.OnPetJump?.Invoke();
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
        if (wildlife.CompareTag("Obstacle"))
        {
            gameManager.AddBonusScore(50f);
        }
    }
}