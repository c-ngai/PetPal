using UnityEngine;

public class Pet : MonoBehaviour
{
    public enum PetStage
    {
        Egg,
        Pet
    }

    [SerializeField] private Sprite eggSprite;
    [SerializeField] private Sprite petSprite;

    private SpriteRenderer sr;

    public PetStage currentStage = PetStage.Egg;

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
        currentStage = PetStage.Pet;
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (currentStage == PetStage.Egg)
            sr.sprite = eggSprite;
        else
            sr.sprite = petSprite;
    }
}