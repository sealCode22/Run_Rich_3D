using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class OpenDoor : MonoBehaviour
{
    [Header("Door Leaves")]
    [SerializeField]
    private Transform leftDoor;

    [SerializeField]
    private Transform rightDoor;

    [Header("Open Rotation")]
    [SerializeField]
    private float openAngle = 90f;

    [Header("Animation")]
    [SerializeField]
    private float duration = 0.6f;

    [SerializeField]
    private Ease ease = Ease.OutBack;

    [Header("Audio")]
    [SerializeField]
    private AudioClip openSound;

    // =========================================================
    // INTERNAL
    // =========================================================

    private Collider triggerCollider;

    private Quaternion leftStartRotation;
    private Quaternion rightStartRotation;

    private bool opened;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        triggerCollider =
            GetComponent<Collider>();

        // -----------------------------------------------------
        // DOOR WORKS AS TRIGGER
        // -----------------------------------------------------

        triggerCollider.isTrigger = true;

        // -----------------------------------------------------
        // SAVE INITIAL ROTATIONS
        // -----------------------------------------------------

        if (leftDoor != null)
        {
            leftStartRotation =
                leftDoor.localRotation;
        }

        if (rightDoor != null)
        {
            rightStartRotation =
                rightDoor.localRotation;
        }
    }

    // =========================================================
    // TRIGGER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (opened)
            return;

        if (!IsPlayer(other))
            return;

        // -----------------------------------------------------
        // ВАЖНО:
        // Дверь открывается БЕЗ ключа.
        //
        // Наличие ReferenceDoor / ключа здесь вообще
        // не проверяется.
        // -----------------------------------------------------

        Open();
    }

    // =========================================================
    // PLAYER CHECK
    // =========================================================

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        return other.GetComponentInParent<
            ReferencePlayerController>() != null;
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void Open()
    {
        if (opened)
            return;

        opened = true;

        // -----------------------------------------------------
        // LEFT
        // -----------------------------------------------------

        if (leftDoor != null)
        {
            leftDoor.DOKill();

            Quaternion targetRotation =
                leftStartRotation *
                Quaternion.Euler(
                    0f,
                    -openAngle,
                    0f
                );

            leftDoor
                .DOLocalRotateQuaternion(
                    targetRotation,
                    duration
                )
                .SetEase(ease);
        }

        // -----------------------------------------------------
        // RIGHT
        // -----------------------------------------------------

        if (rightDoor != null)
        {
            rightDoor.DOKill();

            Quaternion targetRotation =
                rightStartRotation *
                Quaternion.Euler(
                    0f,
                    openAngle,
                    0f
                );

            rightDoor
                .DOLocalRotateQuaternion(
                    targetRotation,
                    duration
                )
                .SetEase(ease);
        }

        // -----------------------------------------------------
        // SOUND
        // -----------------------------------------------------

        if (openSound != null &&
            ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Play(
                openSound
            );
        }
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void Close()
    {
        if (!opened)
            return;

        opened = false;

        // -----------------------------------------------------
        // LEFT
        // -----------------------------------------------------

        if (leftDoor != null)
        {
            leftDoor.DOKill();

            leftDoor
                .DOLocalRotateQuaternion(
                    leftStartRotation,
                    duration
                )
                .SetEase(ease);
        }

        // -----------------------------------------------------
        // RIGHT
        // -----------------------------------------------------

        if (rightDoor != null)
        {
            rightDoor.DOKill();

            rightDoor
                .DOLocalRotateQuaternion(
                    rightStartRotation,
                    duration
                )
                .SetEase(ease);
        }
    }

    // =========================================================
    // STATE
    // =========================================================

    public bool IsOpened
    {
        get
        {
            return opened;
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetDoor()
    {
        if (leftDoor != null)
        {
            leftDoor.DOKill();

            leftDoor.localRotation =
                leftStartRotation;
        }

        if (rightDoor != null)
        {
            rightDoor.DOKill();

            rightDoor.localRotation =
                rightStartRotation;
        }

        opened = false;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (leftDoor != null)
        {
            leftDoor.DOKill();
        }

        if (rightDoor != null)
        {
            rightDoor.DOKill();
        }
    }
}