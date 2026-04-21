using System.Collections.Generic;
using UnityEngine;

public class ShopManager : SelectionList
{
    public static ShopManager Instance;

    [Header("Scene objects (eggs shown in shop)")]
    [SerializeField] private List<GameObject> shopDisplayPets;

    [Header("Actual pet prefabs (assets)")]
    [SerializeField] private List<GameObject> petPrefabs;

    [SerializeField] private Highlightable backButton;

    void Awake()
    {
        Instance = this;

        items.Clear();
        foreach (var pet in shopDisplayPets)
        {
            var highlightable = pet.GetComponent<Highlightable>();
            if (highlightable != null)
                items.Add(highlightable);
        }
        items.Add(backButton);

        currentIndex = 0;
    }

    void Start()
    {
        foreach (var petObj in shopDisplayPets)
        {
            Pet pet = petObj.GetComponent<Pet>();
            if (pet != null)
            {
                pet.SetStage(Pet.PetStage.Egg);
            }
        }

        UpdateHighlight();
    }

    protected override void OnItemSelected(int index)
    {
        if (index == shopDisplayPets.Count)
        {
            GameManager.Instance.GoBack();
        }
        else
        {
            bool success = BuyPet(index);

            if (!success) return;

            GameManager.Instance.IsPlacingPet = true;
            GameManager.Instance.SetState(GameManager.GameState.BuildingSelection);
        }
    }


    public bool BuyPet(int index)
    {
        int price = petPrefabs[index].GetComponent<Pet>().Price;

        if (!CurrencyManager.Instance.HasEnoughCurrency(price))
        {
            Debug.Log("Not enough currency!");
            return false;
        }

        CurrencyManager.Instance.RemoveCurrency(price);
        GameManager.Instance.currentPurchasedPetPrefab = petPrefabs[index];
        return true;
    }
}