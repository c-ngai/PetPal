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
    public float shakeAmount = 0.2f;
    public float shakeDuration = 0.5f;
    public float walkSpeed = 2f;
    public float approachDistance = 2f;

    [Header("Cleaning Settings")]
    public float cleanDuration = 3f;
    private float cleanProgress = 0f;

    private bool isCleaningInputActive = false;
    private float cleanHoldTimer = 0f;
    private const float maxHoldDuration = 3f;

    [SerializeField] private Transform debrisRoot;
    private List<GameObject> debrisObjects = new List<GameObject>();

    private Vector3 homePosition;
    private bool isFeedInputActive = false;

    void Start()
    {
        stats = GetComponent<PetStats>();
        homePosition = transform.position;

        if (debrisRoot != null)
        {
            foreach (Transform child in debrisRoot)
                debrisObjects.Add(child.gameObject);
        }
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

        HandleFeedMode();
        HandleCleanModeTransitions();
    }

    // ================= INPUT =================

    private void HandlePlay()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.PlayMode) return;

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

        if (currentState != PetState.Idle && currentState != PetState.Cleaning)
            return;

        isCleaningInputActive = true;
    }

    private void HandleFeedState(bool isActive)
    {
        isFeedInputActive = isActive;
    }

    // ================= UPDATE MODES =================

    private void HandleFeedMode()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.FeedMode)
        {
            if (currentState == PetState.Idle && isFeedInputActive)
            {
                StartCoroutine(FeedRoutine());
            }
        }
        else
        {
            isFeedInputActive = false;
        }
    }

    private void HandleCleanModeTransitions()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.CleanMode)
        {
            // HARD EXIT CLEAN MODE RESET (fixes your bug)
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

    // ================= CLEAN =================

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

        GameManager.Instance.SetState(GameManager.GameState.RoomSelected);
    }

    private void ResetCleaning()
    {
        cleanProgress = 0f;
        cleanHoldTimer = 0f;
        isCleaningInputActive = false;

        if (currentState == PetState.Cleaning)
            currentState = PetState.Idle;
    }

    // ================= DEBRIS =================

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

    // ================= FEED =================

    private IEnumerator FeedRoutine()
    {
        currentState = PetState.Feeding;

        stats.BoostHunger(30f);

        Vector3 cameraPos = Camera.main != null ? Camera.main.transform.position : homePosition;
        Vector3 dir = (cameraPos - homePosition).normalized;

        Vector3 targetPos = homePosition + dir * approachDistance;
        targetPos.y = homePosition.y;

        while (isFeedInputActive)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(transform.position, homePosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, homePosition, walkSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = homePosition;
        currentState = PetState.Idle;
    }
}