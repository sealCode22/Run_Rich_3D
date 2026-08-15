using UnityEngine;

public sealed class ReferencePathway : MonoBehaviour
{
    [Header("Path")]
    [SerializeField]
    private float pointReachDistance = 0.75f;

    // =========================================================
    // INTERNAL
    // =========================================================

    private Transform[] points;

    private int currentSegment;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        CachePoints();
    }

    // =========================================================
    // CACHE
    // =========================================================

    private void CachePoints()
    {
        int pointCount =
            transform.childCount;

        if (pointCount == 0)
        {
            points =
                System.Array.Empty<Transform>();

            return;
        }

        points =
            new Transform[pointCount];

        for (int i = 0;
             i < pointCount;
             i++)
        {
            points[i] =
                transform.GetChild(i);
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetPath()
    {
        currentSegment = 0;
    }

    // =========================================================
    // TARGET ROTATION
    // =========================================================

    public float GetTargetRotation(
        Vector3 playerPosition)
    {
        if (points == null ||
            points.Length < 2)
        {
            return 0f;
        }

        int lastSegment =
            points.Length - 2;

        currentSegment =
            Mathf.Clamp(
                currentSegment,
                0,
                lastSegment
            );

        Transform startPoint =
            points[currentSegment];

        Transform endPoint =
            points[currentSegment + 1];

        if (startPoint == null ||
            endPoint == null)
        {
            return 0f;
        }

        Vector3 start =
            startPoint.position;

        Vector3 end =
            endPoint.position;

        Vector3 segment =
            end - start;

        segment.y = 0f;

        if (segment.sqrMagnitude < 0.001f)
        {
            return 0f;
        }

        // -----------------------------------------------------
        // CHECK NEXT POINT
        // -----------------------------------------------------

        Vector3 toEnd =
            end - playerPosition;

        toEnd.y = 0f;

        float distanceToEndSqr =
            toEnd.sqrMagnitude;

        float reachDistanceSqr =
            pointReachDistance *
            pointReachDistance;

        if (distanceToEndSqr <=
            reachDistanceSqr &&
            currentSegment < lastSegment)
        {
            currentSegment++;

            startPoint =
                points[currentSegment];

            endPoint =
                points[currentSegment + 1];

            if (startPoint == null ||
                endPoint == null)
            {
                return 0f;
            }

            start =
                startPoint.position;

            end =
                endPoint.position;

            segment =
                end - start;

            segment.y = 0f;

            if (segment.sqrMagnitude < 0.001f)
            {
                return 0f;
            }
        }

        return Mathf.Atan2(
            segment.x,
            segment.z
        ) *
        Mathf.Rad2Deg;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    public int GetCurrentSegment()
    {
        return currentSegment;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        int pointCount =
            transform.childCount;

        if (pointCount < 2)
            return;

        Gizmos.color =
            Color.red;

        for (int i = 0;
             i < pointCount - 1;
             i++)
        {
            Transform aTransform =
                transform.GetChild(i);

            Transform bTransform =
                transform.GetChild(i + 1);

            if (aTransform == null ||
                bTransform == null)
            {
                continue;
            }

            Vector3 a =
                aTransform.position;

            Vector3 b =
                bTransform.position;

            Gizmos.DrawLine(
                a,
                b
            );

            Gizmos.DrawSphere(
                a,
                0.25f
            );

            if (i == pointCount - 2)
            {
                Gizmos.DrawSphere(
                    b,
                    0.25f
                );
            }
        }
    }
}