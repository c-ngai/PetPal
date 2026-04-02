using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        BuildingSelection,
        RoomSelection,
        RoomSelected,
        ActionMode,
        PetPurchasing,
        PlayMode,
        FeedMode,
        CleanMode
    }

    public GameState CurrentState;
    public Stack<GameState> PreviousState;
    public bool IsPlacingPet;

    public SelectionList currentList;

    // IDs instead of scene objects
    public string currentRoomID;
    public string currentBuildingID;

    public GameObject currentPurchasedPetPrefab;

    // Persistent mapping of roomID -> pet prefab
    public Dictionary<string, GameObject> roomPetPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PreviousState = new Stack<GameState>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        SetState(GameState.BuildingSelection);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (CurrentState)
        {
            case GameState.BuildingSelection:
                currentList = BuildingManager.Instance;
                break;
            case GameState.RoomSelection:
                currentList = RoomManager.Instance;
                break;
            case GameState.RoomSelected:
                currentList = GamePlayManager.Instance;
                break;

            case GameState.PetPurchasing:
                currentList = ShopManager.Instance;
                break;

            default:
                currentList = null;
                break;
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState != newState)
            PreviousState.Push(CurrentState);

        CurrentState = newState;
        Debug.Log("Game State changed to: " + newState);

        LoadSceneForState(newState);
    }

    public void GoBack()
    {
        if (PreviousState.Count > 0)
        {
            GameState prev = PreviousState.Pop();
            CurrentState = prev;
            LoadSceneForState(prev);
        }
    }

    public bool IsActionMode()
    {
        return CurrentState == GameState.PlayMode ||
               CurrentState == GameState.FeedMode ||
               CurrentState == GameState.CleanMode;
    }


    void LoadSceneForState(GameState state)
    {
        switch (state)
        {
            case GameState.BuildingSelection:
                SceneManager.LoadScene("BuildingScene");
                break;

            case GameState.RoomSelection:
                SceneManager.LoadScene(currentBuildingID);
                break;

            case GameState.RoomSelected:
            case GameState.FeedMode:
            case GameState.CleanMode:
                SceneManager.LoadScene(currentRoomID);
                break;

            case GameState.PetPurchasing:
                SceneManager.LoadScene("ShopBuilding");
                break;

            case GameState.PlayMode:
                SceneManager.LoadScene("PlayScene");
                currentList = null;
                break;
        }
    }
}