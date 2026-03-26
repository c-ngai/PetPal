using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Hardware Connections")]
    public ArduinoReader arduinoReader;
    public InputActionAsset inputAsset;

    public event Action OnPlayAction;
    public event Action OnCleanAction;
    public event Action<bool> OnFeedAction;

    private InputActionMap playerMap;
    private InputAction playInput;
    private InputAction cleanInput;
    private InputAction feedInput;

    private bool previousArduinoTilt = false;

    void Awake()
    {
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

        if (playInput != null) playInput.performed += ctx => { Debug.Log("PLAY triggered!"); OnPlayAction?.Invoke(); };
        if (cleanInput != null) cleanInput.performed += ctx => { Debug.Log("CLEAN triggered!"); OnCleanAction?.Invoke(); };
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
            OnPlayAction?.Invoke();
        }
        previousArduinoTilt = currentTilt;

        if (arduinoReader.soundTriggered)
        {
            OnCleanAction?.Invoke();
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
}