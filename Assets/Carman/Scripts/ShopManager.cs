using System.Collections.Generic;
using UnityEngine;

public class ShopManager : SelectionList
{
    public static ShopManager Instance;

    [Header("Assign all pets in scene")]
    [SerializeField] private List<GameObject> pets;

    [SerializeField] private Highlightable backButton;


    void Awake()
    {
        Instance = this;

        // Populate the SelectionList items from rooms in the scene
        items.Clear();
        foreach (var pet in pets)
        {
            var highlightable = pet.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }
        items.Add(backButton);

        currentIndex = 0;
    }


    protected override void OnItemSelected(int index)
    {
        if (index == pets.Count)
        {
            GameManager.Instance.GoBack();
        }
        else
        {
            Debug.Log("Room selected: " + index);
            BuyPet(index);
            GameManager.Instance.IsPlacingPet = true;
            GameManager.Instance.SetState(GameManager.GameState.BuildingSelection);
        }
    }

    public void BuyPet(int index)
    {
        //GameManager.Instance.currentPurchasedPet = pets[index];
    }
}
