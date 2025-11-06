using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    [System.Serializable]
    public class EncounterEnemy
    {
        public GameObject enemyPrefab;
        [Range(0, 100)]
        public float spawnChance = 20f;
    }

    [Header("Encounter Settings")]
    public List<EncounterEnemy> possibleEnemies;
    [Range(0, 100)]
    public float encounterChance = 15f;
    public float checkInterval = 5f;

    [Header("Despawn Settings")]
    [Tooltip("Time (in seconds) before a spawned enemy despawns if player stays in the same scene.")]
    public float despawnTime = 10f;

    [Header("Audio Settings")]
    [Tooltip("One or more audio clips that play when an encounter spawns.")]
    public List<AudioClip> spawnAudioClips = new List<AudioClip>();
    [Range(0f, 1f)] public float spawnAudioVolume = 1f;
    [Range(0.5f, 1.5f)] public float spawnAudioPitch = 1f;

    [Header("Background Music Settings")]
    [Tooltip("Background music that plays persistently across all scenes.")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Tooltip("How long to fade music in/out when encounters start or end.")]
    public float fadeDuration = 2f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private bool encounterActive = false;
    private SpawnPoint[] spawnPoints;
    private GameObject currentEncounter = null;
    private GameObject persistentHolder;

    private float spawnTimestamp;
    private string lastSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Persistent holder for spawned enemies
            persistentHolder = new GameObject("PersistentEncounters");
            DontDestroyOnLoad(persistentHolder);

            // Setup SFX source (for spawn sounds)
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            // Setup music source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0f; // start faded out

            // Play background music
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
                StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration));
                Debug.Log("[EncounterManager] Background music started with fade in.");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            lastSceneName = SceneManager.GetActiveScene().name;

            StartEncounterChecks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartEncounterChecks()
    {
        Debug.Log("[EncounterManager] Starting encounter checks...");
        StartCoroutine(CheckForEncounters());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[EncounterManager] Scene changed to: {scene.name}");
        spawnTimestamp = 0f;
        lastSceneName = scene.name;
        spawnPoints = FindObjectsOfType<SpawnPoint>();

        Debug.Log($"[EncounterManager] Found {spawnPoints.Length} spawn points in scene '{scene.name}'");

        if (currentEncounter != null)
        {
            Debug.Log("[EncounterManager] Destroying encounter from previous scene.");
            EndEncounter();
        }

        TrySceneImmediateCheck();
    }

    private void TrySceneImmediateCheck()
    {
        if (possibleEnemies == null || possibleEnemies.Count == 0)
            return;

        float roll = Random.Range(0f, 100f);
        Debug.Log($"[EncounterManager] Immediate scene check... Rolled {roll:F2} (chance {encounterChance}%)");

        if (roll < encounterChance)
        {
            TryStartEncounter();
        }
    }

    private IEnumerator CheckForEncounters()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!encounterActive)
            {
                float roll = Random.Range(0f, 100f);
                Debug.Log($"[EncounterManager] Periodic check... Rolled {roll:F2} (chance {encounterChance}%)");

                if (roll < encounterChance)
                {
                    TryStartEncounter();
                }
            }
            else
            {
                HandleDespawnTimer();
            }
        }
    }

    private void TryStartEncounter()
    {
        spawnPoints = FindObjectsOfType<SpawnPoint>();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EncounterManager] No spawn points found in the current scene!");
            return;
        }

        EncounterEnemy selectedEnemy = SelectRandomEnemy();
        if (selectedEnemy == null || selectedEnemy.enemyPrefab == null)
        {
            Debug.LogWarning("[EncounterManager] No valid enemy selected!");
            return;
        }

        List<SpawnPoint> validSpawns = new List<SpawnPoint>();
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.CanSpawn(selectedEnemy.enemyPrefab))
                validSpawns.Add(sp);
        }

        if (validSpawns.Count == 0)
        {
            Debug.LogWarning($"[EncounterManager] No valid spawn points for {selectedEnemy.enemyPrefab.name}. Using all spawns instead.");
            validSpawns.AddRange(spawnPoints);
        }

        SpawnPoint chosen = validSpawns[Random.Range(0, validSpawns.Count)];
        Debug.Log($"[EncounterManager] Encounter triggered! Spawning {selectedEnemy.enemyPrefab.name} at {chosen.name} ({chosen.transform.position})");

        SpawnEncounter(selectedEnemy.enemyPrefab, chosen.transform.position);
    }

    private EncounterEnemy SelectRandomEnemy()
    {
        float totalChance = 0f;
        foreach (var enemy in possibleEnemies)
            totalChance += enemy.spawnChance;

        float roll = Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var enemy in possibleEnemies)
        {
            cumulative += enemy.spawnChance;
            if (roll <= cumulative)
            {
                Debug.Log($"[EncounterManager] Selected enemy: {enemy.enemyPrefab.name} (roll {roll:F2}/{totalChance:F2})");
                return enemy;
            }
        }

        return null;
    }

    private void SpawnEncounter(GameObject enemyPrefab, Vector3 position)
    {
        if (currentEncounter != null)
        {
            Debug.Log("[EncounterManager] Destroying previous encounter before spawning a new one.");
            Destroy(currentEncounter);
        }

        encounterActive = true;
        currentEncounter = Instantiate(enemyPrefab, position, Quaternion.identity, persistentHolder.transform);
        spawnTimestamp = Time.time;

        Debug.Log($"[EncounterManager] Spawned {enemyPrefab.name} at {position}. Will despawn after {despawnTime} seconds if player stays in this scene.");

        PlaySpawnAudio();

        // Fade out background music during encounter
        StartCoroutine(FadeAudioSource(musicSource, 0f, fadeDuration));
    }

    private void PlaySpawnAudio()
    {
        if (spawnAudioClips == null || spawnAudioClips.Count == 0)
            return;

        AudioClip clip = spawnAudioClips[Random.Range(0, spawnAudioClips.Count)];
        sfxSource.pitch = Random.Range(spawnAudioPitch - 0.05f, spawnAudioPitch + 0.05f);
        sfxSource.volume = spawnAudioVolume;
        sfxSource.PlayOneShot(clip);

        Debug.Log($"[EncounterManager] Played spawn audio: {clip.name}");
    }

    private void HandleDespawnTimer()
    {
        if (!encounterActive || currentEncounter == null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != lastSceneName)
        {
            Debug.Log("[EncounterManager] Scene changed — resetting despawn timer.");
            spawnTimestamp = Time.time;
            lastSceneName = currentScene;
            return;
        }

        if (Time.time - spawnTimestamp >= despawnTime)
        {
            Debug.Log($"[EncounterManager] {currentEncounter.name} has been in the same scene for {despawnTime} seconds. Despawning now.");
            EndEncounter();
        }
    }

    public void EndEncounter()
    {
        encounterActive = false;
        if (currentEncounter != null)
        {
            Debug.Log("[EncounterManager] Encounter ended. Destroying current enemy.");
            Destroy(currentEncounter);
            currentEncounter = null;
        }
        spawnTimestamp = 0f;

        // Fade music back in
        StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration));
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        source.volume = targetVolume;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }
}

