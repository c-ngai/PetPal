using UnityEngine;

public class ShopManager : SelectionList
{
    public static ShopManager Instance;

    void Awake()
    {
        Instance = this;
    }


    protected override void OnItemSelected(int index)
    {
        Debug.Log("Room selected: " + index);
        BuyPet(index);
        GameManager.Instance.SetState(GameManager.GameState.PetPlacement);
    }

    public void BuyPet(int index)
    {
    }
}
