using UnityEngine;

namespace ButchersGames
{
    public sealed class Level : MonoBehaviour
    {
        [SerializeField]
        private Transform playerSpawnPoint;

        public Transform PlayerSpawnPoint =>
            playerSpawnPoint;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;

                var matrix =
                    Gizmos.matrix;

                Gizmos.matrix =
                    playerSpawnPoint.localToWorldMatrix;

                Gizmos.DrawSphere(
                    Vector3.up * 0.5f +
                    Vector3.forward,
                    0.5f);

                Gizmos.DrawCube(
                    Vector3.up * 0.5f,
                    Vector3.one);

                Gizmos.matrix =
                    matrix;
            }
        }
#endif
    }
}