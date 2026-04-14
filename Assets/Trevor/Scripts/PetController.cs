using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

[RequireComponent(typeof(PetStats))]
public class PetController : MonoBehaviour
{
    public InputManager inputManager;
    private PetStats stats;

    private enum PetState { Idle, Playing, Cleaning, Feeding }
    private PetState currentState = PetState.Idle;

    [Header("Animation Settings")]
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;
    public float walkSpeed = 2f;
    public float approachDistance = 2f;

    // ================= CLEANING (UNCHANGED CORE LOGIC) =================
    [Header("Cleaning Settings")]
    public float cleanDuration = 3f;
    private float cleanProgress = 0f;

    private bool isCleaningInputActive = false;
    private float cleanHoldTimer = 0f;
    private const float maxHoldDuration = 3f;

    [SerializeField] private Transform debrisRoot;
    private List<GameObject> debrisObjects = new List<GameObject>();

    // ================= FEEDING =================
    [Header("Feeding Settings")]
    public FeedMinigameController feedGame;

    private bool isFeedInputActive = false;
    private bool isInFeedMode = false;
    private Vector3 homePosition;
    private Vector3 feedTargetPos;

    void Start()
    {
        stats = GetComponent<PetStats>();
        homePosition = transform.position;

        if (debrisRoot != null)
        {
            foreach (Transform child in debrisRoot)
                debrisObjects.Add(child.gameObject);
        }

        feedGame = FindFirstObjectByType<FeedMinigameController>();
    }

    public void Initialize(InputManager manager)
    {
        inputManager = manager;

        if (inputManager != null)
        {
            inputManager.OnPlayAction += HandlePlay;
            inputManager.OnCleanAction += HandleCleanPressed;
            inputManager.OnFeedAction += HandleFeedState;
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnPlayAction -= HandlePlay;
            inputManager.OnCleanAction -= HandleCleanPressed;
            inputManager.OnFeedAction -= HandleFeedState;
        }
    }

    void Update()
    {
        if (stats == null) return;

        UpdateDebrisVisuals();

        HandleFeedMode();           // feed state machine
        HandleFeedMovement();       // feed movement (input gated)

        HandleCleanModeTransitions(); // CLEAN LEFT UNCHANGED STRUCTURE
    }

    // ================= INPUT =================

    private void HandlePlay()
    {
        if (GameManager.Instance.CurrentState != GameState.PlayMode) return;

        if (currentState == PetState.Idle)
            StartCoroutine(PlayRoutine());
    }

    private void HandleCleanPressed()
    {
        if (GameManager.Instance.CurrentState != GameState.CleanMode)
        {
            isCleaningInputActive = false;
            cleanHoldTimer = 0f;
            return;
        }

        isCleaningInputActive = true;
    }

    private void HandleFeedState(bool isActive)
    {
        isFeedInputActive = isActive;
    }

    // ================= FEED SYSTEM (SAFE STATE MACHINE) =================

    private void HandleFeedMode()
    {
        bool inFeed = GameManager.Instance.CurrentState == GameState.FeedMode;

        // ENTER FEED MODE
        if (inFeed && !isInFeedMode)
        {
            isInFeedMode = true;
            currentState = PetState.Feeding;

            feedTargetPos = feedGame.spawnPoint.position;
            feedTargetPos.y = homePosition.y;

            if (feedGame != null)
                feedGame.StartFeed();
        }

        // EXIT FEED MODE
        if (!inFeed && isInFeedMode)
        {
            isInFeedMode = false;
            isFeedInputActive = false;

            if (feedGame != null)
                feedGame.StopFeed();

            currentState = PetState.Idle;
        }
    }

    // ONLY MOVES WHEN INPUT HELD
    private void HandleFeedMovement()
    {
        if (!isInFeedMode) return;

        Vector3 target = isFeedInputActive ? feedTargetPos : homePosition;

        // IMPORTANT: always allow return home even if slightly offset
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            walkSpeed * Time.deltaTime
        );
    }

    public void FinishingFeeding()
    {
        isInFeedMode = false;
        isFeedInputActive = false;
        currentState = PetState.Idle;
    }

    // ================= CLEANING (UNCHANGED LOGIC) =================

    private void HandleCleanModeTransitions()
    {
        if (GameManager.Instance.CurrentState != GameState.CleanMode)
        {
            // HARD EXIT CLEAN MODE RESET (fixes stuck bug)
            if (currentState == PetState.Cleaning)
                currentState = PetState.Idle;

            isCleaningInputActive = false;
            cleanHoldTimer = 0f;
            cleanProgress = 0f;
            return;
        }

        if (isCleaningInputActive)
        {
            RunCleaning();

            cleanHoldTimer += Time.deltaTime;

            if (cleanHoldTimer >= maxHoldDuration)
            {
                ResetCleaning();
            }
        }
    }

    private void RunCleaning()
    {
        if (currentState != PetState.Cleaning)
            currentState = PetState.Cleaning;

        cleanProgress += Time.deltaTime;

        stats.BoostCleanliness(Time.deltaTime * 30f);

        UpdateDebrisVisuals();

        if (cleanProgress >= cleanDuration)
        {
            FinishCleaning();
        }
    }

    private void FinishCleaning()
    {
        cleanProgress = 0f;
        cleanHoldTimer = 0f;
        isCleaningInputActive = false;

        currentState = PetState.Idle;

        stats.BoostCleanliness(100f);

        Debug.Log("Pet cleaned!");

        GameManager.Instance.SetState(GameState.RoomSelected);
    }

    private void ResetCleaning()
    {
        cleanProgress = 0f;
        cleanHoldTimer = 0f;
        isCleaningInputActive = false;

        if (currentState == PetState.Cleaning)
            currentState = PetState.Idle;
    }

    // ================= PLAY =================

    private IEnumerator PlayRoutine()
    {
        currentState = PetState.Playing;

        stats.BoostLove(20f);

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            float newY = homePosition.y + Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(homePosition.x, newY, homePosition.z);
            yield return null;
        }

        transform.position = homePosition;
        currentState = PetState.Idle;
    }

    // ================= VISUALS =================

    private void UpdateDebrisVisuals()
    {
        if (debrisObjects == null || debrisObjects.Count == 0) return;

        float cleanlinessPercent = stats.cleanliness / stats.maxCleanliness;
        cleanlinessPercent = Mathf.Clamp01(cleanlinessPercent);

        if (cleanlinessPercent >= 0.90f)
        {
            for (int i = 0; i < debrisObjects.Count; i++)
                if (debrisObjects[i] != null)
                    debrisObjects[i].SetActive(false);
            return;
        }

        int activeCount = Mathf.RoundToInt((1f - cleanlinessPercent) * debrisObjects.Count);

        for (int i = 0; i < debrisObjects.Count; i++)
        {
            if (debrisObjects[i] == null) continue;

            bool shouldBeActive = i < activeCount;
            debrisObjects[i].SetActive(shouldBeActive);
        }
    }
}