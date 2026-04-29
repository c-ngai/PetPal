using UnityEngine;
using System.Collections;

public class MiniGamePet : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public float jumpForce = 10f;
    public float gravityMultiplier = 2.5f;
    public PlayMiniGameManager gameManager;

    [Header("Squash and Stretch")]
    public Vector3 stretchScale = new Vector3(0.7f, 1.3f, 1f); // Taller and thinner
    public Vector3 squishScale = new Vector3(1.3f, 0.7f, 1f);  // Shorter and wider
    public float animSpeed = 15f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);

        // Save the pet's default scale so we can return to it
        originalScale = transform.localScale;

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

            // Cancel any current squish/stretch and start the stretch
            StopAllCoroutines();
            StartCoroutine(DoSquashStretch(stretchScale));

            // EVENT TRIGGER: Broadcast that the pet jumped
            GameEvents.OnPetJump?.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Only squish if the pet is actually landing from a jump
            if (!isGrounded)
            {
                StopAllCoroutines();
                StartCoroutine(DoSquashStretch(squishScale));
            }
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

    private IEnumerator DoSquashStretch(Vector3 targetScale)
    {
        float t = 0f;
        Vector3 currentScale = transform.localScale;

        // Animate to the target scale (stretch or squish)
        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
            yield return null;
        }

        t = 0f;
        currentScale = transform.localScale;

        // Animate smoothly back to the normal scale
        while (t < 1f)
        {
            // Slower recovery speed makes it look a bit more bouncy
            t += Time.deltaTime * (animSpeed * 0.75f);
            transform.localScale = Vector3.Lerp(currentScale, originalScale, t);
            yield return null;
        }

        // Snap back to exactly normal at the very end
        transform.localScale = originalScale;
    }
}