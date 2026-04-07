using Unity.VisualScripting;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public InputManager inputManager;
    public static SelectionManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist between scenes
    }

    void OnEnable()
    {
        inputManager.OnLeftSelect += MoveLeft;
        inputManager.OnRightSelect += MoveRight;
        inputManager.OnConfirmSelect += Confirm;
    }

    void OnDisable()
    {
        inputManager.OnLeftSelect -= MoveLeft;
        inputManager.OnRightSelect -= MoveRight;
        inputManager.OnConfirmSelect -= Confirm;
    }

    void MoveLeft() => GameManager.Instance.currentList?.MoveLeft();
    void MoveRight() => GameManager.Instance.currentList?.MoveRight();
    void Confirm() => GameManager.Instance.currentList?.Confirm();
}