using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Start,
        Playing,
        Win,
        Lose
    }

    [Header("UI")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject failScreen;
    [SerializeField] private GameObject tutorial;

    [Header("Level UI")]
    [SerializeField] private TextMeshProUGUI levelNumber;

    public GameState State { get; private set; }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        State = GameState.Start;

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        if (failScreen != null)
        {
            failScreen.SetActive(false);
        }

        if (tutorial != null)
        {
            tutorial.SetActive(true);
        }

        UpdateLevelNumber();
    }

    // =========================================================
    // START GAME
    // =========================================================

    public void StartGame()
    {
        if (State != GameState.Start)
            return;

        State = GameState.Playing;

        if (tutorial != null)
        {
            tutorial.SetActive(false);
        }
    }

    // =========================================================
    // WIN
    // =========================================================

    public void Win()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Win;

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Win();
        }
    }

    // =========================================================
    // LOSE
    // =========================================================

    public void Lose()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Lose;

        if (failScreen != null)
        {
            failScreen.SetActive(true);
        }

        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Fail();
        }
    }

    // =========================================================
    // RETRY
    // =========================================================

    public void Retry()
    {
        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Click();
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void NextLevel()
    {
        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Click();
        }

        int nextIndex =
            SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex >=
            SceneManager.sceneCountInBuildSettings)
        {
            nextIndex = 0;
        }

        SceneManager.LoadScene(nextIndex);
    }

    // =========================================================
    // LEVEL NUMBER
    // =========================================================

    private void UpdateLevelNumber()
    {
        if (levelNumber == null)
            return;

        int level =
            SceneManager.GetActiveScene().buildIndex + 1;

        levelNumber.text =
            level.ToString();
    }
}