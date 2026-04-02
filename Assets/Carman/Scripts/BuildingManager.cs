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
        Building selectedBuilding = buildingsInScene[index];

        // Store only the ID in GameManager
        GameManager.Instance.currentBuildingID = selectedBuilding.BuildingID;

        // Determine next state
        if (selectedBuilding.buildingType == Building.BuildingType.Residential)
        {
            GameManager.Instance.SetState(GameManager.GameState.RoomSelection);
        }
        else
        {
            GameManager.Instance.SetState(GameManager.GameState.PetPurchasing);
        }

        Debug.Log("Building selected: " + selectedBuilding.name + " (ID: " + selectedBuilding.BuildingID + ")");
    }
}