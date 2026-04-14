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
        if (!other.CompareTag("Pet")) return;

        Debug.Log("Food item collided with pet!");

        controller.ConsumeItem(isFood, other.GetComponent<PetStats>());
    }
}