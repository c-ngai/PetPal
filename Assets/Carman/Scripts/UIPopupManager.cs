using System.Collections;
using TMPro;
using UnityEngine;

public class UIPopupManager : MonoBehaviour
{
    public static UIPopupManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text popupText;

    [Header("Settings")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine activeRoutine;

    // persistent state
    private string persistentMessage;
    private bool hasPersistent;

    // KEY FIX: track whether persistent is currently allowed in this scene/context
    private bool persistentActiveInContext;

    private bool isTemporaryActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (popupRoot != null)
            popupRoot.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        PetStats.OnPetWarning += HandlePetWarning;
    }

    void OnDisable()
    {
        PetStats.OnPetWarning -= HandlePetWarning;
    }

    private void HandlePetWarning(string roomID, string message)
    {
        ShowTemporaryTimed(message);
    }

    // -------------------------
    // CONTEXT CONTROL (IMPORTANT)
    // -------------------------

    public void SetContextActive(bool active)
    {
        persistentActiveInContext = active;

        if (!active)
        {
            hasPersistent = false;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
                isTemporaryActive = false;
            }

            Hide();
        }
        else if (hasPersistent && !isTemporaryActive)
        {
            ShowInternal(persistentMessage);
        }
    }

    // -------------------------
    // PERSISTENT
    // -------------------------
    public void ShowPersistent(string message)
    {
        persistentMessage = message;
        hasPersistent = true;

        if (persistentActiveInContext && !isTemporaryActive)
        {
            ShowInternal(message);
        }
    }

    public void HidePersistent()
    {
        hasPersistent = false;

        if (!isTemporaryActive)
            Hide();
    }

    // -------------------------
    // TEMPORARY
    // -------------------------
    public void ShowTemporaryTimed(string message)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(TemporaryRoutine(message));
    }

    private IEnumerator TemporaryRoutine(string message)
    {
        isTemporaryActive = true;

        ShowInternal(message);

        yield return new WaitForSeconds(displayTime);

        isTemporaryActive = false;

        // ONLY restore if persistent is valid AND context still active
        if (hasPersistent && persistentActiveInContext)
            ShowInternal(persistentMessage);
        else
            Hide();
    }

    // -------------------------
    // CORE
    // -------------------------
    private void ShowInternal(string message)
    {
        if (popupRoot == null) return;

        popupRoot.SetActive(true);
        popupText.text = message;
    }

    public void Hide()
    {
        if (popupRoot == null) return;

        popupRoot.SetActive(false);
    }

    public void ResetUIState()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        hasPersistent = false;
        persistentMessage = null;
        persistentActiveInContext = false;
        isTemporaryActive = false;

        Hide();
    }
}