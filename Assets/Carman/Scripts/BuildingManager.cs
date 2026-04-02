using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : SelectionList
{
    public static BuildingManager Instance;

    [Header("Assign all buildings in scene")]
    [SerializeField] private List<Building> buildingsInScene;

    void Awake()
    {
        Instance = this;

        // Convert buildings to Highlightable list
        items.Clear();
        foreach (var b in buildingsInScene)
        {
            var highlightable = b.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }

        currentIndex = 0;
    }


    protected override void OnItemSelected(int index)
    {
        var building = buildingsInScene[index];

        GameManager.Instance.currentBuilding = building;

        if (building.buildingType == Building.BuildingType.Residential)
        {
            GameManager.Instance.SetState(GameManager.GameState.RoomSelection);
        }
        else
        {
            GameManager.Instance.SetState(GameManager.GameState.PetPurchasing);
        }

        Debug.Log("Building selected: " + building.name);
    }
}