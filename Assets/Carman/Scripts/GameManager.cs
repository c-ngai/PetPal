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
    public Room currentRoom;
    public Building currentBuilding;
    public GameObject currentPurchasedPet;

    public Dictionary<string, GameObject> roomPetPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist between scenes

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
                RoomManager.Instance.LoadPets();
                break;
            case GameState.RoomSelected:
                currentRoom.SpawnPet();
                break;
            case GameState.PetPurchasing:
                currentList = ShopManager.Instance;
                break;
            default:
                currentList = null;
                break;
        }

        Debug.Log("SelectionList set to: " + currentList);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState != newState)
        {
            PreviousState.Push(CurrentState);
        }

        CurrentState = newState;

        Debug.Log("Game State changed to: " + newState);

        LoadSceneForState(newState);
    }

    public bool IsActionMode()
    {
        return CurrentState == GameState.PlayMode ||
               CurrentState == GameState.FeedMode ||
               CurrentState == GameState.CleanMode;
    }

    public void GoBack()
    {
        if (PreviousState.Count > 0)
        {
            GameState prev = PreviousState.Pop();
            Debug.Log("Going back to: " + prev);

            CurrentState = prev;

            LoadSceneForState(prev);
        }
        else
        {
            Debug.Log("No previous state.");
        }
    }

    void LoadSceneForState(GameState state)
    {
        switch (state)
        {
            case GameState.BuildingSelection:
                SceneManager.LoadScene("BuildingScene");
                break;

            case GameState.RoomSelection:
                SceneManager.LoadScene(currentBuilding.name);
                break;

            case GameState.RoomSelected:
            case GameState.FeedMode:
            case GameState.CleanMode:
                SceneManager.LoadScene(currentRoom.name);
                break;

            case GameState.PetPurchasing:
                SceneManager.LoadScene("ShopScene");
                break;

            case GameState.PlayMode:
                SceneManager.LoadScene("PlayScene");
                currentList = null;
                break;
        }
    }

}