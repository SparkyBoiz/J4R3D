using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    [System.Serializable]
    public class EncounterEnemy
    {
        public GameObject enemyPrefab;
        [Range(0, 100)]
        public float spawnChance = 20f; // Default 20% chance
    }

    [Header("Encounter Settings")]
    public List<EncounterEnemy> possibleEnemies;
    [Range(0, 100)]
    public float encounterChance = 15f; // Default 15% chance per check
    public float checkInterval = 5f; // How often to check for encounters
    public float minimumPlayerMovementRequired = 1f; // Minimum distance player needs to move for encounter check
    
    private bool isCheckingForEncounter = false;
    private Vector3 lastPlayerPosition;
    private SpawnPoint[] spawnPoints;
    private bool encounterActive = false;
    private GameObject currentEncounter = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartEncounterChecks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartEncounterChecks()
    {
        lastPlayerPosition = Vector3.zero;
        StartCoroutine(CheckForEncounters());
    }

    private IEnumerator CheckForEncounters()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!encounterActive && CanTriggerEncounter())
            {
                TryStartEncounter();
            }
        }
    }

    private bool CanTriggerEncounter()
    {
        if (possibleEnemies == null || possibleEnemies.Count == 0)
            return false;

        // Get current player position - replace with your actual player reference
        Vector3 currentPlayerPos = FindPlayer()?.transform.position ?? Vector3.zero;
        
        float distanceMoved = Vector3.Distance(currentPlayerPos, lastPlayerPosition);
        lastPlayerPosition = currentPlayerPos;

        return distanceMoved >= minimumPlayerMovementRequired && Random.Range(0f, 100f) < encounterChance;
    }

    private GameObject FindPlayer()
    {
        // Replace this tag with whatever tag you use for your player
        return GameObject.FindGameObjectWithTag("Player");
    }

    private void TryStartEncounter()
    {
        // Find all spawn points in the current scene
        spawnPoints = FindObjectsOfType<SpawnPoint>();
        
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found in the current scene!");
            return;
        }

        // Pick a random valid spawn point
        SpawnPoint selectedSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Select an enemy based on spawn chances
        EncounterEnemy selectedEnemy = SelectRandomEnemy();
        
        if (selectedEnemy != null && selectedEnemy.enemyPrefab != null)
        {
            SpawnEncounter(selectedEnemy.enemyPrefab, selectedSpawn.transform.position);
        }
    }

    private EncounterEnemy SelectRandomEnemy()
    {
        float totalChance = 0f;
        foreach (var enemy in possibleEnemies)
        {
            totalChance += enemy.spawnChance;
        }

        float random = Random.Range(0f, totalChance);
        float currentTotal = 0f;

        foreach (var enemy in possibleEnemies)
        {
            currentTotal += enemy.spawnChance;
            if (random <= currentTotal)
            {
                return enemy;
            }
        }

        return null;
    }

    private void SpawnEncounter(GameObject enemyPrefab, Vector3 position)
    {
        if (currentEncounter != null)
        {
            Destroy(currentEncounter);
        }

        encounterActive = true;
        currentEncounter = Instantiate(enemyPrefab, position, Quaternion.identity);
        Debug.Log($"Spawned encounter at position: {position}");
    }

    public void EndEncounter()
    {
        encounterActive = false;
        if (currentEncounter != null)
        {
            Destroy(currentEncounter);
            currentEncounter = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}