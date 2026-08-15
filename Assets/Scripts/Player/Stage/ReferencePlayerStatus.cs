using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ReferencePlayerStatus : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private float initialStatus = 25f;
    [SerializeField] private float maxStatus = 100f;

    [Header("References")]
    [SerializeField] private ReferencePlayerController playerController;

    [Header("UI")]
    [SerializeField] private GameObject statusRoot;
    [SerializeField] private Slider statusSlider;
    [SerializeField] private TMP_Text statusText;

    [Header("Level Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float scorePunch = 0.15f;
    [SerializeField] private float scorePunchDuration = 0.2f;

    [Header("Status Popup")]
    [SerializeField] private ReferenceStatusPopup statusPopup;

    [Header("Auto Find")]
    [SerializeField] private string sliderObjectName = "StatusSlider";
    [SerializeField] private string textObjectName = "StatusText";

    [Header("Stages")]
    [SerializeField]
    private List<ReferenceStatusStage> statusStages =
        new List<ReferenceStatusStage>();

    [Header("Status Animation")]
    [SerializeField] private float fillAnimationDuration = 0.18f;
    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float punchDuration = 0.22f;
    [SerializeField] private int punchVibrato = 5;
    [SerializeField] private float punchElasticity = 0.7f;

    [Header("Stage Change Animation")]
    [SerializeField] private float stageScale = 1.65f;
    [SerializeField] private float stageScaleUpDuration = 0.12f;
    [SerializeField] private float stageScaleDownDuration = 0.35f;

    private float status;
    private float levelScore;

    private ReferenceStatusStage currentStage;

    private bool visible;

    private Image sliderFillImage;
    private RectTransform sliderRect;

    private float displayedSliderValue;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController =
                GetComponentInParent<
                    ReferencePlayerController>();
        }

        AutoFindUI();
        SortStages();

        status =
            Mathf.Clamp(
                initialStatus,
                0f,
                maxStatus
            );

        levelScore =
            status;

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
            scoreText.text =
                Mathf.RoundToInt(levelScore).ToString();
        }

        PrepareSlider();
        RefreshStatus(false);
    }

    private void Start()
    {
        Hide();
    }

    private void AutoFindUI()
    {
        if (statusRoot == null)
        {
            Transform root =
                FindChildByName(
                    transform,
                    "Canvas_Bar3D"
                );

            if (root == null)
            {
                root =
                    FindChildByName(
                        transform,
                        "StatusRoot"
                    );
            }

            if (root != null)
                statusRoot = root.gameObject;
        }

        if (statusSlider == null)
        {
            Transform sliderTransform =
                FindChildByName(
                    transform,
                    sliderObjectName
                );

            if (sliderTransform != null)
            {
                statusSlider =
                    sliderTransform.GetComponent<Slider>();
            }
        }

        if (statusSlider == null)
        {
            statusSlider =
                GetComponentInChildren<Slider>(true);
        }

        if (statusText == null)
        {
            Transform textTransform =
                FindChildByName(
                    transform,
                    textObjectName
                );

            if (textTransform != null)
            {
                statusText =
                    textTransform.GetComponent<TMP_Text>();
            }
        }

        if (statusText == null)
        {
            statusText =
                GetComponentInChildren<TMP_Text>(true);
        }

        if (statusPopup == null)
        {
            statusPopup =
                GetComponentInChildren<
                    ReferenceStatusPopup>(true);
        }

        SetupSlider();
    }

    private void SetupSlider()
    {
        if (statusSlider == null)
            return;

        statusSlider.minValue = 0f;
        statusSlider.maxValue = 1f;
        statusSlider.wholeNumbers = false;
        statusSlider.interactable = false;

        sliderRect =
            statusSlider.GetComponent<RectTransform>();

        if (statusSlider.fillRect != null)
        {
            sliderFillImage =
                statusSlider.fillRect.GetComponent<Image>();
        }
    }

    private void PrepareSlider()
    {
        if (statusSlider == null)
            return;

        float value =
            GetNormalizedStatus();

        statusSlider.SetValueWithoutNotify(value);

        displayedSliderValue =
            value;

        if (sliderRect != null)
        {
            sliderRect.DOKill();
            sliderRect.localScale =
                Vector3.one;
        }
    }

    private Transform FindChildByName(
        Transform root,
        string objectName)
    {
        if (root == null ||
            string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        Transform[] children =
            root.GetComponentsInChildren<
                Transform>(true);

        for (int i = 0;
            i < children.Length;
            i++)
        {
            if (children[i].name == objectName)
                return children[i];
        }

        return null;
    }

    public void AddStatus(float value)
    {
        if (value <= 0f)
            return;

        float oldStatus =
            status;

        float newStatus =
            Mathf.Clamp(
                status + value,
                0f,
                maxStatus
            );

        float actualChange =
            newStatus - oldStatus;

        if (actualChange <= 0f)
            return;

        status =
            newStatus;

        levelScore =
            status;

        UpdateScoreText();

        RefreshStatus(true);

        ShowStatusPopup(
            actualChange
        );
    }

    public void RemoveStatus(float value)
    {
        if (value <= 0f)
            return;

        float oldStatus =
            status;

        float newStatus =
            Mathf.Max(
                0f,
                status - value
            );

        float actualChange =
            oldStatus - newStatus;

        if (actualChange <= 0f)
            return;

        status =
            newStatus;

        levelScore =
            status;

        UpdateScoreText();

        RefreshStatus(true);

        ShowStatusPopup(
            -actualChange
        );

        CheckLoseCondition();
    }

    public void SetStatus(float value)
    {
        float newStatus =
            Mathf.Clamp(
                value,
                0f,
                maxStatus
            );

        float change =
            newStatus - status;

        status =
            newStatus;

        levelScore =
            status;

        UpdateScoreText();

        RefreshStatus(true);

        if (!Mathf.Approximately(change, 0f))
        {
            ShowStatusPopup(change);
        }

        CheckLoseCondition();
    }

    private void SyncScoreAndStatus()
    {
        levelScore =
            Mathf.Clamp(
                levelScore,
                0f,
                maxStatus
            );

        status =
            levelScore;

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText == null)
            return;

        scoreText.text =
            Mathf.RoundToInt(
                levelScore
            ).ToString();

        scoreText.gameObject.SetActive(
            visible
        );

        if (!visible)
            return;

        RectTransform rect =
            scoreText.rectTransform;

        rect.DOKill();

        rect.localScale =
            Vector3.one;

        rect.DOPunchScale(
            Vector3.one * scorePunch,
            scorePunchDuration,
            5,
            0.7f
        );
    }

    public float GetLevelScore()
    {
        return levelScore;
    }

    public int GetLevelScoreInt()
    {
        return Mathf.RoundToInt(
            levelScore
        );
    }

    public void ResetLevelScore()
    {
        levelScore = 0f;
        status = 0f;

        if (scoreText != null)
        {
            scoreText.DOKill();
            scoreText.rectTransform.DOKill();

            scoreText.text = "0";

            scoreText.rectTransform.localScale =
                Vector3.one;

            scoreText.gameObject.SetActive(
                visible
            );
        }

        currentStage = null;

        RefreshStatus(false);
    }

    private void CheckLoseCondition()
    {
        if (status > 0f &&
            levelScore > 0f)
        {
            return;
        }

        status = 0f;
        levelScore = 0f;

        UpdateScoreText();
        RefreshStatus(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Lose();
        }
    }

    private void ShowStatusPopup(float change)
    {
        if (!visible ||
            statusPopup == null ||
            Mathf.Approximately(
                change,
                0f))
        {
            return;
        }

        statusPopup.Show(change);
    }

    private void RefreshStatus(
        bool animate)
    {
        ReferenceStatusStage stage =
            GetCurrentStage();

        bool stageChanged =
            stage != currentStage;

        if (stageChanged)
        {
            currentStage =
                stage;

            if (stage != null)
            {
                ApplyStage(stage);
            }
        }
        else
        {
            UpdateText(stage);
            UpdateFillColor(stage);
        }

        UpdateSlider(animate);
    }

    private ReferenceStatusStage GetCurrentStage()
    {
        if (statusStages == null ||
            statusStages.Count == 0)
        {
            return null;
        }

        ReferenceStatusStage result =
            null;

        for (int i = 0;
            i < statusStages.Count;
            i++)
        {
            ReferenceStatusStage stage =
                statusStages[i];

            if (stage == null)
                continue;

            if (status >= stage.minStatus)
            {
                result = stage;
            }
            else
            {
                break;
            }
        }

        return result;
    }

    private void ApplyStage(
        ReferenceStatusStage stage)
    {
        if (stage == null)
            return;

        UpdateText(stage);
        UpdateFillColor(stage);
        UpdateAppearance(stage);

        AnimateStageChange();
    }

    private void AnimateStageChange()
    {
        if (!visible ||
            statusText == null)
        {
            return;
        }

        RectTransform rect =
            statusText.rectTransform;

        rect.DOKill();

        rect.localScale =
            Vector3.one;

        Sequence sequence =
            DOTween.Sequence();

        sequence.Append(
            rect.DOScale(
                Vector3.one * stageScale,
                stageScaleUpDuration
            ).SetEase(
                Ease.OutBack
            )
        );

        sequence.Append(
            rect.DOScale(
                Vector3.one,
                stageScaleDownDuration
            ).SetEase(
                Ease.OutBounce
            )
        );
    }

    private void UpdateText(
        ReferenceStatusStage stage)
    {
        if (statusText == null)
            return;

        statusText.text =
            stage != null
                ? stage.statusName
                : string.Empty;

        if (stage != null)
        {
            Color color =
                stage.statusColor;

            color.a = 1f;

            statusText.color =
                color;
        }

        statusText.gameObject.SetActive(
            visible
        );
    }

    private void UpdateFillColor(
        ReferenceStatusStage stage)
    {
        if (sliderFillImage == null ||
            stage == null)
        {
            return;
        }

        Color color =
            stage.statusColor;

        color.a = 1f;

        sliderFillImage.color =
            color;
    }

    private void UpdateAppearance(
        ReferenceStatusStage stage)
    {
        if (statusStages == null)
            return;

        for (int i = 0;
            i < statusStages.Count;
            i++)
        {
            ReferenceStatusStage item =
                statusStages[i];

            if (item == null ||
                item.appearance == null)
            {
                continue;
            }

            item.appearance.SetActive(
                item == stage
            );
        }
    }

    private void UpdateSlider(
        bool animate)
    {
        if (statusSlider == null)
            return;

        float target =
            GetNormalizedStatus();

        statusSlider.gameObject.SetActive(
            visible
        );

        if (!animate)
        {
            statusSlider.DOKill();

            statusSlider.SetValueWithoutNotify(
                target
            );

            displayedSliderValue =
                target;

            if (sliderRect != null)
            {
                sliderRect.DOKill();
                sliderRect.localScale =
                    Vector3.one;
            }

            return;
        }

        statusSlider.DOKill();

        DOTween.To(
            () => displayedSliderValue,
            value =>
            {
                displayedSliderValue =
                    value;

                if (statusSlider != null)
                {
                    statusSlider.SetValueWithoutNotify(
                        value
                    );
                }
            },
            target,
            fillAnimationDuration
        ).SetEase(
            Ease.OutQuad
        );

        if (sliderRect != null)
        {
            sliderRect.DOKill();

            sliderRect.localScale =
                Vector3.one;

            sliderRect.DOPunchScale(
                Vector3.one * punchScale,
                punchDuration,
                punchVibrato,
                punchElasticity
            );
        }
    }

    public void Show()
    {
        visible = true;

        if (statusRoot != null)
            statusRoot.SetActive(true);

        if (statusSlider != null)
            statusSlider.gameObject.SetActive(true);

        if (statusText != null)
            statusText.gameObject.SetActive(true);

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.alpha = 1f;

            UpdateScoreText();
        }

        RefreshStatus(false);
    }

    public void Hide()
    {
        visible = false;

        if (statusSlider != null)
            statusSlider.DOKill();

        if (sliderRect != null)
        {
            sliderRect.DOKill();
            sliderRect.localScale =
                Vector3.one;
        }

        if (statusText != null)
        {
            statusText.DOKill();
            statusText.rectTransform.DOKill();

            statusText.rectTransform.localScale =
                Vector3.one;
        }

        if (scoreText != null)
        {
            scoreText.DOKill();
            scoreText.rectTransform.DOKill();

            scoreText.gameObject.SetActive(false);
            scoreText.alpha = 1f;

            scoreText.rectTransform.localScale =
                Vector3.one;
        }

        if (statusPopup != null)
            statusPopup.Hide();

        if (statusRoot != null)
        {
            statusRoot.SetActive(false);
            return;
        }

        if (statusSlider != null)
            statusSlider.gameObject.SetActive(false);

        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }

    public void ResetStatus()
    {
        status =
            Mathf.Clamp(
                initialStatus,
                0f,
                maxStatus
            );

        levelScore =
            status;

        currentStage = null;

        if (statusPopup != null)
            statusPopup.Hide();

        PrepareSlider();
        RefreshStatus(false);

        Hide();
    }

    private void SortStages()
    {
        if (statusStages == null ||
            statusStages.Count <= 1)
        {
            return;
        }

        statusStages.Sort(
            (a, b) =>
            {
                if (a == null)
                    return -1;

                if (b == null)
                    return 1;

                return a.minStatus.CompareTo(
                    b.minStatus
                );
            }
        );
    }

    public float GetStatus()
    {
        return status;
    }

    public float GetNormalizedStatus()
    {
        if (maxStatus <= 0f)
            return 0f;

        return Mathf.Clamp01(
            status / maxStatus
        );
    }

    public string GetCurrentStatusName()
    {
        return currentStage != null
            ? currentStage.statusName
            : string.Empty;
    }

    public bool IsVisible()
    {
        return visible;
    }

    private void OnDestroy()
    {
        if (statusSlider != null)
            statusSlider.DOKill();

        if (sliderRect != null)
            sliderRect.DOKill();

        if (statusText != null)
        {
            statusText.DOKill();
            statusText.rectTransform.DOKill();
        }

        if (scoreText != null)
        {
            scoreText.DOKill();
            scoreText.rectTransform.DOKill();
        }

        if (statusPopup != null)
            statusPopup.Hide();
    }
}