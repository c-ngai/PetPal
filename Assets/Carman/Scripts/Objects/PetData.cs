using UnityEngine;
using System; // Required for DateTime

[System.Serializable]
public class PetData
{
    public GameObject petPrefab;
    public float hunger;
    public float cleanliness;
    public float love;
    public Pet.PetStage stage;

    // Add this to track the exact moment the pet was last active
    public long lastSavedTime;
}