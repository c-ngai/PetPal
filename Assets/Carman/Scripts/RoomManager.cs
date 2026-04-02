using UnityEngine;

public class RoomManager : SelectionList
{
    public static RoomManager Instance;

    private Building currentBuilding;

    void Awake()
    {
        Instance = this;
    }

    public void LoadRooms(Building building)
    {
        currentBuilding = building;

        items.Clear();

        foreach (var room in building.rooms)
        {
            var highlightable = room.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }

        // Switch game scene to single building view with rooms

        // Optional: reset index to first room
        if (items.Count > 0)
        {
            GetCurrentIndex(); // or call UpdateHighlight() from SelectionList
        }
    }


    public void PutPet(GameObject pet)
    {
        GameManager.Instance.currentRoom.PlacePet(pet);
    }

    protected override void OnItemSelected(int index)
    {
        var room = currentBuilding.rooms[index];
        Debug.Log("Room selected: " + room.name);

        // Store selected room in GameManager for pet placement
        GameManager.Instance.currentRoom = room;

        GameManager.Instance.SetState(GameManager.GameState.PlayMode);
    }
}