using UnityEngine;

public class UIHandler : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.gameOverRestartStarted) return;

        GameManager.Instance.gameOverRestartStarted = true;
        GameManager.Instance.RestartGameAfterDelay(3f);
    }
}