using UnityEngine;

public class Room : MonoBehaviour
{
    public string RoomID;
    public string BuildingID;
    public Transform petPosition;

    private GameObject currentPetInstance;

    [Header("Scaling")]
    [SerializeField] private Vector3 selectionScale = Vector3.one;
    [SerializeField] private Vector3 roomViewScale = Vector3.one * 2f;

    // FIX 1: Check the persistent data, not just the local visual instance
    public bool IsOccupied()
    {
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.roomPets.ContainsKey(RoomID);
    }

    public void PlacePet(GameObject petPrefab)
    {
        // We removed the IsOccupied check here because RoomManager already does it before calling this

        currentPetInstance = Instantiate(petPrefab, petPosition.position, Quaternion.identity);
        currentPetInstance.transform.SetParent(petPosition);

        if (GameManager.Instance.CurrentState == GameManager.GameState.RoomSelection)
        {
            currentPetInstance.transform.localScale = selectionScale;
        }
        else
        {
            currentPetInstance.transform.localScale = roomViewScale;
        }

        Pet pet = currentPetInstance.GetComponent<Pet>();
        PetStats stats = currentPetInstance.GetComponent<PetStats>();
        PetController controller = currentPetInstance.GetComponent<PetController>();

        pet.SetStage(Pet.PetStage.Pet); // Hatch when placed

        // FIX 2: We removed the PetData creation here. RoomManager handles the data and timestamp now.
        // We just need to tell the stats script to wake up and bind to this room.
        if (stats != null)
        {
            stats.Initialize(RoomID);
        }

        if (controller != null && InputManager.Instance != null)
        {
            controller.Initialize(InputManager.Instance);
        }
    }

    public void SpawnPet()
    {
        if (currentPetInstance != null) return;
        if (!GameManager.Instance.roomPets.ContainsKey(RoomID)) return;

        PetData data = GameManager.Instance.roomPets[RoomID];

        currentPetInstance = Instantiate(data.petPrefab, petPosition.position, Quaternion.identity);
        currentPetInstance.transform.SetParent(petPosition);

        if (GameManager.Instance.CurrentState == GameManager.GameState.RoomSelection)
        {
            currentPetInstance.transform.localScale = selectionScale;
        }
        else
        {
            currentPetInstance.transform.localScale = roomViewScale;
        }

        Pet pet = currentPetInstance.GetComponent<Pet>();
        PetStats stats = currentPetInstance.GetComponent<PetStats>();
        PetController controller = currentPetInstance.GetComponent<PetController>();

        pet.SetStage(data.stage);

        // FIX 3: Instead of manually setting stats.hunger = data.hunger, we trigger Initialize.
        // The PetStats script will load the data AND calculate the missed time automatically.
        if (stats != null)
        {
            stats.Initialize(RoomID);
        }

        if (controller != null && InputManager.Instance != null)
        {
            controller.Initialize(InputManager.Instance);
        }
    }

    void OnDestroy()
    {
        RemovePetOnSceneChange();
    }

    public void RemovePetOnSceneChange()
    {
        if (currentPetInstance != null)
            Destroy(currentPetInstance);
    }
}