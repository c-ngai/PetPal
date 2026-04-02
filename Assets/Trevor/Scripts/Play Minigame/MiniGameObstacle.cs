using UnityEngine;

public class MiniGameObstacle : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Destroy if it goes off screen
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}