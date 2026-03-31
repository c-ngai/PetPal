using UnityEngine;

public class PetStats : MonoBehaviour
{
    [Header("Current Stats")]
    public float hunger = 100f;
    public float cleanliness = 100f;
    public float love = 100f;

    [Header("Max Stats (Set per Pet)")]
    public float maxHunger = 100f;
    public float maxCleanliness = 100f;
    public float maxLove = 100f;

    public float depletionRate = 2f;

    void Update()
    {
        // Clamp to the dynamic max values instead of 100f
        hunger = Mathf.Clamp(hunger - (depletionRate * Time.deltaTime), 0f, maxHunger);
        cleanliness = Mathf.Clamp(cleanliness - (depletionRate * Time.deltaTime), 0f, maxCleanliness);
        love = Mathf.Clamp(love - (depletionRate * Time.deltaTime), 0f, maxLove);
    }

    public void BoostHappiness(float amount) { hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger); }
    public void BoostCleanliness(float amount) { cleanliness = Mathf.Clamp(cleanliness + amount, 0f, maxCleanliness); }
    public void BoostLove(float amount) { love = Mathf.Clamp(love + amount, 0f, maxLove); }
}