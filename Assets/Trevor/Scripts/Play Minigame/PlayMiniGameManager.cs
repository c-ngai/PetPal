using UnityEngine;
using TMPro; // Ensure you have TextMeshPro installed
using System.Collections;

public class PlayMiniGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

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
    private bool gameActive = false;

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
        StartCoroutine(SpawnRoutine());
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
    }

    public void AddBonusScore(float amount)
    {
        score += amount;
    }

    public void EndGame()
    {
        if (!gameActive) return;

        gameActive = false;
        gameOverPanel.SetActive(true);

        StartCoroutine(EndGameRoutine());
    }

    IEnumerator EndGameRoutine()
    {
        yield return new WaitForSecondsRealtime(2f); // works even if timescale = 0

        Time.timeScale = 1f; // reset before leaving
        GameManager.Instance.GoBack();
    }
}