using UnityEngine;
using System; // Required for DateTime and TimeSpan

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

    public float depletionRate = 2f; // Depletion per second

    // Track if this is a "real" pet or a shop display pet
    private bool isInitialized = false;
    private string myRoomID;

    public static event Action<string> OnAnyPetStatDepleted;

    private bool hasTriggeredGameOver = false;

    // The Room will call this right after spawning the pet
    public void Initialize(string assignedRoomID)
    {
        myRoomID = assignedRoomID;
        isInitialized = true;
        LoadAndCatchUpStats();
    }

    void Update()
    {
        // Don't deplete stats if this is just a display egg in the shop
        if (!isInitialized) return;

        // Live depletion while the pet is in the active scene
        hunger = Mathf.Clamp(hunger - (depletionRate * Time.deltaTime), 0f, maxHunger);
        cleanliness = Mathf.Clamp(cleanliness - (depletionRate * Time.deltaTime), 0f, maxCleanliness);
        love = Mathf.Clamp(love - (depletionRate * Time.deltaTime), 0f, maxLove);

        if (!hasTriggeredGameOver && (hunger <= 0f || cleanliness <= 0f || love <= 0f))
        {
            hasTriggeredGameOver = true;
            OnAnyPetStatDepleted?.Invoke(myRoomID);
        }
    }

    // Save when the player leaves the room (scene changes or object is destroyed)
    void OnDestroy()
    {
        if (isInitialized) SaveStats();
    }

    // Save if the player minimizes the game on their phone/PC
    void OnApplicationPause(bool isPaused)
    {
        if (isPaused && isInitialized) SaveStats();
    }

    // Save if the player force-closes the game
    void OnApplicationQuit()
    {
        if (isInitialized && isInitialized) SaveStats();
    }

    private void LoadAndCatchUpStats()
    {
        if (GameManager.Instance == null) return;

        // Use MY ID, not the global GameManager ID
        if (GameManager.Instance.roomPets.TryGetValue(myRoomID, out PetData data))
        {
            // Fallback for brand new pets to prevent massive depletion
            if (data.lastSavedTime == 0)
            {
                data.lastSavedTime = DateTime.UtcNow.Ticks;
            }

            // 1. Calculate the exact time away
            DateTime lastSaved = new DateTime(data.lastSavedTime);
            TimeSpan timeAway = DateTime.UtcNow - lastSaved;
            float secondsAway = (float)timeAway.TotalSeconds;

            // 2. Calculate the missed depletion
            float totalDepletion = secondsAway * depletionRate;

            // 3. Apply it to the saved stats
            hunger = Mathf.Clamp(data.hunger - totalDepletion, 0f, maxHunger);
            cleanliness = Mathf.Clamp(data.cleanliness - totalDepletion, 0f, maxCleanliness);
            love = Mathf.Clamp(data.love - totalDepletion, 0f, maxLove);

            if (hunger <= 0f || cleanliness <= 0f || love <= 0f)
            {
                OnAnyPetStatDepleted?.Invoke(myRoomID);
            }
        }
    }

    private void SaveStats()
    {
        if (GameManager.Instance == null) return;

        // Use MY ID to save, not the global GameManager ID
        if (GameManager.Instance.roomPets.TryGetValue(myRoomID, out PetData data))
        {
            data.hunger = hunger;
            data.cleanliness = cleanliness;
            data.love = love;

            // Record the exact time we saved
            data.lastSavedTime = DateTime.UtcNow.Ticks;
        }
    }

    public void BoostHunger(float amount) { hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger); }
    public void BoostCleanliness(float amount) { cleanliness = Mathf.Clamp(cleanliness + amount, 0f, maxCleanliness); }
    public void BoostLove(float amount) { love = Mathf.Clamp(love + amount, 0f, maxLove); }
}