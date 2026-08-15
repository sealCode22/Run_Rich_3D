using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ReferenceStatusPopup : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image icon;

    // =========================================================
    // ICONS
    // =========================================================

    [Header("Icons")]
    [SerializeField] private Sprite positiveIcon;
    [SerializeField] private Sprite negativeIcon;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Animation")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float moveDistance = 55f;
    [SerializeField] private float startScale = 0.7f;
    [SerializeField] private float punchScale = 1.15f;

    [Header("Colors")]
    [SerializeField] private Color positiveColor = Color.white;
    [SerializeField] private Color negativeColor = Color.white;

    // =========================================================
    // INTERNAL
    // =========================================================

    private Vector2 startPosition;

    private Sequence sequence;

    // Сумма только текущего типа изменения.
    //
    // Например:
    // +2 -> +4 = +6
    //
    // Но:
    // +6 -> -1 = -1
    //
    // То есть положительные и отрицательные
    // изменения никогда не смешиваются.
    private float displayedChange;

    private bool displayedPositive;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot =
                GetComponent<RectTransform>();
        }

        if (valueText == null)
        {
            valueText =
                GetComponentInChildren<TMP_Text>(true);
        }

        if (icon == null)
        {
            icon =
                GetComponentInChildren<Image>(true);
        }

        if (popupRoot != null)
        {
            startPosition =
                popupRoot.anchoredPosition;
        }

        HideImmediate();
    }

    // =========================================================
    // SHOW
    // =========================================================

    public void Show(float change)
    {
        if (popupRoot == null ||
            valueText == null)
        {
            return;
        }

        if (Mathf.Approximately(change, 0f))
            return;

        // -----------------------------------------------------
        // ТЕКУЩЕЕ НАПРАВЛЕНИЕ
        // -----------------------------------------------------

        bool positive =
            change > 0f;

        // -----------------------------------------------------
        // ЕСЛИ НАПРАВЛЕНИЕ ПОМЕНЯЛОСЬ
        // -----------------------------------------------------
        //
        // Было:
        // +6
        //
        // Пришло:
        // -2
        //
        // Не делаем:
        // +4
        //
        // А начинаем новое отрицательное значение:
        // -2
        // -----------------------------------------------------

        if (displayedChange != 0f &&
            displayedPositive != positive)
        {
            displayedChange = 0f;
        }

        // -----------------------------------------------------
        // ЗАПОМИНАЕМ НАПРАВЛЕНИЕ
        // -----------------------------------------------------

        displayedPositive =
            positive;

        // -----------------------------------------------------
        // НАКАПЛИВАЕМ
        // -----------------------------------------------------
        //
        // +2 -> +4
        // displayedChange:
        // 2 -> 6
        //
        // -2 -> -3
        // displayedChange:
        // 2 -> 5
        // -----------------------------------------------------

        displayedChange +=
            Mathf.Abs(change);

        // -----------------------------------------------------
        // ПЕРЕЗАПУСКАЕМ АНИМАЦИЮ
        // -----------------------------------------------------

        sequence?.Kill();

        popupRoot.DOKill();

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        string prefix =
            displayedPositive
                ? "+"
                : "-";

        valueText.text =
            prefix +
            FormatValue(displayedChange);

        valueText.color =
            displayedPositive
                ? positiveColor
                : negativeColor;

        // -----------------------------------------------------
        // ICON
        // -----------------------------------------------------

        if (icon != null)
        {
            icon.sprite =
                displayedPositive
                    ? positiveIcon
                    : negativeIcon;

            icon.enabled =
                icon.sprite != null;
        }

        // -----------------------------------------------------
        // SHOW
        // -----------------------------------------------------

        popupRoot.gameObject.SetActive(true);

        popupRoot.anchoredPosition =
            startPosition;

        popupRoot.localScale =
            Vector3.one *
            startScale;

        SetAlpha(0f);

        // =====================================================
        // ANIMATION
        // =====================================================

        sequence =
            DOTween.Sequence();

        // -----------------------------------------------------
        // POP-IN
        // -----------------------------------------------------
        //
        // Картинка и число появляются одновременно,
        // потому что являются дочерними элементами popupRoot.
        // -----------------------------------------------------

        sequence.Append(
            popupRoot.DOScale(
                Vector3.one *
                punchScale,

                0.12f
            )
            .SetEase(
                Ease.OutBack
            )
        );

        sequence.Join(
            DOTween.To(
                GetAlpha,
                SetAlpha,
                1f,
                0.12f
            )
            .SetEase(
                Ease.OutQuad
            )
        );

        // -----------------------------------------------------
        // HOLD
        // -----------------------------------------------------

        sequence.AppendInterval(
            0.15f
        );

        // -----------------------------------------------------
        // MOVE UP
        // -----------------------------------------------------

        sequence.Append(
            popupRoot.DOAnchorPos(
                startPosition +
                Vector2.up *
                moveDistance,

                duration
            )
            .SetEase(
                Ease.OutCubic
            )
        );

        // -----------------------------------------------------
        // FADE OUT
        // -----------------------------------------------------
        //
        // Одновременно с движением исчезают:
        // - число
        // - иконка
        // -----------------------------------------------------

        sequence.Join(
            DOTween.To(
                GetAlpha,
                SetAlpha,
                0f,
                duration *
                0.65f
            )
            .SetEase(
                Ease.InQuad
            )
        );

        // -----------------------------------------------------
        // COMPLETE
        // -----------------------------------------------------

        sequence.OnComplete(
            HideImmediate
        );
    }

    // =========================================================
    // FORMAT
    // =========================================================

    private string FormatValue(
        float value)
    {
        if (Mathf.Approximately(
                value,
                Mathf.Round(value)))
        {
            return Mathf.RoundToInt(value)
                .ToString();
        }

        return value.ToString(
            "0.##"
        );
    }

    // =========================================================
    // ALPHA
    // =========================================================

    private float GetAlpha()
    {
        if (valueText == null)
            return 0f;

        return valueText.color.a;
    }

    private void SetAlpha(
        float alpha)
    {
        if (valueText != null)
        {
            Color color =
                valueText.color;

            color.a =
                alpha;

            valueText.color =
                color;
        }

        if (icon != null)
        {
            Color color =
                icon.color;

            color.a =
                alpha;

            icon.color =
                color;
        }
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        sequence?.Kill();

        HideImmediate();
    }

    private void HideImmediate()
    {
        if (popupRoot == null)
            return;

        popupRoot.gameObject.SetActive(false);

        popupRoot.anchoredPosition =
            startPosition;

        popupRoot.localScale =
            Vector3.one;

        SetAlpha(0f);

        // Сбрасываем накопление.
        displayedChange = 0f;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        sequence?.Kill();

        if (popupRoot != null)
        {
            popupRoot.DOKill();
        }
    }
}