using UnityEngine;

public class Building : MonoBehaviour
{
    public string BuildingID; // Unique ID for persistence
    public enum BuildingType
    {
        Residential,
        Shop
    }

    public BuildingType buildingType;
}