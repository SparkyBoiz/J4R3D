using UnityEngine;
using UnityEngine.SceneManagement;

public class DestructionTimer : MonoBehaviour
{
    public float destroyDelay = 5f;
    public string targetTag = "Player";  // The tag to look for
    public bool destroyWithoutTarget = true;  // If true, will destroy even if target isn't found
    private bool targetInScene = false;
    private float destructionTime;

    private void Start()
    {
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckForPlayerInScene();
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckForPlayerInScene();
    }

    private void CheckForPlayerInScene()
    {
        // If we should destroy without target, start timer immediately
        if (destroyWithoutTarget && destructionTime == 0)
        {
            destructionTime = Time.time + destroyDelay;
            return;
        }

        // Check if there's a target in the current scene
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        bool wasTargetInScene = targetInScene;
        targetInScene = target != null;

        // If target just entered the scene, start destruction timer
        if (!wasTargetInScene && targetInScene)
        {
            destructionTime = Time.time + destroyDelay;
        }
    }

    private void Update()
    {
        if ((targetInScene || destroyWithoutTarget) && destructionTime > 0 && Time.time >= destructionTime)
        {
            Destroy(gameObject);
        }
    }
}