using UnityEngine;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public bool isEnabled = true;
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;

    [Header("Enemy Restrictions")]
    [Tooltip("Optional: restrict which enemies can spawn here.")]
    public List<GameObject> allowedEnemies = new List<GameObject>();

    private void OnDrawGizmos()
    {
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

    public bool CanSpawn(GameObject enemyPrefab)
    {
        if (!isEnabled)
            return false;

        // If no restrictions, any enemy can spawn here
        if (allowedEnemies == null || allowedEnemies.Count == 0)
            return true;

        // Otherwise only allow listed enemies
        return allowedEnemies.Contains(enemyPrefab);
    }
}