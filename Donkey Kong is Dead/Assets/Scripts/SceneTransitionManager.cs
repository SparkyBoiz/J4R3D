using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    public float fadeSpeed = 1.5f;
    private Image fadeImage;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeImage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupFadeImage()
    {
        // Create a Canvas that renders in Screen Space - Overlay
        GameObject canvasObj = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it renders on top
        canvasObj.AddComponent<CanvasScaler>();
        
        // Add GraphicRaycaster but configure it to ignore raycast
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        // Create the fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.clear;
        fadeImage.raycastTarget = false; // Prevent the fade image from blocking raycasts
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        
        // Parent the canvas to this object
        canvasObj.transform.SetParent(transform);
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(sceneName));
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Fade back in
        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0;
        Color currentColor = fadeImage.color;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0;
        Color currentColor = fadeImage.color;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}