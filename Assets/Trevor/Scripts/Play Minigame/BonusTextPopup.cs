using UnityEngine;
using TMPro;

public class BonusTextPopup : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float lifeTime = 1f;

    private TextMeshPro textMesh;
    private TextMeshProUGUI uiText;
    private Color textColor;
    private float timer;

    void Awake()
    {
        // Check for either 3D Text or UI Text
        textMesh = GetComponent<TextMeshPro>();
        uiText = GetComponent<TextMeshProUGUI>();

        if (textMesh != null) textColor = textMesh.color;
        else if (uiText != null) textColor = uiText.color;
    }

    public void Setup(float bonusAmount)
    {
        string textStr = "+" + bonusAmount.ToString();

        if (textMesh != null) textMesh.text = textStr;
        else if (uiText != null) uiText.text = textStr;
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out over time
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifeTime);
        textColor.a = alpha;

        if (textMesh != null) textMesh.color = textColor;
        else if (uiText != null) uiText.color = textColor;

        // Destroy when lifetime is up
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}