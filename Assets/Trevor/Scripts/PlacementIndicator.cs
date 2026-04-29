using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer handRenderer;
    public SpriteRenderer eggRenderer;

    [Header("Settings")]
    public Sprite closedHandSprite;
    public Vector3 hoverOffset = new Vector3(0f, 1.5f, 0f); // How high above the object it sits
    public float moveSpeed = 15f; // How fast it glides between selections

    private bool hasSnappedToFirstPosition = false;

    void Start()
    {
        // Only show this if we are actively placing a pet
        if (GameManager.Instance != null && GameManager.Instance.IsPlacingPet)
        {
            handRenderer.sprite = closedHandSprite;

            // Grab the prefab that was purchased in the shop
            GameObject purchasedPet = GameManager.Instance.currentPurchasedPetPrefab;

            if (purchasedPet != null)
            {
                Pet petScript = purchasedPet.GetComponent<Pet>();
                if (petScript != null)
                {
                    // Set the sprite to match the purchased egg
                    eggRenderer.sprite = petScript.GetEggSprite();
                }
            }
        }
        else
        {
            // Turn off the object entirely if we are not placing a pet
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Safety checks to make sure we have a valid list to track
        if (GameManager.Instance == null || GameManager.Instance.currentList == null) return;

        SelectionList list = GameManager.Instance.currentList;
        if (list.items.Count == 0) return;

        int currentIndex = list.GetCurrentIndex();

        // Ensure the index is valid
        if (currentIndex >= 0 && currentIndex < list.items.Count)
        {
            Highlightable currentTarget = list.items[currentIndex];

            if (currentTarget != null)
            {
                // Calculate where the indicator should be
                Vector3 targetPos = currentTarget.transform.position + hoverOffset;

                // If this is the first frame, snap instantly so it doesn't fly in from 0,0,0
                if (!hasSnappedToFirstPosition)
                {
                    transform.position = targetPos;
                    hasSnappedToFirstPosition = true;
                }
                else
                {
                    // Smoothly glide to the new target
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
                }
            }
        }
    }
}