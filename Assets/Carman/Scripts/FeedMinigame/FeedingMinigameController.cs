using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedMinigameController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public List<FoodItemData> items = new();
    public float foodSpawnChance = .75f;

    [Header("Timing")]
    public float itemLifetime = 3f;

    private GameObject currentItem;
    private Coroutine lifetimeRoutine;
    private bool isConsuming;

    public void StartFeed()
    {
        StopFeed();
        SpawnRandomItem();
    }

    public void StopFeed()
    {
        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        isConsuming = false;
    }

    public void SpawnRandomItem()
    {
        if (items.Count == 0 || spawnPoint == null)
            return;

        // Clean up existing item
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        bool spawnFood = Random.value < foodSpawnChance;

        List<FoodItemData> filtered = items.FindAll(i => i.isFood == spawnFood);

        if (filtered.Count == 0)
        {
            filtered = items;
        }

        int index = Random.Range(0, filtered.Count);
        FoodItemData chosen = filtered[index];

        currentItem = Instantiate(chosen.prefab, spawnPoint.position, Quaternion.identity);

        var behaviour = currentItem.GetComponent<FoodItemBehaviour>();
        if (behaviour != null)
            behaviour.Initialize(this, chosen.isFood);

        // Reset lifetime timer
        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        lifetimeRoutine = StartCoroutine(ItemLifetimeRoutine());
    }

    private IEnumerator ItemLifetimeRoutine()
    {
        yield return new WaitForSeconds(itemLifetime);

        // Destroy if still exists
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        // ALWAYS spawn next item
        SpawnRandomItem();
    }

    public void EatItem(bool isFood, PetStats stats)
    {
        if (isConsuming) return;
        isConsuming = true;

        if (stats == null)
        {
            isConsuming = false;
            return;
        }

        // Stop lifetime timer so it doesn't interfere
        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        // Apply effect
        if (isFood)
            stats.BoostHunger(25f);
        else
            stats.BoostHunger(-15f);

        // Destroy current item
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        // Check for end condition
        if (stats.hunger >= stats.maxHunger)
        {
            EndMinigame(stats);
            return;
        }

        StartCoroutine(SpawnNextItemDelay());
    }

    private IEnumerator SpawnNextItemDelay()
    {
        yield return new WaitForSeconds(1f);

        isConsuming = false;
        SpawnRandomItem();
    }

    private void EndMinigame(PetStats stats)
    {
        StopFeed();

        PetController pet = FindFirstObjectByType<PetController>();
        if (pet != null)
            pet.FinishingFeeding();

        GameManager.Instance.SetState(GameManager.GameState.RoomSelected);
    }
}