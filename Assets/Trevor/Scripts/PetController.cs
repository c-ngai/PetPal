using System.Collections;
using UnityEngine;

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

    private Vector3 homePosition;
    private bool isFeedInputActive = false;

    void Start()
    {
        stats = GetComponent<PetStats>();
        homePosition = transform.position;
    }

    public void Initialize(InputManager manager)
    {
        inputManager = manager;

        if (inputManager != null)
        {
            inputManager.OnPlayAction += HandlePlay;
            inputManager.OnCleanAction += HandleClean;
            inputManager.OnFeedAction += HandleFeedState;
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnPlayAction -= HandlePlay;
            inputManager.OnCleanAction -= HandleClean;
            inputManager.OnFeedAction -= HandleFeedState;
        }
    }

    void Update()
    {
        // FIX: Gatekeeper to prevent ghost inputs filling stats on load
        if (GameManager.Instance.CurrentState != GameManager.GameState.FeedMode)
        {
            isFeedInputActive = false;
        }

        if (currentState == PetState.Idle && isFeedInputActive)
        {
            StartCoroutine(FeedRoutine());
        }
    }

    private void HandlePlay()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.PlayMode) return;
        if (currentState == PetState.Idle) StartCoroutine(PlayRoutine());
    }

    private void HandleClean()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.CleanMode) return;
        if (currentState == PetState.Idle) StartCoroutine(CleanRoutine());
    }

    private void HandleFeedState(bool isActive)
    {
        isFeedInputActive = isActive;
    }

    private IEnumerator PlayRoutine()
    {
        currentState = PetState.Playing;

        // FIX: Playing boosts Love
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

    private IEnumerator CleanRoutine()
    {
        currentState = PetState.Cleaning;
        stats.BoostCleanliness(25f);

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = Random.insideUnitSphere * shakeAmount;
            transform.position = new Vector3(homePosition.x + offset.x, homePosition.y, homePosition.z + offset.z);
            yield return null;
        }

        transform.position = homePosition;
        currentState = PetState.Idle;
    }

    private IEnumerator FeedRoutine()
    {
        currentState = PetState.Feeding;

        // FIX: Feeding boosts Hunger
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