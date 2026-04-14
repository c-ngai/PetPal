using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedMinigameController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public List<FoodItemData> items = new();

    [Header("Timing")]
    public float itemLifetime = 3f;

    private GameObject currentItem;
    private Coroutine lifetimeRoutine;

    // ================= CONTROL =================

    public void StartFeed()
    {
        StopFeed();
        SpawnRandomItem();
    }

    public void StopFeed()
    {
        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        lifetimeRoutine = null;

        if (currentItem != null)
            Destroy(currentItem);

        currentItem = null;
    }

    // ================= SPAWN =================

    public void SpawnRandomItem()
    {
        if (items.Count == 0 || spawnPoint == null)
            return;

        if (currentItem != null)
            Destroy(currentItem);

        int index = Random.Range(0, items.Count);
        FoodItemData chosen = items[index];

        currentItem = Instantiate(chosen.prefab, spawnPoint.position, Quaternion.identity);

        var behaviour = currentItem.GetComponent<FoodItemBehaviour>();
        if (behaviour != null)
            behaviour.Initialize(this, chosen.isFood);

        // RESET TIMER EVERY SPAWN
        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        lifetimeRoutine = StartCoroutine(ItemLifetimeRoutine());
    }

    // ================= TIMEOUT SYSTEM =================

    private IEnumerator ItemLifetimeRoutine()
    {
        yield return new WaitForSeconds(itemLifetime);

        // If item still exists (not consumed)
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;

            SpawnRandomItem(); // force progression
        }
    }

    // ================= CONSUME =================

    public void ConsumeItem(bool isFood, PetStats stats)
    {
        if (stats == null) return;

        if (isFood)
        {
            stats.BoostHunger(25f);
            Debug.Log("Fed pet! Hunger increased.");
        }
        else
        {
            stats.BoostHunger(-15f);
            Debug.Log("Gave pet junk! Hunger decreased.");
        }

        if (currentItem != null)
            Destroy(currentItem);

        currentItem = null;

        if (stats.hunger >= stats.maxHunger)
        {
            EndMinigame(stats);
            return;
        }

        // reset timer + continue loop
        SpawnRandomItem();
    }

    private void EndMinigame(PetStats stats)
    {
        Debug.Log("Feeding complete!");

        StopFeed();
        GameManager.Instance.SetState(GameManager.GameState.RoomSelected);
    }
}