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

    // =========================================================
    // PICKUP
    // =========================================================

    [Header("Pickup")]
    [SerializeField]
    private PickupType pickupType = PickupType.Coin;

    [Header("Random Status Value")]
    [Min(0)]
    [SerializeField]
    private int statusMin = 5;

    [Min(0)]
    [SerializeField]
    private int statusMax = 15;

    [Tooltip("Если включено, после подбора объект уничтожается.")]
    [SerializeField]
    private bool destroyAfterPickup = true;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Pickup Animation")]
    [SerializeField]
    private float animationDuration = 0.3f;

    [SerializeField]
    private float scaleMultiplier = 1.35f;

    [SerializeField]
    private float moveUp = 0.35f;

    // =========================================================
    // IDLE ANIMATION
    // =========================================================

    [Header("Idle Animation")]
    [SerializeField]
    private bool rotate = true;

    [SerializeField]
    private float rotationSpeed = 120f;

    [SerializeField]
    private bool floatAnimation = true;

    [SerializeField]
    private float floatHeight = 0.12f;

    [SerializeField]
    private float floatDuration = 0.8f;

    // =========================================================
    // INTERNAL
    // =========================================================

    protected ReferencePlayerStatus playerStatus;

    protected Collider pickupCollider;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;

    private bool collected;

    // =========================================================
    // AWAKE
    // =========================================================

    protected virtual void Awake()
    {
        pickupCollider =
            GetComponent<Collider>();

        pickupCollider.isTrigger = true;

        startPosition =
            transform.localPosition;

        startScale =
            transform.localScale;

        startRotation =
            transform.localRotation;

        statusMin =
            Mathf.Max(
                0,
                statusMin
            );

        statusMax =
            Mathf.Max(
                0,
                statusMax
            );

        if (statusMax < statusMin)
        {
            statusMax =
                statusMin;
        }

        FindPlayerStatus();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ResetIdleAnimation();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (collected)
            return;

        UpdateIdleAnimation();
    }

    // =========================================================
    // IDLE ANIMATION
    // =========================================================

    private void UpdateIdleAnimation()
    {
        if (rotate)
        {
            float rotation =
                Time.time *
                rotationSpeed;

            Quaternion rotationOffset =
                Quaternion.Euler(
                    0f,
                    rotation,
                    0f
                );

            transform.localRotation =
                startRotation *
                rotationOffset;
        }

        if (floatAnimation)
        {
            float frequency =
                Mathf.PI * 2f /
                Mathf.Max(
                    0.01f,
                    floatDuration * 2f
                );

            float offset =
                Mathf.Sin(
                    Time.time *
                    frequency
                ) *
                floatHeight;

            Vector3 position =
                startPosition;

            position.y += offset;

            transform.localPosition =
                position;
        }
    }

    // =========================================================
    // RESET IDLE
    // =========================================================

    private void ResetIdleAnimation()
    {
        transform.localPosition =
            startPosition;

        transform.localRotation =
            startRotation;
    }

    // =========================================================
    // FIND STATUS
    // =========================================================

    private void FindPlayerStatus()
    {
        if (playerStatus != null)
            return;

        playerStatus =
            FindFirstObjectByType<
                ReferencePlayerStatus>();

        if (playerStatus == null)
        {
            Debug.LogWarning(
                "ReferencePickup: " +
                "ReferencePlayerStatus не найден.",
                this
            );
        }
    }

    // =========================================================
    // TRIGGER
    // =========================================================

    protected virtual void OnTriggerEnter(
        Collider other)
    {
        if (collected)
            return;

        if (!IsPlayer(other))
            return;

        Collect();
    }

    // =========================================================
    // PLAYER CHECK
    // =========================================================

    protected bool IsPlayer(
        Collider other)
    {
        if (other == null)
            return false;

        return other.GetComponentInParent<
            ReferencePlayerController>() != null;
    }

    // =========================================================
    // COLLECT
    // =========================================================

    protected virtual void Collect()
    {
        if (collected)
            return;

        collected = true;

        ApplyStatus();

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        PlayCollectAnimation();
    }

    // =========================================================
    // STATUS
    // =========================================================

    protected void ApplyStatus()
    {
        if (playerStatus == null)
        {
            FindPlayerStatus();
        }

        if (playerStatus == null)
            return;

        int randomValue =
            GetRandomStatusValue();

        switch (pickupType)
        {
            case PickupType.Coin:

                playerStatus.AddStatus(
                    randomValue
                );

                if (ReferencePlayerAudio.Instance != null)
                {
                    ReferencePlayerAudio.Instance
                        .CollectCoins();
                }

                break;

            case PickupType.Negative:

                playerStatus.RemoveStatus(
                    randomValue
                );

                if (ReferencePlayerAudio.Instance != null)
                {
                    ReferencePlayerAudio.Instance
                        .LoseCoins();
                }

                break;
        }
    }

    // =========================================================
    // RANDOM VALUE
    // =========================================================

    private int GetRandomStatusValue()
    {
        if (statusMax < statusMin)
        {
            statusMax =
                statusMin;
        }

        return Random.Range(
            statusMin,
            statusMax + 1
        );
    }

    // =========================================================
    // COLLECT ANIMATION
    // =========================================================

    private void PlayCollectAnimation()
    {
        transform.DOKill();

        Sequence sequence =
            DOTween.Sequence();

        Vector3 targetScale =
            startScale *
            scaleMultiplier;

        Vector3 targetPosition =
            transform.localPosition +
            Vector3.up *
            moveUp;

        sequence.Join(
            transform.DOScale(
                targetScale,
                animationDuration * 0.45f
            )
            .SetEase(
                Ease.OutBack
            )
        );

        sequence.Join(
            transform.DOLocalMove(
                targetPosition,
                animationDuration
            )
            .SetEase(
                Ease.OutQuad
            )
        );

        sequence.Append(
            transform.DOScale(
                Vector3.zero,
                animationDuration * 0.55f
            )
            .SetEase(
                Ease.InBack
            )
        );

        sequence.OnComplete(() =>
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

    // =========================================================
    // SETUP
    // =========================================================

    public void SetType(
        PickupType type,
        int minValue,
        int maxValue)
    {
        pickupType =
            type;

        statusMin =
            Mathf.Max(
                0,
                minValue
            );

        statusMax =
            Mathf.Max(
                0,
                maxValue
            );

        if (statusMax < statusMin)
        {
            statusMax =
                statusMin;
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

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

    // =========================================================
    // DESTROY
    // =========================================================

    protected virtual void OnDestroy()
    {
        transform.DOKill();
    }
}