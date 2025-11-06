using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public Vector3 spawnPosition;
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 10f;
    public float destroyDelay = 5f;

    [Header("Rotation Settings")]
    public RotationType rotationType = RotationType.Fixed;
    public Vector3 fixedRotation = Vector3.zero;
    public Vector3 minRotation = Vector3.zero;
    public Vector3 maxRotation = new Vector3(0, 0, 360);

    public enum RotationType
    {
        Fixed,
        Random,
        RandomRange
    }

    private void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait for a random interval
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Spawn the prefab
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            // Calculate rotation based on selected type
            Quaternion rotation = GetSpawnRotation();
            
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, rotation);
            // Add the destruction component
            DestructionTimer destructionTimer = spawnedObject.AddComponent<DestructionTimer>();
            destructionTimer.destroyDelay = destroyDelay;
        }
        else
        {
            Debug.LogWarning("No prefab assigned to spawn!");
        }
    }

    private Quaternion GetSpawnRotation()
    {
        Vector3 eulerAngles;
        
        switch (rotationType)
        {
            case RotationType.Fixed:
                eulerAngles = fixedRotation;
                break;

            case RotationType.Random:
                eulerAngles = new Vector3(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );
                break;

            case RotationType.RandomRange:
                eulerAngles = new Vector3(
                    Random.Range(minRotation.x, maxRotation.x),
                    Random.Range(minRotation.y, maxRotation.y),
                    Random.Range(minRotation.z, maxRotation.z)
                );
                break;

            default:
                eulerAngles = Vector3.zero;
                break;
        }

        return Quaternion.Euler(eulerAngles);
    }
}