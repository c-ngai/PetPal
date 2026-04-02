using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [Header("Hardware Connections")]
    public ArduinoReader arduinoReader;
    public InputActionAsset inputAsset;

    public event Action OnPlayAction;
    public event Action OnCleanAction;
    public event Action<bool> OnFeedAction;

    public event Action OnLeftSelect;
    public event Action OnRightSelect;
    public event Action OnConfirmSelect;

    private InputActionMap playerMap;
    private InputAction playInput;
    private InputAction cleanInput;
    private InputAction feedInput;

    private bool previousArduinoTilt = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist between scenes

        Debug.Log("InputManager Awake() is running...");

        if (inputAsset == null)
        {
            Debug.LogError("ERROR: The Input Asset slot is empty in the Inspector!");
            return;
        }

        playerMap = inputAsset.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("ERROR: Could not find an Action Map named 'Player'!");
            return;
        }

        playInput = playerMap.FindAction("Play");
        cleanInput = playerMap.FindAction("Clean");
        feedInput = playerMap.FindAction("Feed");
    }

    void OnEnable()
    {
        if (playerMap != null)
        {
            playerMap.Enable(); // This is the secret sauce to make the keys wake up
            Debug.Log("🟢 Input Manager is ALIVE and listening for keys!");
        }

        if (playInput != null) playInput.performed += ctx => { HandlePlayPressed(); };
        if (cleanInput != null) cleanInput.performed += ctx => { HandleCleanPressed(); };
        if (feedInput != null) feedInput.performed += ctx => { HandleFeedPressed(); };
    }

    void OnDisable()
    {
        if (playerMap != null) playerMap.Disable();
    }

    void Update()
    {
        PollArduinoInputs();
        PollContinuousInputs();
    }

    private void PollArduinoInputs()
    {
        if (arduinoReader == null || !arduinoReader.enableArduino) return;

        bool currentTilt = arduinoReader.isTilted && arduinoReader.tiltBtnPressed;
        if (currentTilt && !previousArduinoTilt)
        {
            HandlePlayPressed();
        }
        previousArduinoTilt = currentTilt;

        if (arduinoReader.soundTriggered)
        {
            HandleCleanPressed();
            arduinoReader.ConsumeSoundTrigger();
        }
    }

    private void PollContinuousInputs()
    {
        bool kbFeeding = feedInput != null && feedInput.IsPressed();

        bool ardFeeding = arduinoReader != null &&
                          arduinoReader.enableArduino &&
                          arduinoReader.distPressed &&
                          arduinoReader.smoothedDistance > 0f &&
                          arduinoReader.smoothedDistance <= 10f;

        OnFeedAction?.Invoke(kbFeeding || ardFeeding);
    }

    private void HandlePlayPressed()
    {
        if (GameManager.Instance.IsActionMode())
        {
            Debug.Log("PLAY triggered!");
            OnPlayAction?.Invoke();
        }
        else
        {
            Debug.Log("LEFT selection");
            OnLeftSelect?.Invoke();
        }
    }

    private void HandleCleanPressed()
    {
        if (GameManager.Instance.IsActionMode())
        {
            Debug.Log("CLEAN triggered!");
            OnCleanAction?.Invoke();
        }
        else
        {
            Debug.Log("RIGHT selection");
            OnRightSelect?.Invoke();
        }
    }

    private void HandleFeedPressed()
    {
        if (!GameManager.Instance.IsActionMode())
        {
            Debug.Log("CONFIRM selection");
            OnConfirmSelect?.Invoke();
        }
    }
}