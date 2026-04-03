using System.Collections.Generic;
using UnityEngine;

public class RoomManager : SelectionList
{
    public static RoomManager Instance;

    [Header("Assign all rooms in the scene for this building")]
    [SerializeField] private List<Room> roomsInScene;

    [SerializeField] private Highlightable backButton;

    void Awake()
    {
        Instance = this;

        // Populate the SelectionList items from rooms in the scene
        items.Clear();
        foreach (var room in roomsInScene)
        {
            var highlightable = room.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }
        items.Add(backButton);

        currentIndex = 0;
    }

    public void PlacePetInRoom(Room room, GameObject petPrefab)
    {
        if (!room.IsOccupied())
        {
            room.PlacePet(petPrefab);
        }
    }

    protected override void OnItemSelected(int index)
    {
        if (index < 0 || index > roomsInScene.Count) return;

        Debug.Log($"Selected index: {index}");
        if (index == roomsInScene.Count)
        {
            // Back button selected
            GameManager.Instance.SetState(GameManager.GameState.BuildingSelection);
            return;
        }
        else
        {
            Room selectedRoom = roomsInScene[index];

            // Store selected room in GameManager
            GameManager.Instance.currentRoomID = selectedRoom.RoomID;

            if (GameManager.Instance.IsPlacingPet)
            {
                PlacePetInRoom(selectedRoom, GameManager.Instance.currentPurchasedPetPrefab);
                GameManager.Instance.IsPlacingPet = false;
            }

            GameManager.Instance.SetState(GameManager.GameState.RoomSelected);
        }
    }

    public void LoadPets()
    {
        foreach (var room in roomsInScene)
        {
            room.SpawnPet();
        }
    }
}