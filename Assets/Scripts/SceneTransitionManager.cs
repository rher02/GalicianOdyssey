
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Configuración")]
    public float transitionDuration = 0.8f;

    private CanvasGroup canvasGroup;
    private Image fadeImage;

    private bool transitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateTransitionCanvas();
    }

    private void CreateTransitionCanvas()
    {
        GameObject canvasObject = new GameObject("TransitionCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        canvasObject.AddComponent<CanvasScaler>();

        canvasGroup = canvasObject.AddComponent<CanvasGroup>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = true;

        RectTransform rect = fadeImage.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void ChangeScene(string sceneName)
    {
        if (transitioning)
            return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "No se puede cargar la escena: " + sceneName +
                ". Comprueba que está en Build Profiles > Scene List."
            );

            return;
        }

        StartCoroutine(ChangeSceneRoutine(sceneName));
    }

    private IEnumerator ChangeSceneRoutine(string sceneName)
    {
        transitioning = true;

        Time.timeScale = 1f;

        canvasGroup.blocksRaycasts = true;

        Debug.Log("Iniciando transición hacia " + sceneName);

        yield return Fade(1f);

        Debug.Log("Cargando escena: " + sceneName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Error al iniciar la carga de escena.");

            yield return Fade(0f);

            canvasGroup.blocksRaycasts = false;
            transitioning = false;

            yield break;
        }

        while (!operation.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();

        Debug.Log("Escena cargada correctamente.");

        yield return Fade(0f);

        canvasGroup.blocksRaycasts = false;
        transitioning = false;
    }

    private IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                start,
                target,
                elapsed / transitionDuration
            );

            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
