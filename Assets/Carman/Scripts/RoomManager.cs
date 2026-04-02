using System.Collections.Generic;
using UnityEngine;

public class RoomManager : SelectionList
{
    public static RoomManager Instance;

    [Header("Assign all rooms in scene")]
    [SerializeField] private List<Room> roomsInScene;

    [SerializeField] private Highlightable backButton;


    void Awake()
    {
        Instance = this;

        items.Clear();
        foreach (var b in roomsInScene)
        {
            var highlightable = b.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }
        items.Add(backButton);

        currentIndex = 0;
    }

    public void PutPet(GameObject pet)
    {
        GameManager.Instance.currentRoom.PlacePet(pet);
    }

    protected override void OnItemSelected(int index)
    {
        // If Back button selected
        if (index == roomsInScene.Count)
        {
            GameManager.Instance.GoBack();
            return;
        }

        // Otherwise it's a room
        var room = roomsInScene[index];
        GameManager.Instance.currentRoom = room;

        if (GameManager.Instance.IsPlacingPet)
        {
            PutPet(GameManager.Instance.currentPurchasedPet);
            GameManager.Instance.IsPlacingPet = false;
        }

        GameManager.Instance.SetState(GameManager.GameState.RoomSelected);
    }

    public void LoadPets()
    {
        foreach (var room in roomsInScene)
        {
            room.SpawnPet();
        }
    }
}