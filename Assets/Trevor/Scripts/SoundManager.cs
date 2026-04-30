using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.35f; // Lowers the background music
    [Range(0f, 2f)] public float jumpVolumeMultiplier = 1.5f; // Boosts the jump sound

    [Header("Background Music")]
    public AudioClip mainBGM;
    public AudioClip minigameBGM;

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip bonusSound;
    public AudioClip gameOverSound;
    public AudioClip countdownSound;

    [Header("Feeding")]
    public AudioClip eatGoodSound;
    public AudioClip eatBadSound;
    public AudioClip feedingCompleteSound;

    [Header("Cleaning")]
    public AudioClip cleaningLoopSound;
    public AudioClip cleaningCompleteSound;

    [Header("Navigation")]
    public AudioClip arrowSound;
    public AudioClip selectionSound;

    [Header("Misc")]
    public AudioClip hatchingSound;
    public AudioClip errorSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        // Start the normal music when the game boots up
        PlayMainBGM();
    }

    void OnEnable()
    {
        GameEvents.OnPetJump += PlayJumpSound;
        GameEvents.OnBonusScored += PlayBonusSound;
        GameEvents.OnGameOver += PlayGameOverSound;
        GameEvents.OnGameCountdown += PlayGameCountdown;

        GameEvents.OnFeedingGood += PlayEatGood;
        GameEvents.OnFeedingBad += PlayEatBad;
        GameEvents.OnFeedingComplete += PlayFeedingComplete;

        GameEvents.OnCleaning += PlayCleaningTick;
        GameEvents.OnCleaningComplete += PlayCleaningComplete;
    }

    void OnDisable()
    {
        GameEvents.OnPetJump -= PlayJumpSound;
        GameEvents.OnBonusScored -= PlayBonusSound;
        GameEvents.OnGameOver -= PlayGameOverSound;
        GameEvents.OnGameCountdown -= PlayGameCountdown;

        GameEvents.OnFeedingGood -= PlayEatGood;
        GameEvents.OnFeedingBad -= PlayEatBad;
        GameEvents.OnFeedingComplete -= PlayFeedingComplete;

        GameEvents.OnCleaning -= PlayCleaningTick;
        GameEvents.OnCleaningComplete -= PlayCleaningComplete;
    }

    // Music Control Methods
    public void PlayMainBGM()
    {
        // Don't restart the song if it is already playing
        if (bgmSource != null && bgmSource.clip != mainBGM)
        {
            PlayBGM(mainBGM);
        }
    }

    public void PlayMinigameBGM()
    {
        if (bgmSource != null && bgmSource.clip != minigameBGM)
        {
            PlayBGM(minigameBGM);
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null)
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume; // Apply the reduced volume here
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // Event Handler Methods
    private void PlayJumpSound()
    {
        if (jumpSound != null && sfxSource != null)
        {
            // Use the volume multiplier specifically for the jump sound
            sfxSource.PlayOneShot(jumpSound, jumpVolumeMultiplier);
        }
    }

    private void PlayBonusSound() { PlayClip(bonusSound); }
    private void PlayGameOverSound() { PlayClip(gameOverSound); }

    private void PlayGameCountdown() { PlayClip(countdownSound); }

    private void PlayEatGood() { PlayClip(eatGoodSound); }
    private void PlayEatBad() { PlayClip(eatBadSound); }
    private void PlayFeedingComplete() { PlayClip(feedingCompleteSound); }

    private void PlayCleaningTick() { PlayClip(cleaningLoopSound); }
    private void PlayCleaningComplete() { PlayClip(cleaningCompleteSound); }

    public void PlayArrow() { PlayClip(arrowSound); }
    public void PlaySelection() { PlayClip(selectionSound); }
    public void PlayHatching() { PlayClip(hatchingSound); }
    public void PlayError() { PlayClip(errorSound); }

    // The core play method
    private void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}