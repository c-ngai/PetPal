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

    public bool IsOccupied()
    {
        return currentPetInstance != null;
    }

    public void PlacePet(GameObject petPrefab)
    {
        if (IsOccupied()) return;

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

        pet.SetStage(Pet.PetStage.Pet); // Hatch when placed

        // Create PetData and save it
        PetData data = new PetData();
        data.petPrefab = petPrefab;
        data.hunger = stats.hunger;
        data.cleanliness = stats.cleanliness;
        data.love = stats.love;
        data.stage = Pet.PetStage.Pet;

        GameManager.Instance.roomPets[RoomID] = data;
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

        // Load saved data
        pet.SetStage(data.stage);
        stats.hunger = data.hunger;
        stats.cleanliness = data.cleanliness;
        stats.love = data.love;

        controller.Initialize(InputManager.Instance);
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