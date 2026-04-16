using UnityEngine;
using TMPro;
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
        GameEvents.OnGameCountdown?.Invoke();
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

            float currentInterval = Mathf.Max(minSpawnInterval, startSpawnInterval - (score / 1000f));
            yield return new WaitForSeconds(Random.Range(currentInterval, currentInterval + 0.5f));
        }
    }

    void Update()
    {
        if (!gameActive) return;

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

        GameEvents.OnBonusScored?.Invoke();

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

        Time.timeScale = 0f;

        GameEvents.OnGameOver?.Invoke();

        gameOverPanel.SetActive(true);

        Debug.Log("Final Coins Earned: " + coinsEarned);

        // Ensure CurrencyManager exists in your project before calling this
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(coinsEarned);
        }

        // Apply max love stat to the specific pet that played the game
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.activeMinigameRoomID))
        {
            string roomID = GameManager.Instance.activeMinigameRoomID;

            if (GameManager.Instance.roomPets.TryGetValue(roomID, out PetData activePet))
            {
                // Set love to 100 (full) regardless of score
                activePet.love = 100f;
                activePet.lastSavedTime = System.DateTime.UtcNow.Ticks;
            }
        }

        StartCoroutine(EndGameRoutine());
    }

    IEnumerator EndGameRoutine()
    {
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        GameManager.Instance.GoBack();
    }
}