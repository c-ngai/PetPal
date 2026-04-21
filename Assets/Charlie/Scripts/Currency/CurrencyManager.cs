using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{

    public static CurrencyManager Instance;

    public int currency = 5;
    public TextMeshProUGUI tmp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ChangeCurrencyUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasEnoughCurrency(int amount)
    {
        return currency >= amount;
    }

    public void AddCurrency(int i)
    {
        currency += i;
        ChangeCurrencyUI();
    }

    public void RemoveCurrency(int i)
    {
        currency -= i; 
        ChangeCurrencyUI();
    }

    void ChangeCurrencyUI()
    {
        tmp.text = $"{currency}";
    }
}
