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

    public bool PlacePetInRoom(Room room, GameObject petPrefab)
    {
        if (!room.IsOccupied())
        {
            // 1. Create the persistent data for the new pet
            PetData newPet = new PetData();
            newPet.petPrefab = petPrefab;
            newPet.hunger = 100f;
            newPet.cleanliness = 100f;
            newPet.love = 100f;

            // Save the permanent data as a hatched Pet, not an Egg
            newPet.stage = Pet.PetStage.Pet;

            // 2. Set the critical timestamp right now
            newPet.lastSavedTime = System.DateTime.UtcNow.Ticks;

            // 3. Save it to the GameManager dictionary using the roomID
            GameManager.Instance.roomPets[room.RoomID] = newPet;

            // FIX: Removed room.PlacePet(petPrefab) here. 
            // The scene switches instantly, so placing it here causes it to be destroyed mid-animation.
            // GameManager handles this in the new scene now.

            return true; // Success!
        }

        Debug.Log("Room is already occupied!");
        return false; // Failed to place
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

            if (GameManager.Instance.IsPlacingPet)
            {
                // Try to place the pet
                bool success = PlacePetInRoom(selectedRoom, GameManager.Instance.currentPurchasedPetPrefab);

                if (success)
                {
                    // Only turn off placement mode if they successfully moved in
                    GameManager.Instance.IsPlacingPet = false;
                    GameManager.Instance.currentPurchasedPetPrefab = null;

                    // FIX: Tell the GameManager to trigger the Hatch animation in the new scene!
                    GameManager.Instance.justPlacedNewPet = true;
                }
                else
                {
                    // Room was full. Stop here so they can pick another room.
                    return;
                }
            }

            // Store selected room in GameManager and switch states
            GameManager.Instance.currentRoomID = selectedRoom.RoomID;
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