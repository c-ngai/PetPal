using UnityEngine;

[System.Serializable]
public class PetData
{
    public GameObject petPrefab;
    public float hunger;
    public float cleanliness;
    public float love;
    public Pet.PetStage stage;
}