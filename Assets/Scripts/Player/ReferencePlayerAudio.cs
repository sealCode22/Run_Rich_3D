using UnityEngine;

public sealed class ReferencePlayerAudio : MonoBehaviour
{
    public static ReferencePlayerAudio Instance { get; private set; }

    [Header("Sounds")]
    [SerializeField] private AudioClip collectCoins;
    [SerializeField] private AudioClip loseCoins;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip fail;
    [SerializeField] private AudioClip win;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource =
            GetComponentInChildren<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning(
                "ReferencePlayerAudio: AudioSource не найден."
            );
        }
    }

    public void CollectCoins()
    {
        Play(collectCoins);
    }

    public void LoseCoins()
    {
        Play(loseCoins);
    }

    public void Click()
    {
        Play(click);
    }

    public void Fail()
    {
        Play(fail);
    }

    public void Win()
    {
        Play(win);
    }

    public void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}