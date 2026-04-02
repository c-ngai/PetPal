using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public enum BuildingType
    {
        Residential,
        Shop
    }

    public BuildingType buildingType;
    public List<Room> rooms = new List<Room>();
}