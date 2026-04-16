using UnityEngine;

public class UIActions : MonoBehaviour
{
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}