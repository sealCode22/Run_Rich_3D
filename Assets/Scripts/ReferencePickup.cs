using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReferencePickup : MonoBehaviour
{
    public enum PickupType
    {
        Coin,
        Negative
    }

    [Header("Pickup")]
    [SerializeField] private PickupType pickupType = PickupType.Coin;

    [Min(0)]
    [SerializeField] private int statusMin = 5;

    [Min(0)]
    [SerializeField] private int statusMax = 15;

    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Pickup Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float scaleMultiplier = 1.35f;
    [SerializeField] private float moveUp = 0.35f;

    [Header("Idle Animation")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 120f;

    [SerializeField] private bool floatAnimation = true;
    [SerializeField] private float floatHeight = 0.12f;
    [SerializeField] private float floatDuration = 0.8f;

    private ReferencePlayerStatus playerStatus;
    protected Collider pickupCollider;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;

    private Tween rotateTween;
    private Tween floatTween;
    private Sequence collectSequence;

    private bool collected;

    protected virtual void Awake()
    {
        pickupCollider = GetComponent<Collider>();

        if (pickupCollider != null)
            pickupCollider.isTrigger = true;

        startPosition = transform.localPosition;
        startScale = transform.localScale;
        startRotation = transform.localRotation;

        statusMin = Mathf.Max(0, statusMin);
        statusMax = Mathf.Max(0, statusMax);

        if (statusMax < statusMin)
            statusMax = statusMin;
    }

    private void Start()
    {
        StartIdleAnimation();
    }

    private void StartIdleAnimation()
    {
        StopIdleAnimation();

        if (rotate && rotationSpeed != 0f)
        {
            float duration =
                360f / Mathf.Abs(rotationSpeed);

            rotateTween =
                transform.DOLocalRotate(
                    new Vector3(0f, 360f, 0f),
                    duration,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        if (floatAnimation &&
            floatHeight > 0f &&
            floatDuration > 0.01f)
        {
            floatTween =
                transform.DOLocalMoveY(
                    startPosition.y + floatHeight,
                    floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void StopIdleAnimation()
    {
        rotateTween?.Kill();
        floatTween?.Kill();

        rotateTween = null;
        floatTween = null;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!IsPlayer(other))
            return;

        Collect();
    }

    protected bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        return other.GetComponentInParent<
            ReferencePlayerController>() != null;
    }

    protected virtual void Collect()
    {
        if (collected)
            return;

        collected = true;

        StopIdleAnimation();

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        ApplyStatus();

        PlayCollectAnimation();
    }

    protected void ApplyStatus()
    {
        if (playerStatus == null)
        {
            playerStatus =
                FindFirstObjectByType<
                    ReferencePlayerStatus>();
        }

        if (playerStatus == null)
            return;

        int value =
            GetRandomStatusValue();

        if (pickupType == PickupType.Coin)
        {
            playerStatus.AddStatus(value);

            if (ReferencePlayerAudio.Instance != null)
            {
                ReferencePlayerAudio.Instance
                    .CollectCoins();
            }
        }
        else
        {
            playerStatus.RemoveStatus(value);

            if (ReferencePlayerAudio.Instance != null)
            {
                ReferencePlayerAudio.Instance
                    .LoseCoins();
            }
        }
    }

    private int GetRandomStatusValue()
    {
        if (statusMax < statusMin)
            statusMax = statusMin;

        return Random.Range(
            statusMin,
            statusMax + 1
        );
    }

    private void PlayCollectAnimation()
    {
        transform.DOKill();

        collectSequence?.Kill();

        Vector3 targetScale =
            startScale * scaleMultiplier;

        Vector3 targetPosition =
            transform.localPosition +
            Vector3.up * moveUp;

        collectSequence =
            DOTween.Sequence();

        collectSequence.Join(
            transform.DOScale(
                targetScale,
                animationDuration * 0.45f)
            .SetEase(Ease.OutBack));

        collectSequence.Join(
            transform.DOLocalMove(
                targetPosition,
                animationDuration)
            .SetEase(Ease.OutQuad));

        collectSequence.Append(
            transform.DOScale(
                Vector3.zero,
                animationDuration * 0.55f)
            .SetEase(Ease.InBack));

        collectSequence.OnComplete(() =>
        {
            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        });
    }

    public void SetPlayerStatus(
        ReferencePlayerStatus status)
    {
        playerStatus = status;
    }

    public void SetType(
        PickupType type,
        int minValue,
        int maxValue)
    {
        pickupType = type;

        statusMin = Mathf.Max(0, minValue);
        statusMax = Mathf.Max(0, maxValue);

        if (statusMax < statusMin)
            statusMax = statusMin;
    }

    public PickupType GetPickupType()
    {
        return pickupType;
    }

    public int GetStatusValue()
    {
        return GetRandomStatusValue();
    }

    public int GetStatusMin()
    {
        return statusMin;
    }

    public int GetStatusMax()
    {
        return statusMax;
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        collected = false;

        if (pickupCollider != null)
            pickupCollider.enabled = true;

        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
        transform.localScale = startScale;

        StartIdleAnimation();
    }

    private void OnDisable()
    {
        StopIdleAnimation();

        collectSequence?.Kill();
        collectSequence = null;

        transform.DOKill();
    }

    protected virtual void OnDestroy()
    {
        StopIdleAnimation();

        collectSequence?.Kill();

        transform.DOKill();
    }
}