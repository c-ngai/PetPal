using UnityEngine;

public class FoodItemBehaviour : MonoBehaviour
{
    private FeedMinigameController controller;
    private bool isFood;

    public void Initialize(FeedMinigameController c, bool food)
    {
        controller = c;
        isFood = food;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Food item collided with: " + other.tag);

        if (!other.CompareTag("Pet")) return;

        Debug.Log("Food item collided with pet!");

        controller.EatItem(isFood, other.GetComponent<PetStats>());
    }
}