using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : SelectionList
{
    public static GamePlayManager Instance;

    [Header("Assign all buttons in scene")]
    [SerializeField] private List<GameObject> buttons;

    void Awake()
    {
        Instance = this;

        items.Clear();
        foreach (var b in buttons)
        {
            var highlightable = b.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }
        currentIndex = 0;
    }

    protected override void OnItemSelected(int index)
    {
        GameObject selectedButton = buttons[index];

        Room room = FindFirstObjectByType<Room>();

        bool hasPet = room != null && room.IsOccupied();


        switch (selectedButton.name)
        {
            case "Back":
                GameManager.Instance.GoBack();
                break;

            case "Play":
                if (!hasPet) return;

                // Find the pet in the room to grab its data before switching scenes
                Pet activePet = FindFirstObjectByType<Pet>();
                if (activePet != null)
                {
                    // Pass the Sprite to the GameManager
                    SpriteRenderer petSprite = activePet.GetComponentInChildren<SpriteRenderer>();
                    if (petSprite != null)
                    {
                        GameManager.Instance.activeMinigameSprite = petSprite.sprite;
                    }

                    // Pass the Room ID to the GameManager so stats update correctly later
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