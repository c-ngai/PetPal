using System.Collections.Generic;
using UnityEngine;

public class ShopManager : SelectionList
{
    public static ShopManager Instance;

    [Header("Assign all pets in scene")]
    [SerializeField] private List<GameObject> pets;

    void Awake()
    {
        Instance = this;
    }


    protected override void OnItemSelected(int index)
    {
        Debug.Log("Room selected: " + index);
        BuyPet(index);
        GameManager.Instance.IsPlacingPet = true;
        GameManager.Instance.SetState(GameManager.GameState.BuildingSelection);
    }

    public void BuyPet(int index)
    {
        GameManager.Instance.currentPurchasedPet = pets[index];
    }
}
