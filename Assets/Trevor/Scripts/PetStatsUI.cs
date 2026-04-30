using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required to access the Image component

public class PetStatsUI : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("The parent objects holding the chunks. Add a Horizontal Layout Group to these.")]
    public Transform hungerContainer;
    public Transform cleanlinessContainer;
    public Transform loveContainer;

    [Header("Bar Colors")]
    public Color hungerColor = Color.white;
    public Color cleanlinessColor = Color.white;
    public Color loveColor = Color.white;

    [Header("Flashing Settings")]
    [Tooltip("The color it flashes when on the last chunk.")]
    public Color criticalFlashColor = Color.red;
    [Tooltip("How fast it flashes when the last chunk is completely full.")]
    public float minFlashSpeed = 5f;
    [Tooltip("How fast it flashes when the stat is almost at 0.")]
    public float maxFlashSpeed = 10f;

    [Header("Prefabs & Settings")]
    [Tooltip("The single white UI Image prefab that represents one chunk.")]
    public GameObject statChunkPrefab;
    [Tooltip("How many stat points one visual chunk represents.")]
    public float pointsPerChunk = 10f;

    private PetStats currentPet;

    // Lists to keep track of the spawned chunk GameObjects
    private List<GameObject> hungerChunks = new List<GameObject>();
    private List<GameObject> cleanlinessChunks = new List<GameObject>();
    private List<GameObject> loveChunks = new List<GameObject>();

    [Header("UI Root")]
    [SerializeField] private GameObject statsUIRoot;
    private bool barsBuilt = false;

    void Start()
    {
        AssignPetInRoom();
    }

    public void AssignPetInRoom()
    {
        currentPet = Object.FindFirstObjectByType<PetStats>();

        if (currentPet != null)
        {
            statsUIRoot.SetActive(true);

            if (!barsBuilt)
            {
                BuildStatBar(currentPet.maxHunger, hungerContainer, hungerChunks, hungerColor);
                BuildStatBar(currentPet.maxCleanliness, cleanlinessContainer, cleanlinessChunks, cleanlinessColor);
                BuildStatBar(currentPet.maxLove, loveContainer, loveChunks, loveColor);
                barsBuilt = true;
            }
        }
        else
        {
            statsUIRoot.SetActive(false);
            currentPet = null;
            barsBuilt = false;

            hungerChunks.Clear();
            cleanlinessChunks.Clear();
            loveChunks.Clear();

            Debug.Log("No pet in room: hiding stats UI");
        }
    }

    void Update()
    {
        if (currentPet == null)
        {
            PetStats foundPet = Object.FindFirstObjectByType<PetStats>();
            if (foundPet != null)
            {
                AssignPetInRoom();
                return;
            }
        }

        if (currentPet != null)
        {
            // We now pass the default color in so it knows what to revert to when not flashing
            UpdateStatBar(currentPet.hunger, hungerChunks, hungerColor);
            UpdateStatBar(currentPet.cleanliness, cleanlinessChunks, cleanlinessColor);
            UpdateStatBar(currentPet.love, loveChunks, loveColor);
        }
    }

    private void BuildStatBar(float maxStat, Transform container, List<GameObject> chunkList, Color chunkColor)
    {
        // Clear old chunks
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        chunkList.Clear();

        int totalChunksNeeded = Mathf.CeilToInt(maxStat / pointsPerChunk);

        for (int i = 0; i < totalChunksNeeded; i++)
        {
            // Spawn the new chunk
            GameObject newChunk = Instantiate(statChunkPrefab, container);

            // Grab the Image component and set its color
            Image chunkImage = newChunk.GetComponent<Image>();
            if (chunkImage != null)
            {
                chunkImage.color = chunkColor;
            }

            chunkList.Add(newChunk);
        }
    }

    private void UpdateStatBar(float currentStat, List<GameObject> chunkList, Color defaultColor)
    {
        int activeChunks = Mathf.CeilToInt(currentStat / pointsPerChunk);

        for (int i = 0; i < chunkList.Count; i++)
        {
            if (chunkList[i] == null) continue;

            bool isActive = i < activeChunks;
            chunkList[i].SetActive(isActive);

            // Handle the color and flashing logic
            if (isActive)
            {
                Image chunkImage = chunkList[i].GetComponent<Image>();
                if (chunkImage != null)
                {
                    // If we are on the very last chunk
                    if (activeChunks == 1 && i == 0)
                    {
                        // Calculate percentage of the last chunk remaining (0.0 to 1.0)
                        float percentageLeft = Mathf.Clamp01(currentStat / pointsPerChunk);

                        // Speed up as percentage drops closer to 0
                        float currentSpeed = Mathf.Lerp(maxFlashSpeed, minFlashSpeed, percentageLeft);

                        // Create a pulsing value between 0 and 1
                        float pulse = (Mathf.Sin(Time.time * currentSpeed) + 1f) / 2f;

                        // Blend between the critical color and the default color
                        chunkImage.color = Color.Lerp(criticalFlashColor, defaultColor, pulse);
                    }
                    else
                    {
                        // Reset back to normal color just in case it was flashing previously
                        chunkImage.color = defaultColor;
                    }
                }
            }
        }
    }
}