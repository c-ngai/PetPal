using UnityEngine;

public class PetStats : MonoBehaviour
{
    [Range(0, 100)] public float happiness = 100f;
    [Range(0, 100)] public float cleanliness = 100f;
    [Range(0, 100)] public float love = 100f;

    public float depletionRate = 2f;

    void Update()
    {
        happiness = Mathf.Clamp(happiness - (depletionRate * Time.deltaTime), 0f, 100f);
        cleanliness = Mathf.Clamp(cleanliness - (depletionRate * Time.deltaTime), 0f, 100f);
        love = Mathf.Clamp(love - (depletionRate * Time.deltaTime), 0f, 100f);
    }

    public void BoostHappiness(float amount) { happiness = Mathf.Clamp(happiness + amount, 0f, 100f); }
    public void BoostCleanliness(float amount) { cleanliness = Mathf.Clamp(cleanliness + amount, 0f, 100f); }
    public void BoostLove(float amount) { love = Mathf.Clamp(love + amount, 0f, 100f); }
}