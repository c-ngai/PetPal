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

            Debug.Log("No pet in room — hiding stats UI");
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
            UpdateStatBar(currentPet.hunger, hungerChunks);
            UpdateStatBar(currentPet.cleanliness, cleanlinessChunks);
            UpdateStatBar(currentPet.love, loveChunks);
        }
    }

    // Notice we added 'Color chunkColor' to the parameters here
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

    private void UpdateStatBar(float currentStat, List<GameObject> chunkList)
    {
        int activeChunks = Mathf.CeilToInt(currentStat / pointsPerChunk);

        for (int i = 0; i < chunkList.Count; i++)
        {
            if (chunkList[i] != null)
                chunkList[i].SetActive(i < activeChunks);
        }
    }
}