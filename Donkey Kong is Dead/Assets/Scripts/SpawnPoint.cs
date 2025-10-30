using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public bool isEnabled = true;
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        // Visual indicator in the editor for spawn points
        Gizmos.color = gizmoColor;
        if (isEnabled)
        {
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
        }
    }
}