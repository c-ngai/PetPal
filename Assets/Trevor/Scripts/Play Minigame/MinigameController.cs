using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    public InputManager inputManager;
    public MiniGamePet miniGamePet;

    private void Awake()
    {
        if (inputManager != null)
        {
            // Assuming your InputManager has an action for jumping or playing
            inputManager = InputManager.Instance;
            inputManager.OnPlayAction += HandleJumpInput;
        }
    }
    void Start()
    {
        if (inputManager != null)
        {
            // Assuming your InputManager has an action for jumping or playing
            //inputManager.OnPlayAction += HandleJumpInput;
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnPlayAction -= HandleJumpInput;
        }
    }

    // Alternative Update loop if you aren't using the event-based InputManager in this scene
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            HandleJumpInput();
        }
    }

    private void HandleJumpInput()
    {
        if (miniGamePet != null)
        {
            miniGamePet.Jump();
        }
    }
}