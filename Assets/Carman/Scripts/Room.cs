using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject currentPet;

    public bool IsOccupied()
    {
        return currentPet != null;
    }

    public void PlacePet(GameObject pet)
    {
        currentPet = pet;
        pet.transform.position = transform.position;
    }

    public void RemovePet()
    {
        currentPet = null;
    }
}