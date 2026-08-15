using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ReferenceFinish : MonoBehaviour
{
    private Collider finishCollider;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        finishCollider =
            GetComponent<Collider>();

        finishCollider.isTrigger = true;
    }

    // =========================================================
    // TRIGGER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        ReferencePlayerController player =
            other.GetComponentInParent<
                ReferencePlayerController>();

        if (player == null)
            return;

        // -----------------------------------------------------
        // STOP PLAYER
        // -----------------------------------------------------

        player.StopAtFinish();

        // -----------------------------------------------------
        // WIN
        // -----------------------------------------------------

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Win();
        }

        // -----------------------------------------------------
        // DISABLE FINISH
        // -----------------------------------------------------

        if (finishCollider != null)
        {
            finishCollider.enabled = false;
        }
    }
}