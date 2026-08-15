using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ReferenceCheckpoint : MonoBehaviour
{
    [Header("Flag Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private string animationTrigger = "Open";

    [Header("Audio")]
    [SerializeField] private AudioClip sound;

    private Collider triggerCollider;

    private bool activated;

    private int animationTriggerHash;

    private void Awake()
    {
        triggerCollider =
            GetComponent<Collider>();

        triggerCollider.isTrigger = true;

        animationTriggerHash =
            Animator.StringToHash(
                animationTrigger
            );

        if (animator == null)
        {
            Debug.LogWarning(
                "ReferenceCheckpoint: " +
                "Animator флага не назначен."
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        ReferencePlayerController player =
            other.GetComponentInParent<
                ReferencePlayerController>();

        if (player == null)
            return;

        Activate();
    }

    private void Activate()
    {
        activated = true;

        // -----------------------------------------------------
        // FLAG ANIMATION
        // -----------------------------------------------------

        if (animator != null)
        {
            animator.SetTrigger(
                animationTriggerHash
            );
        }

        // -----------------------------------------------------
        // DISABLE TRIGGER
        // -----------------------------------------------------

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        // -----------------------------------------------------
        // SOUND
        // -----------------------------------------------------

        if (ReferencePlayerAudio.Instance != null)
        {
            ReferencePlayerAudio.Instance.Play(
                sound
            );
        }
    }

    public void ResetCheckpoint()
    {
        activated = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}