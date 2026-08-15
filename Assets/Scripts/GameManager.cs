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

    public GameState State { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject failScreen;
    [SerializeField] private GameObject tutorial;

    [Header("Level UI")]
    [SerializeField] private TextMeshProUGUI levelNumber;

    [Header("Player")]
    [SerializeField] private ReferencePlayerController playerController;

    private ReferencePlayerStatus playerStatus;

    private static int displayedLevel = -1;
    private static string levelSceneName = string.Empty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeLevelNumber();
    }

    private void Start()
    {
        State = GameState.Start;

        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<ReferencePlayerController>();
        }

        if (playerStatus == null)
        {
            playerStatus =
                FindFirstObjectByType<ReferencePlayerStatus>();
        }

        if (winScreen != null)
            winScreen.SetActive(false);

        if (failScreen != null)
            failScreen.SetActive(false);

        if (tutorial != null)
            tutorial.SetActive(true);

        UpdateLevelNumber();
    }

    private void InitializeLevelNumber()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        if (levelSceneName == sceneName &&
            displayedLevel > 0)
        {
            return;
        }

        levelSceneName = sceneName;

        int sceneLevel =
            GetSceneLevelNumber(sceneName);

        if (sceneLevel > 0)
        {
            displayedLevel = sceneLevel;
        }
        else
        {
            displayedLevel = 1;
        }
    }

    public void StartGame()
    {
        if (State != GameState.Start)
            return;

        State = GameState.Playing;

        if (tutorial != null)
            tutorial.SetActive(false);
    }

    public void Win()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Win;

        StopPlayer();

        if (winScreen != null)
            winScreen.SetActive(true);

        if (failScreen != null)
            failScreen.SetActive(false);

        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Win();
        }
    }

    public void Lose()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Lose;

        StopPlayer();

        if (failScreen != null)
            failScreen.SetActive(true);

        if (winScreen != null)
            winScreen.SetActive(false);

        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Fail();
        }
    }

    private void StopPlayer()
    {
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<ReferencePlayerController>();
        }

        if (playerController != null)
        {
            playerController.StopAtFinish();
        }
    }

    public void Retry()
    {
        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Click();
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void NextLevel()
    {
        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Click();
        }

        Time.timeScale = 1f;

        displayedLevel++;

        Scene currentScene =
            SceneManager.GetActiveScene();

        string currentSceneName =
            currentScene.name;

        string nextSceneName =
            GetNextLevelName(currentSceneName);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            int nextSceneIndex =
                FindSceneBuildIndex(nextSceneName);

            if (nextSceneIndex >= 0)
            {
                levelSceneName = nextSceneName;

                SceneManager.LoadScene(
                    nextSceneIndex
                );

                return;
            }
        }

        levelSceneName = currentSceneName;

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    private string GetNextLevelName(
        string currentSceneName)
    {
        if (string.IsNullOrEmpty(currentSceneName))
            return null;

        const string prefix = "Level_";

        if (!currentSceneName.StartsWith(prefix))
            return null;

        string numberText =
            currentSceneName.Substring(
                prefix.Length
            );

        if (!int.TryParse(
                numberText,
                out int currentLevel))
        {
            return null;
        }

        return prefix +
               (currentLevel + 1);
    }

    private int FindSceneBuildIndex(
        string sceneName)
    {
        int sceneCount =
            SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath =
                SceneUtility.GetScenePathByBuildIndex(i);

            string buildSceneName =
                System.IO.Path.GetFileNameWithoutExtension(
                    scenePath
                );

            if (buildSceneName == sceneName)
                return i;
        }

        return -1;
    }

    private int GetSceneLevelNumber(
        string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return -1;

        const string prefix = "Level_";

        if (!sceneName.StartsWith(prefix))
            return -1;

        string numberText =
            sceneName.Substring(
                prefix.Length
            );

        if (int.TryParse(
                numberText,
                out int level))
        {
            return level;
        }

        return -1;
    }

    private void UpdateLevelNumber()
    {
        if (levelNumber == null)
            return;

        levelNumber.text =
            "Уровень " +
            displayedLevel;
    }

    public int GetCurrentLevel()
    {
        return displayedLevel;
    }

    public bool IsPlaying()
    {
        return State == GameState.Playing;
    }

    public bool IsFinished()
    {
        return State == GameState.Win ||
               State == GameState.Lose;
    }
}