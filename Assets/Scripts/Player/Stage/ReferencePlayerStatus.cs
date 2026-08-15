using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ReferencePlayerStatus : MonoBehaviour
{
    // =========================================================
    // STATUS
    // =========================================================

    [Header("Status")]
    [SerializeField] private float initialStatus = 25f;
    [SerializeField] private float maxStatus = 100f;

    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private GameObject statusRoot;
    [SerializeField] private Slider statusSlider;
    [SerializeField] private TMP_Text statusText;

    // =========================================================
    // POPUP
    // =========================================================

    [Header("Status Popup")]
    [Tooltip("Один popup для положительных и отрицательных изменений.")]
    [SerializeField] private ReferenceStatusPopup statusPopup;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Status Animation")]
    [SerializeField] private float fillAnimationDuration = 0.18f;
    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float punchDuration = 0.22f;
    [SerializeField] private int punchVibrato = 5;
    [SerializeField] private float punchElasticity = 0.7f;

    // =========================================================
    // AUTO FIND
    // =========================================================

    [Header("Auto Find")]
    [SerializeField] private string sliderObjectName = "StatusSlider";
    [SerializeField] private string textObjectName = "StatusText";

    // =========================================================
    // STAGES
    // =========================================================

    [Header("Stages")]
    [SerializeField]
    private List<ReferenceStatusStage> statusStages =
        new List<ReferenceStatusStage>();

    // =========================================================
    // INTERNAL
    // =========================================================

    private float status;

    private ReferenceStatusStage currentStage;

    private bool visible;

    private Image sliderFillImage;

    private RectTransform sliderRect;

    private float displayedSliderValue;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        AutoFindUI();

        SortStages();

        status =
            Mathf.Clamp(
                initialStatus,
                0f,
                maxStatus
            );

        PrepareSlider();

        RefreshStatus(false);
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Hide();
    }

    // =========================================================
    // UI FIND
    // =========================================================

    private void AutoFindUI()
    {
        // -----------------------------------------------------
        // STATUS ROOT
        // -----------------------------------------------------

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
            {
                statusRoot =
                    root.gameObject;
            }
        }

        // -----------------------------------------------------
        // SLIDER
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // STATUS TEXT
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // POPUP
        // -----------------------------------------------------

        if (statusPopup == null)
        {
            statusPopup =
                GetComponentInChildren<
                    ReferenceStatusPopup>(
                    true
                );
        }

        SetupSlider();
    }

    // =========================================================
    // SLIDER SETUP
    // =========================================================

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
                statusSlider.fillRect
                    .GetComponent<Image>();
        }
    }

    // =========================================================
    // PREPARE SLIDER
    // =========================================================

    private void PrepareSlider()
    {
        if (statusSlider == null)
            return;

        float value =
            GetNormalizedStatus();

        statusSlider.SetValueWithoutNotify(
            value
        );

        displayedSliderValue =
            value;

        if (sliderRect != null)
        {
            sliderRect.DOKill();

            sliderRect.localScale =
                Vector3.one;
        }
    }

    // =========================================================
    // FIND CHILD
    // =========================================================

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
                Transform>(
                    true
                );

        for (int i = 0;
             i < children.Length;
             i++)
        {
            if (children[i].name ==
                objectName)
            {
                return children[i];
            }
        }

        return null;
    }

    // =========================================================
    // ADD STATUS
    // =========================================================

    public void AddStatus(float value)
    {
        if (value <= 0f)
            return;

        float oldStatus =
            status;

        status =
            Mathf.Clamp(
                status + value,
                0f,
                maxStatus
            );

        float actualChange =
            status - oldStatus;

        if (actualChange <= 0f)
            return;

        RefreshStatus(true);

        ShowStatusPopup(
            actualChange
        );
    }

    // =========================================================
    // REMOVE STATUS
    // =========================================================

    public void RemoveStatus(float value)
    {
        if (value <= 0f)
            return;

        float oldStatus =
            status;

        status =
            Mathf.Clamp(
                status - value,
                0f,
                maxStatus
            );

        float actualChange =
            oldStatus - status;

        if (actualChange > 0f)
        {
            RefreshStatus(true);

            ShowStatusPopup(
                -actualChange
            );
        }

        // -----------------------------------------------------
        // LOSE
        // -----------------------------------------------------

        if (status <= 0f &&
            GameManager.Instance != null)
        {
            GameManager.Instance.Lose();
        }
    }

    // =========================================================
    // STATUS POPUP
    // =========================================================

    private void ShowStatusPopup(
        float change)
    {
        if (!visible)
            return;

        if (statusPopup == null)
            return;

        if (Mathf.Approximately(
                change,
                0f))
        {
            return;
        }

        statusPopup.Show(
            change
        );
    }

    // =========================================================
    // SET STATUS
    // =========================================================

    public void SetStatus(float value)
    {
        status =
            Mathf.Clamp(
                value,
                0f,
                maxStatus
            );

        RefreshStatus(false);

        if (status <= 0f &&
            GameManager.Instance != null)
        {
            GameManager.Instance.Lose();
        }
    }

    // =========================================================
    // REFRESH STATUS
    // =========================================================

    private void RefreshStatus(
        bool animate)
    {
        ReferenceStatusStage stage =
            GetCurrentStage();

        // -----------------------------------------------------
        // STAGE CHANGED
        // -----------------------------------------------------

        if (stage != currentStage)
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

        // -----------------------------------------------------
        // SLIDER
        // -----------------------------------------------------

        UpdateSlider(
            animate
        );
    }

    // =========================================================
    // GET CURRENT STAGE
    // =========================================================

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
                result =
                    stage;
            }
            else
            {
                break;
            }
        }

        return result;
    }

    // =========================================================
    // APPLY STAGE
    // =========================================================

    private void ApplyStage(
        ReferenceStatusStage stage)
    {
        if (stage == null)
            return;

        UpdateText(stage);

        UpdateFillColor(stage);

        UpdateAppearance(stage);
    }

    // =========================================================
    // TEXT
    // =========================================================

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

    // =========================================================
    // FILL COLOR
    // =========================================================

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

    // =========================================================
    // APPEARANCE
    // =========================================================

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

    // =========================================================
    // SLIDER
    // =========================================================

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

        // -----------------------------------------------------
        // INSTANT
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // VALUE
        // -----------------------------------------------------

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
        )
        .SetEase(
            Ease.OutQuad
        );

        // -----------------------------------------------------
        // PUNCH
        // -----------------------------------------------------

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

    // =========================================================
    // SHOW
    // =========================================================

    public void Show()
    {
        visible = true;

        if (statusRoot != null)
        {
            statusRoot.SetActive(true);
        }

        if (statusSlider != null)
        {
            statusSlider.gameObject.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
        }

        RefreshStatus(false);
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        visible = false;

        // -----------------------------------------------------
        // SLIDER ANIMATION
        // -----------------------------------------------------

        if (statusSlider != null)
        {
            statusSlider.DOKill();
        }

        if (sliderRect != null)
        {
            sliderRect.DOKill();

            sliderRect.localScale =
                Vector3.one;
        }

        // -----------------------------------------------------
        // POPUP
        // -----------------------------------------------------

        if (statusPopup != null)
        {
            statusPopup.Hide();
        }

        // -----------------------------------------------------
        // ROOT
        // -----------------------------------------------------

        if (statusRoot != null)
        {
            statusRoot.SetActive(false);

            return;
        }

        // -----------------------------------------------------
        // SLIDER
        // -----------------------------------------------------

        if (statusSlider != null)
        {
            statusSlider.gameObject.SetActive(false);
        }

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetStatus()
    {
        status =
            Mathf.Clamp(
                initialStatus,
                0f,
                maxStatus
            );

        currentStage =
            null;

        if (statusPopup != null)
        {
            statusPopup.Hide();
        }

        PrepareSlider();

        RefreshStatus(false);

        Hide();
    }

    // =========================================================
    // SORT STAGES
    // =========================================================

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

    // =========================================================
    // PUBLIC API
    // =========================================================

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

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (statusSlider != null)
        {
            statusSlider.DOKill();
        }

        if (sliderRect != null)
        {
            sliderRect.DOKill();
        }

        if (statusPopup != null)
        {
            statusPopup.Hide();
        }
    }
}