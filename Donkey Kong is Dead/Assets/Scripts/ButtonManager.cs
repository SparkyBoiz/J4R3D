using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button hallwayButton;
    public Button windowButton;
    public Button holeButton;
    public Button basementButton;

    private void Start()
    {
        Debug.Log("ButtonManager: Starting initialization");
        
        // Add click listeners to the buttons
        if (hallwayButton != null)
        {
            hallwayButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            hallwayButton.onClick.AddListener(() => {
                Debug.Log("Hallway button clicked!");
                LoadScene("Hallway");
            });
            Debug.Log($"Hallway button initialized. Is interactable: {hallwayButton.interactable}");
        }
        else
        {
            Debug.LogError("Hallway button reference is missing!");
        }
        
        if (windowButton != null)
        {
            windowButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            windowButton.onClick.AddListener(() => {
                Debug.Log("Window button clicked!");
                LoadScene("Window");
            });
            Debug.Log($"Window button initialized. Is interactable: {windowButton.interactable}");
        }
        else
        {
            Debug.LogError("Window button reference is missing!");
        }
        
        if (holeButton != null)
        {
            holeButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            holeButton.onClick.AddListener(() => {
                Debug.Log("Hole button clicked!");
                LoadScene("Hole");
            });
            Debug.Log($"Hole button initialized. Is interactable: {holeButton.interactable}");
        }
        else
        {
            Debug.LogError("Hole button reference is missing!");
        }

        if (basementButton != null)
        {
            basementButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            basementButton.onClick.AddListener(() => {
                Debug.Log("Basement button clicked!");
                LoadScene("Basement");
            });
            Debug.Log($"Basement button initialized. Is interactable: {basementButton.interactable}");
        }
        else
        {
            Debug.LogError("Basement button reference is missing!");
        }
        
        // Check if EventSystem exists
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogError("No EventSystem found in the scene! Buttons won't work without it.");
        }
    }

    private void LoadScene(string sceneName)
    {
        Debug.Log($"Attempting to load scene: {sceneName}");
        // Use the SceneTransitionManager to load the scene with fade effect
        if (SceneTransitionManager.Instance != null)
        {
            Debug.Log("SceneTransitionManager found, initiating transition");
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("SceneTransitionManager not found in the scene! Make sure it's added to a GameObject in your starting scene.");
        }
    }

    private void OnDestroy()
    {
        // Clean up the listeners when the object is destroyed
        if (hallwayButton != null)
            hallwayButton.onClick.RemoveAllListeners();
        
        if (windowButton != null)
            windowButton.onClick.RemoveAllListeners();
        
        if (holeButton != null)
            holeButton.onClick.RemoveAllListeners();

        if (basementButton != null)
            basementButton.onClick.RemoveAllListeners();
    }
}