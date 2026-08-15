using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ButchersGames
{
    public class LevelManager : MonoBehaviour
    {
        // =========================================================
        // SINGLETON
        // =========================================================

        private static LevelManager _default;

        public static LevelManager Default
        {
            get
            {
                return _default;
            }
        }

        private void Awake()
        {
            if (_default != null &&
                _default != this)
            {
                Destroy(gameObject);
                return;
            }

            _default = this;
        }

        // =========================================================
        // PREFS
        // =========================================================

        private const string CurrentLevel_PrefsKey =
            "Current Level";

        private const string CompleteLevelCount_PrefsKey =
            "Complete Lvl Count";

        private const string LastLevelIndex_PrefsKey =
            "Last Level Index";

        private const string CurrentAttempt_PrefsKey =
            "Current Attempt";

        // =========================================================
        // CURRENT LEVEL
        // =========================================================

        public static int CurrentLevel
        {
            get
            {
                if (Default == null)
                    return 1;

                return
                    (CompleteLevelCount <
                     Default.Levels.Count
                        ? Default.CurrentLevelIndex
                        : CompleteLevelCount)
                    + 1;
            }

            set
            {
                PlayerPrefs.SetInt(
                    CurrentLevel_PrefsKey,
                    value
                );

                PlayerPrefs.Save();
            }
        }

        // =========================================================
        // COMPLETE LEVEL COUNT
        // =========================================================

        public static int CompleteLevelCount
        {
            get
            {
                return PlayerPrefs.GetInt(
                    CompleteLevelCount_PrefsKey
                );
            }

            set
            {
                PlayerPrefs.SetInt(
                    CompleteLevelCount_PrefsKey,
                    value
                );

                PlayerPrefs.Save();
            }
        }

        // =========================================================
        // LAST LEVEL INDEX
        // =========================================================

        public static int LastLevelIndex
        {
            get
            {
                return PlayerPrefs.GetInt(
                    LastLevelIndex_PrefsKey
                );
            }

            set
            {
                PlayerPrefs.SetInt(
                    LastLevelIndex_PrefsKey,
                    value
                );

                PlayerPrefs.Save();
            }
        }

        // =========================================================
        // CURRENT ATTEMPT
        // =========================================================

        public static int CurrentAttempt
        {
            get
            {
                return PlayerPrefs.GetInt(
                    CurrentAttempt_PrefsKey
                );
            }

            set
            {
                PlayerPrefs.SetInt(
                    CurrentAttempt_PrefsKey,
                    value
                );

                PlayerPrefs.Save();
            }
        }

        // =========================================================
        // LEVEL DATA
        // =========================================================

        public int CurrentLevelIndex;

        [SerializeField]
        private bool editorMode = false;

        [SerializeField]
        private LevelsList levels;

        public List<Level> Levels
        {
            get
            {
                return levels.lvls;
            }
        }

        // =========================================================
        // EVENTS
        // =========================================================

        public event Action OnLevelStarted;

        // =========================================================
        // INIT
        // =========================================================

        public void Init()
        {
#if !UNITY_EDITOR
            editorMode = false;
#endif

            if (!editorMode)
            {
                SelectLevel(
                    LastLevelIndex,
                    true
                );
            }

            if (LastLevelIndex != CurrentLevel)
            {
                CurrentAttempt = 0;
            }
        }

        // =========================================================
        // START LEVEL
        // =========================================================

        public void StartLevel()
        {
            OnLevelStarted?.Invoke();
        }

        // =========================================================
        // RESTART LEVEL
        // =========================================================

        public void RestartLevel()
        {
            SelectLevel(
                CurrentLevelIndex,
                false
            );
        }

        // =========================================================
        // NEXT LEVEL
        // =========================================================

        public void NextLevel()
        {
            if (!editorMode)
            {
                CurrentLevel++;
            }

            SelectLevel(
                CurrentLevelIndex + 1
            );
        }

        // =========================================================
        // SELECT LEVEL
        // =========================================================

        public void SelectLevel(
            int levelIndex,
            bool indexCheck = true)
        {
            if (Levels == null ||
                Levels.Count == 0)
            {
                Debug.LogError(
                    "LevelManager: " +
                    "Список уровней пуст."
                );

                return;
            }

            if (indexCheck)
            {
                levelIndex =
                    GetCorrectedIndex(
                        levelIndex
                    );
            }

            if (levelIndex < 0 ||
                levelIndex >= Levels.Count)
            {
                levelIndex =
                    0;
            }

            if (Levels[levelIndex] == null)
            {
                Debug.LogError(
                    "LevelManager: " +
                    "There is no prefab attached!"
                );

                return;
            }

            Level level =
                Levels[levelIndex];

            if (level)
            {
                SelLevelParams(level);

                CurrentLevelIndex =
                    levelIndex;
            }
        }

        // =========================================================
        // PREVIOUS LEVEL
        // =========================================================

        public void PrevLevel()
        {
            SelectLevel(
                CurrentLevelIndex - 1
            );
        }

        // =========================================================
        // CORRECT INDEX
        // =========================================================

        private int GetCorrectedIndex(
            int levelIndex)
        {
            if (editorMode)
            {
                return levelIndex >
                           Levels.Count - 1 ||
                       levelIndex <= 0
                    ? 0
                    : levelIndex;
            }

            int levelId =
                CurrentLevel;

            if (levelId >
                Levels.Count - 1)
            {
                if (levels.randomizedLvls)
                {
                    List<int> lvls =
                        Enumerable.Range(
                            0,
                            levels.lvls.Count
                        ).ToList();

                    if (CurrentLevelIndex >= 0 &&
                        CurrentLevelIndex < lvls.Count)
                    {
                        lvls.RemoveAt(
                            CurrentLevelIndex
                        );
                    }

                    if (lvls.Count > 0)
                    {
                        return lvls[
                            UnityEngine.Random.Range(
                                0,
                                lvls.Count
                            )
                        ];
                    }
                }

                return levelIndex %
                       Levels.Count;
            }

            return levelId;
        }

        // =========================================================
        // CREATE LEVEL
        // =========================================================

        private void SelLevelParams(
            Level level)
        {
            if (!level)
                return;

            ClearChilds();

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Instantiate(
                    level,
                    transform
                );
            }
            else
            {
                PrefabUtility.InstantiatePrefab(
                    level,
                    transform
                );
            }
#else
            Instantiate(
                level,
                transform
            );
#endif
        }

        // =========================================================
        // CLEAR LEVEL
        // =========================================================

        private void ClearChilds()
        {
            for (int i = transform.childCount - 1;
                 i >= 0;
                 i--)
            {
                GameObject destroyObject =
                    transform
                        .GetChild(i)
                        .gameObject;

#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Destroy(destroyObject);
                }
                else
                {
                    DestroyImmediate(
                        destroyObject
                    );
                }
#else
                Destroy(destroyObject);
#endif
            }
        }

        // =========================================================
        // DESTROY
        // =========================================================

        private void OnDestroy()
        {
            if (_default == this)
            {
                LastLevelIndex =
                    CurrentLevelIndex;

                _default = null;
            }
        }

        // =========================================================
        // QUIT
        // =========================================================

        private void OnApplicationQuit()
        {
            LastLevelIndex =
                CurrentLevelIndex;
        }
    }
}
