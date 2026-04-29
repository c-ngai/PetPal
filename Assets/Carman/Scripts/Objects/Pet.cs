using UnityEngine;
using System.Collections;

public class Pet : MonoBehaviour
{
    public enum PetStage
    {
        Egg,
        Pet
    }

    [Header("Sprites")]
    [SerializeField] private Sprite eggSprite;
    [SerializeField] private Sprite petSprite;

    [Header("Hatch Animation Settings")]
    public float wiggleDuration = 1.5f;
    public float wiggleSpeed = 25f;
    public float wiggleMagnitude = 15f;
    public float growDuration = 0.5f;

    public PetStage currentStage = PetStage.Egg;
    public int Price;

    private SpriteRenderer sr;
    private bool isHatching = false;
    private Vector3 originalScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public void SetStage(PetStage stage)
    {
        currentStage = stage;
        UpdateSprite();
    }

    public void Hatch()
    {
        // Prevent calling the hatch animation multiple times at once
        if (!isHatching)
        {
            // FORCE the stage to Egg to ensure the sprite is correct before animating
            currentStage = PetStage.Egg;
            UpdateSprite();

            // Capture the scale right now, AFTER the Room script has resized it
            originalScale = transform.localScale;

            StartCoroutine(HatchRoutine());
        }
    }

    void UpdateSprite()
    {
        if (currentStage == PetStage.Egg)
            sr.sprite = eggSprite;
        else
            sr.sprite = petSprite;
    }

    private IEnumerator HatchRoutine()
    {
        isHatching = true;

        // Save the starting rotation so we can snap back to it
        Quaternion originalRotation = transform.rotation;
        float timeElapsed = 0f;

        // PHASE 1: Wiggle the egg left and right
        while (timeElapsed < wiggleDuration)
        {
            timeElapsed += Time.deltaTime;

            // Use a Sine wave to smoothly calculate a back-and-forth angle
            float zRotation = Mathf.Sin(timeElapsed * wiggleSpeed) * wiggleMagnitude;
            transform.rotation = Quaternion.Euler(0, 0, zRotation);

            yield return null;
        }

        // Snap rotation back to perfectly upright
        transform.rotation = originalRotation;

        // PHASE 2: Swap the sprite to the pet
        currentStage = PetStage.Pet;
        UpdateSprite();

        // PHASE 3: Spawn small and grow to normal size
        transform.localScale = Vector3.zero; // Shrink to 0 instantly
        timeElapsed = 0f;

        while (timeElapsed < growDuration)
        {
            timeElapsed += Time.deltaTime;

            // Calculate a percentage from 0.0 to 1.0 based on time passed
            float percentComplete = timeElapsed / growDuration;

            // Smoothly scale up
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, percentComplete);

            yield return null;
        }

        // Ensure the scale ends up exactly at the original size
        transform.localScale = originalScale;
        isHatching = false;
    }
}