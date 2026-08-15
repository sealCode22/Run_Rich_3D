using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ReferenceFinish : MonoBehaviour
{
    private Collider finishCollider;

    private void Awake()
    {
        finishCollider =
            GetComponent<Collider>();

        finishCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (GameManager.Instance == null)
            return;

        if (!GameManager.Instance.IsPlaying())
            return;

        ReferencePlayerController player =
            other.GetComponentInParent<
                ReferencePlayerController>();

        if (player == null)
            return;

        player.StopAtFinish();

        GameManager.Instance.Win();

        if (finishCollider != null)
        {
            finishCollider.enabled = false;
        }
    }
}