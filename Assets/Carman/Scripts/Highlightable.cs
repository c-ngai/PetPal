using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Highlightable : MonoBehaviour
{
    private SpriteRenderer sr;
    private SpriteRenderer outlineSR;

    [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, .65f); 
    [SerializeField] private float outlineScale = 1.08f;

    [SerializeField] private bool useScaleHighlight = false;
    [SerializeField] private float highlightScale = 1.1f;
    private Vector3 originalScale;

    void Awake()
    {

        originalScale = transform.localScale;

        sr = GetComponent<SpriteRenderer>();

        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(transform);

        // Place slightly behind in Z
        outline.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        outline.transform.localRotation = Quaternion.identity;
        outline.transform.localScale = Vector3.one * outlineScale;

        outlineSR = outline.AddComponent<SpriteRenderer>();
        outlineSR.sprite = sr.sprite;
        outlineSR.color = outlineColor;

        // Keep sorting the same
        outlineSR.sortingLayerID = sr.sortingLayerID;
        outlineSR.sortingOrder = sr.sortingOrder;

        outline.SetActive(false);

    }

    public void SetHighlight(bool highlighted)
    {
        outlineSR.gameObject.SetActive(highlighted);

        if (useScaleHighlight)
        {
            transform.localScale = highlighted ? originalScale * highlightScale : originalScale;
        }
    }

    void LateUpdate()
    {
        if (outlineSR.sprite != sr.sprite)
            outlineSR.sprite = sr.sprite;
    }
}