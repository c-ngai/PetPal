using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : SelectionList
{
    public static GamePlayManager Instance;

    [Header("Assign all buttons in scene")]
    [SerializeField] private List<GameObject> buttons;

    private bool lastHasPet;

    void Awake()
    {
        Instance = this;
        currentIndex = 0;
    }

    void Start()
    {
        UpdateButtonVisibility();
    }

    void Update()
    {
        Room room = FindFirstObjectByType<Room>();
        bool hasPet = room != null && room.IsOccupied();

        // Only update when state actually changes
        if (hasPet != lastHasPet)
        {
            lastHasPet = hasPet;
            UpdateButtonVisibility();
        }
    }

    void UpdateButtonVisibility()
    {
        Room room = FindFirstObjectByType<Room>();
        bool hasPet = room != null && room.IsOccupied();

        foreach (var button in buttons)
        {
            if (button.name == "Back")
                button.SetActive(true);
            else
                button.SetActive(hasPet);
        }

        RefreshSelectableItems();
    }

    void RefreshSelectableItems()
    {
        items.Clear();

        foreach (var b in buttons)
        {
            if (!b.activeSelf) continue;

            var highlightable = b.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }

        if (items.Count == 0) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, items.Count - 1);
        UpdateHighlight();
    }

    protected override void OnItemSelected(int index)
    {
        if (items.Count == 0) return;

        GameObject selectedButton = items[index].gameObject;

        Room room = FindFirstObjectByType<Room>();
        bool hasPet = room != null && room.IsOccupied();

        switch (selectedButton.name)
        {
            case "Back":
                GameManager.Instance.GoBack();
                break;

            case "Play":
                if (!hasPet) return;

                Pet activePet = FindFirstObjectByType<Pet>();
                if (activePet != null)
                {
                    SpriteRenderer petSprite = activePet.GetComponentInChildren<SpriteRenderer>();
                    if (petSprite != null)
                    {
                        GameManager.Instance.activeMinigameSprite = petSprite.sprite;
                    }

                    if (room != null)
                    {
                        GameManager.Instance.activeMinigameRoomID = room.RoomID;
                    }
                }

                GameManager.Instance.SetState(GameManager.GameState.PlayMode);
                break;

            case "Feed":
                if (!hasPet) return;
                GameManager.Instance.SetState(GameManager.GameState.FeedMode);
                break;

            case "Clean":
                if (!hasPet) return;
                GameManager.Instance.SetState(GameManager.GameState.CleanMode);
                break;

            default:
                Debug.LogWarning("Unhandled button: " + selectedButton.name);
                break;
        }
    }
}