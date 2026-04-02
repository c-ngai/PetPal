using UnityEngine;

public class Building : MonoBehaviour
{
    public enum BuildingType
    {
        Residential,
        Shop
    }

    public BuildingType buildingType;
}