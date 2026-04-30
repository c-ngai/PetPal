using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        CleanMode,
        GameOver
    }

    public GameState CurrentState;
    public Stack<GameState> PreviousState;
    public bool IsPlacingPet;

    public bool justPlacedNewPet = false;

    public SelectionList currentList;

    public string currentRoomID;
    public string currentBuildingID;

    public GameObject currentPurchasedPetPrefab;
    public bool gameOverRestartStarted;

    [Header("Minigame Handoff Data")]
    public string activeMinigameRoomID;
    public Sprite activeMinigameSprite;

    public Dictionary<string, PetData> roomPets = new Dictionary<string, PetData>();

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

        PetStats.OnAnyPetStatDepleted += HandlePetDeath;

        SetState(GameState.BuildingSelection);
    }

    void OnDestroy()
    {
        PetStats.OnAnyPetStatDepleted -= HandlePetDeath;
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
                CheckAllPetsForGameOver();
                break;
            case GameState.RoomSelected:
                currentList = GamePlayManager.Instance;
                Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
                foreach (Room room in rooms)
                {
                    if (room.RoomID == currentRoomID)
                    {
                        // FIX: Check if we just placed this pet to trigger the Hatch coroutine
                        if (justPlacedNewPet)
                        {
                            room.PlacePet(roomPets[currentRoomID].petPrefab);
                            justPlacedNewPet = false; // Reset the flag immediately
                        }
                        else
                        {
                            // Otherwise, just spawn it instantly as an adult
                            room.SpawnPet();
                        }
                        break;
                    }
                }
                break;

            case GameState.PetPurchasing:
                currentList = ShopManager.Instance;
                break;

            default:
                currentList = null;
                break;
        }
    }

    private void HandlePetDeath(string roomID)
    {
        if (CurrentState == GameState.GameOver) return;
        SetState(GameState.GameOver);
    }

    private void CheckAllPetsForGameOver()
    {
        foreach (var kvp in roomPets)
        {
            var data = kvp.Value;

            if (data.hunger <= 0f || data.cleanliness <= 0f || data.love <= 0f)
            {
                SetState(GameState.GameOver);
                return;
            }
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        if (IsNavigationState(CurrentState) && IsNavigationState(newState))
        {
            PreviousState.Push(CurrentState);
        }

        CurrentState = newState;
        LoadSceneForState(newState);
    }

    public void GoBack()
    {
        while (PreviousState.Count > 0)
        {
            GameState prev = PreviousState.Pop();

            if (IsNavigationState(prev))
            {
                CurrentState = prev;
                LoadSceneForState(prev);
                return;
            }
        }

        Debug.LogWarning("Back stack empty, nothing to return to");
    }

    private bool IsNavigationState(GameState state)
    {
        return state == GameState.BuildingSelection ||
               state == GameState.RoomSelection ||
               state == GameState.RoomSelected ||
               state == GameState.PetPurchasing;
    }

    public bool IsActionMode()
    {
        return CurrentState == GameState.PlayMode ||
               CurrentState == GameState.FeedMode ||
               CurrentState == GameState.CleanMode;
    }

    public bool IsPlayMode()
    {
        return CurrentState == GameState.PlayMode;
    }

    void LoadSceneForState(GameState state)
    {
        switch (state)
        {
            case GameState.BuildingSelection:
                SceneManager.LoadScene("BuildingScene");
                SoundManager.Instance?.PlayMainBGM();
                break;

            case GameState.RoomSelection:
                SceneManager.LoadScene(currentBuildingID);
                SoundManager.Instance?.PlayMainBGM();
                break;

            case GameState.RoomSelected:
                SceneManager.LoadScene(currentRoomID);
                SoundManager.Instance?.PlayMainBGM();
                break;

            case GameState.FeedMode:
            case GameState.CleanMode:
                // stay in the same scene, just change mode
                break;

            case GameState.PetPurchasing:
                SceneManager.LoadScene("ShopBuilding");
                SoundManager.Instance?.PlayMainBGM();
                break;

            case GameState.PlayMode:
                SceneManager.LoadScene("PlayScene");
                currentList = null;
                // Swap to the minigame music!
                SoundManager.Instance?.PlayMinigameBGM();
                break;

            case GameState.GameOver:
                SceneManager.LoadScene("GameOverScene");
                SoundManager.Instance?.PlayMainBGM();
                gameOverRestartStarted = false;
                break;
        }
    }

    public void RestartGameAfterDelay(float delay)
    {
        StartCoroutine(RestartRoutine(delay));
    }

    private IEnumerator RestartRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // reset runtime data
        PreviousState.Clear();
        roomPets.Clear();
        currentPurchasedPetPrefab = null;
        justPlacedNewPet = false;
        CurrencyManager.Instance.currency = 5;
        CurrencyManager.Instance.ChangeCurrencyUI();

        CurrentState = GameState.BuildingSelection;

        SceneManager.LoadScene("BuildingScene");

        SoundManager.Instance?.PlayMainBGM();
    }
}