using System;
using DG.Tweening;
using UnityEngine;

public sealed class ReferenceSwipeTutorial : MonoBehaviour
{
    [Header("Tutorial Objects")]
    [SerializeField]
    private RectTransform finger;

    [SerializeField]
    private RectTransform textObject;

    [Header("Objects To Hide During Tutorial")]
    [SerializeField]
    private GameObject[] objectsToHide;

    [Header("Finger Animation")]
    [SerializeField]
    private float fingerDistance = 180f;

    [SerializeField]
    private float fingerMoveDuration = 0.8f;

    [Header("Text Pulse")]
    [SerializeField]
    private float pulseScale = 1.04f;

    [SerializeField]
    private float pulseDuration = 1.8f;

    [Header("Hide Animation")]
    [SerializeField]
    private float hideDuration = 0.35f;

    [SerializeField]
    private float hideMoveY = -40f;

    private RectTransform panelRect;
    private CanvasGroup canvasGroup;

    private Sequence fingerSequence;
    private Sequence pulseSequence;
    private Sequence hideSequence;

    private Vector2 fingerStartPosition;
    private Vector2 panelStartPosition;

    private Vector3 textStartScale;

    private bool hidden;

    private bool[] objectsInitialState;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        panelRect =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        if (panelRect != null)
        {
            panelStartPosition =
                panelRect.anchoredPosition;
        }

        if (finger != null)
        {
            fingerStartPosition =
                finger.anchoredPosition;
        }

        if (textObject != null)
        {
            textStartScale =
                textObject.localScale;
        }

        CacheObjectsState();
    }

    // =========================================================
    // CACHE STATES
    // =========================================================

    private void CacheObjectsState()
    {
        if (objectsToHide == null ||
            objectsToHide.Length == 0)
        {
            objectsInitialState =
                Array.Empty<bool>();

            return;
        }

        objectsInitialState =
            new bool[objectsToHide.Length];

        for (int i = 0;
             i < objectsToHide.Length;
             i++)
        {
            GameObject target =
                objectsToHide[i];

            if (target != null)
            {
                objectsInitialState[i] =
                    target.activeSelf;
            }
        }
    }

    // =========================================================
    // SHOW
    // =========================================================

    public void Show()
    {
        hidden = false;

        KillAnimations();

        gameObject.SetActive(true);

        HideTutorialObjects();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRect != null)
        {
            panelRect.DOKill();

            panelRect.anchoredPosition =
                panelStartPosition;
        }

        if (finger != null)
        {
            finger.DOKill();

            finger.anchoredPosition =
                fingerStartPosition;
        }

        if (textObject != null)
        {
            textObject.DOKill();

            textObject.localScale =
                textStartScale;
        }

        StartFingerAnimation();
        StartTextPulse();
    }

    // =========================================================
    // HIDE OTHER OBJECTS
    // =========================================================

    private void HideTutorialObjects()
    {
        if (objectsToHide == null)
            return;

        for (int i = 0;
             i < objectsToHide.Length;
             i++)
        {
            GameObject target =
                objectsToHide[i];

            if (target == null)
                continue;

            target.SetActive(false);
        }
    }

    // =========================================================
    // RESTORE OTHER OBJECTS
    // =========================================================

    private void RestoreTutorialObjects()
    {
        if (objectsToHide == null)
            return;

        for (int i = 0;
             i < objectsToHide.Length;
             i++)
        {
            GameObject target =
                objectsToHide[i];

            if (target == null)
                continue;

            bool state = true;

            if (objectsInitialState != null &&
                i < objectsInitialState.Length)
            {
                state =
                    objectsInitialState[i];
            }

            target.SetActive(state);
        }
    }

    // =========================================================
    // FINGER
    // =========================================================

    private void StartFingerAnimation()
    {
        if (finger == null)
            return;

        fingerSequence?.Kill();
        finger.DOKill();

        finger.anchoredPosition =
            fingerStartPosition;

        fingerSequence =
            DOTween.Sequence();

        fingerSequence.Append(
            finger.DOAnchorPosX(
                fingerStartPosition.x -
                fingerDistance,
                fingerMoveDuration)
            .SetEase(Ease.InOutSine)
        );

        fingerSequence.Append(
            finger.DOAnchorPosX(
                fingerStartPosition.x +
                fingerDistance,
                fingerMoveDuration * 2f)
            .SetEase(Ease.InOutSine)
        );

        fingerSequence.Append(
            finger.DOAnchorPosX(
                fingerStartPosition.x,
                fingerMoveDuration)
            .SetEase(Ease.InOutSine)
        );

        fingerSequence.SetLoops(
            -1,
            LoopType.Restart
        );
    }

    // =========================================================
    // TEXT
    // =========================================================

    private void StartTextPulse()
    {
        if (textObject == null)
            return;

        pulseSequence?.Kill();
        textObject.DOKill();

        textObject.localScale =
            textStartScale;

        Vector3 targetScale =
            textStartScale *
            pulseScale;

        pulseSequence =
            DOTween.Sequence();

        pulseSequence.Append(
            textObject.DOScale(
                targetScale,
                pulseDuration)
            .SetEase(Ease.InOutSine)
        );

        pulseSequence.Append(
            textObject.DOScale(
                textStartScale,
                pulseDuration)
            .SetEase(Ease.InOutSine)
        );

        pulseSequence.SetLoops(
            -1,
            LoopType.Restart
        );
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide(
        Action onComplete = null)
    {
        if (hidden)
        {
            onComplete?.Invoke();
            return;
        }

        hidden = true;

        fingerSequence?.Kill();
        pulseSequence?.Kill();

        if (finger != null)
        {
            finger.DOKill();
        }

        if (textObject != null)
        {
            textObject.DOKill();
        }

        hideSequence?.Kill();

        if (canvasGroup == null)
        {
            gameObject.SetActive(false);

            RestoreTutorialObjects();

            onComplete?.Invoke();

            return;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        hideSequence =
            DOTween.Sequence();

        hideSequence.Join(
            canvasGroup
                .DOFade(
                    0f,
                    hideDuration)
                .SetEase(Ease.InOutQuad)
        );

        if (panelRect != null)
        {
            hideSequence.Join(
                panelRect.DOAnchorPosY(
                    panelStartPosition.y +
                    hideMoveY,
                    hideDuration)
                .SetEase(Ease.InOutQuad)
            );
        }

        if (textObject != null)
        {
            hideSequence.Join(
                textObject.DOScale(
                    textStartScale,
                    hideDuration)
            );
        }

        hideSequence.OnComplete(() =>
        {
            if (this == null)
                return;

            gameObject.SetActive(false);

            RestoreTutorialObjects();

            onComplete?.Invoke();
        });
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetTutorial()
    {
        KillAnimations();

        RestoreTutorialObjects();

        hidden = false;

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRect != null)
        {
            panelRect.anchoredPosition =
                panelStartPosition;
        }

        if (finger != null)
        {
            finger.anchoredPosition =
                fingerStartPosition;
        }

        if (textObject != null)
        {
            textObject.localScale =
                textStartScale;
        }

        HideTutorialObjects();

        StartFingerAnimation();
        StartTextPulse();
    }

    // =========================================================
    // KILL ANIMATIONS
    // =========================================================

    private void KillAnimations()
    {
        fingerSequence?.Kill();
        pulseSequence?.Kill();
        hideSequence?.Kill();

        fingerSequence = null;
        pulseSequence = null;
        hideSequence = null;

        if (finger != null)
        {
            finger.DOKill();
        }

        if (textObject != null)
        {
            textObject.DOKill();
        }

        if (panelRect != null)
        {
            panelRect.DOKill();
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }
    }

    // =========================================================
    // STATE
    // =========================================================

    public bool IsVisible()
    {
        return !hidden;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        KillAnimations();
    }
}