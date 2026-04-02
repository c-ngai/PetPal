using UnityEngine;

public class Room : MonoBehaviour
{
    public string roomID; // unique per room
    public Transform petPosition;

    private GameObject currentPetInstance;

    public bool IsOccupied()
    {
        return GameManager.Instance.roomPetPrefabs.ContainsKey(roomID);
    }

    public void PlacePet(GameObject petPrefab)
    {
        if (IsOccupied())
        {
            Debug.Log("Room already occupied!");
            return;
        }

        GameManager.Instance.roomPetPrefabs[roomID] = petPrefab;
        SpawnPet();
    }

    public void SpawnPet()
    {
        if (!IsOccupied()) return;

        GameObject petPrefab = GameManager.Instance.roomPetPrefabs[roomID];

        currentPetInstance = Instantiate(petPrefab, petPosition.position, Quaternion.identity);
        currentPetInstance.transform.SetParent(petPosition);
    }

    public void RemovePet()
    {
        // Destroy the spawned pet
        if (currentPetInstance != null)
        {
            Destroy(currentPetInstance);
        }

        // Remove from persistent data
        if (GameManager.Instance.roomPetPrefabs.ContainsKey(roomID))
        {
            GameManager.Instance.roomPetPrefabs.Remove(roomID);
        }
    }
}