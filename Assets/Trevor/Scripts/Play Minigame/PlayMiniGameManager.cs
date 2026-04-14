using UnityEngine;
using TMPro; // Ensure you have TextMeshPro installed
using System.Collections;

public class PlayMiniGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinsText;
    public GameObject gameOverPanel;

    [Header("Bonus Popups")]
    public GameObject bonusPopupPrefab;
    public Transform popupSpawnPoint;

    [Header("Spawning")]
    public GameObject obstaclePrefab;
    public Transform spawnPoint;
    public float startSpawnInterval = 2f;
    public float minSpawnInterval = 0.8f;

    [Header("Difficulty Scaling")]
    public float initialSpeed = 5f;
    public float maxSpeed = 15f;
    public float speedIncreaseRate = 0.1f;

    private float currentSpeed;
    private float score = 0f;
    private int coinsEarned = 0;
    private bool gameActive = false;

    private Coroutine spawnCoroutine;

    void OnEnable()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        currentSpeed = initialSpeed;
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        gameOverPanel.SetActive(false);
        int countdown = 3;

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);

        gameActive = true;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (gameActive)
        {
            GameObject obs = Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity);
            obs.GetComponent<MiniGameObstacle>().speed = currentSpeed;

            // Wait for a random interval that gets shorter over time
            float currentInterval = Mathf.Max(minSpawnInterval, startSpawnInterval - (score / 1000f));
            yield return new WaitForSeconds(Random.Range(currentInterval, currentInterval + 0.5f));
        }
    }

    void Update()
    {
        if (!gameActive) return;

        // Increase score and speed over time
        score += Time.deltaTime * 10f;
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed + (speedIncreaseRate * Time.deltaTime));

        scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();

        coinsEarned = Mathf.FloorToInt(score / 100f) * 10;

        if (coinsText != null)
        {
            coinsText.text = "Coins: " + coinsEarned.ToString();
        }
    }

    public void AddBonusScore(float amount)
    {
        score += amount;

        if (bonusPopupPrefab != null && popupSpawnPoint != null)
        {
            GameObject popup = Instantiate(bonusPopupPrefab, popupSpawnPoint.position, Quaternion.identity);

            BonusTextPopup popupScript = popup.GetComponent<BonusTextPopup>();
            if (popupScript != null)
            {
                popupScript.Setup(amount);
            }
        }
    }

    public void EndGame()
    {
        if (!gameActive) return;

        gameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // ADDED: Freeze game time immediately when the game ends
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);

        Debug.Log("Final Coins Earned: " + coinsEarned);
        // CurrencyManager.Instance.AddCurrency(coinsEarned); 

        StartCoroutine(EndGameRoutine());
    }

    IEnumerator EndGameRoutine()
    {
        // Because Time.timeScale is 0, we must use WaitForSecondsRealtime
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f; // reset before leaving
        GameManager.Instance.GoBack();
    }
}