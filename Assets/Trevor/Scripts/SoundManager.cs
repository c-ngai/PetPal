using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip bonusSound;
    public AudioClip gameOverSound;
    public AudioClip countdownSound;

    void Awake()
    {
        // Standard Singleton setup to persist across scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-grab the AudioSource if it wasn't assigned in the inspector
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    // Subscribe to events when this script is turned on
    void OnEnable()
    {
        GameEvents.OnPetJump += PlayJumpSound;
        GameEvents.OnBonusScored += PlayBonusSound;
        GameEvents.OnGameOver += PlayGameOverSound;
        GameEvents.OnGameCountdown += PlayGameCountdown;
    }

    // ALWAYS unsubscribe when disabled to prevent memory leaks
    void OnDisable()
    {
        GameEvents.OnPetJump -= PlayJumpSound;
        GameEvents.OnBonusScored -= PlayBonusSound;
        GameEvents.OnGameOver -= PlayGameOverSound;
        GameEvents.OnGameCountdown -= PlayGameCountdown;
    }

    // Event Handler Methods
    private void PlayJumpSound() { PlayClip(jumpSound); }
    private void PlayBonusSound() { PlayClip(bonusSound); }
    private void PlayGameOverSound() { PlayClip(gameOverSound); }

    private void PlayGameCountdown() { PlayClip(countdownSound); }

    // The core play method
    private void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot allows multiple sounds to overlap without cutting each other off
            sfxSource.PlayOneShot(clip);
        }
    }
}