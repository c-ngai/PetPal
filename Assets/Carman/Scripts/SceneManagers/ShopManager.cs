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

    [Header("UI Indicators")]
    public GameObject openHandPrefab;
    public Vector3 handOffset = new Vector3(0, 1.5f, 0);
    private string lastPopupMessage;

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

    void OnEnable()
    {
        UIPopupManager.Instance.SetContextActive(true);
    }

    void OnDisable()
    {
        UIPopupManager.Instance.SetContextActive(false);
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

            // Spawn the open hand above each display egg
            if (openHandPrefab != null)
            {
                GameObject hand = Instantiate(openHandPrefab, petObj.transform.position + handOffset, Quaternion.identity);
                // Make it a child of the egg so it moves/scales with it if needed
                hand.transform.SetParent(petObj.transform);
            }
        }

        UpdateHighlight();
    }

    public override void UpdateHighlight()
    {
        base.UpdateHighlight();
        UpdatePricePopup();
    }

    protected override void OnItemSelected(int index)
    {
        UIPopupManager.Instance.Hide();
        lastPopupMessage = null;

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
            SoundManager.Instance.PlayError();
            UIPopupManager.Instance.ShowPersistent("Not enough currency!");
            return false;
        }

        CurrencyManager.Instance.RemoveCurrency(price);
        GameManager.Instance.currentPurchasedPetPrefab = petPrefabs[index];
        return true;
    }

    void UpdatePricePopup()
    {
        int index = GetCurrentIndex();

        if (index >= shopDisplayPets.Count)
        {
            UIPopupManager.Instance.Hide();
            lastPopupMessage = null;
            return;
        }

        Pet pet = petPrefabs[index].GetComponent<Pet>();

        if (pet == null)
        {
            UIPopupManager.Instance.Hide();
            lastPopupMessage = null;
            return;
        }

        string msg = $"Cost: {pet.Price}";

        if (msg == lastPopupMessage) return;

        lastPopupMessage = msg;
        UIPopupManager.Instance.ShowPersistent(msg);
    }
}