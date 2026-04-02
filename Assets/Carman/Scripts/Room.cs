using UnityEngine;

public class Room : MonoBehaviour
{
    public string RoomID;      // unique per room
    public string BuildingID;  // which building this belongs to
    public Transform petPosition;

    private GameObject currentPetInstance;

    public bool IsOccupied()
    {
        return currentPetInstance != null;
    }

    public void PlacePet(GameObject petPrefab)
    {
        if (IsOccupied()) return;

        currentPetInstance = Instantiate(petPrefab, petPosition.position, Quaternion.identity);
        currentPetInstance.transform.SetParent(petPosition);

        // Persist to GameManager
        GameManager.Instance.roomPetPrefabs[RoomID] = petPrefab;
    }

    public void SpawnPet(GameObject petPrefab)
    {
        if (petPrefab == null) return;

        currentPetInstance = Instantiate(petPrefab, petPosition.position, Quaternion.identity);
        currentPetInstance.transform.SetParent(petPosition);
    }

    public void RemovePet()
    {
        if (currentPetInstance != null)
            Destroy(currentPetInstance);

        if (GameManager.Instance.roomPetPrefabs.ContainsKey(RoomID))
            GameManager.Instance.roomPetPrefabs.Remove(RoomID);
    }
}