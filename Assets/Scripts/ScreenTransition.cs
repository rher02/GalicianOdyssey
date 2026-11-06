
using UnityEngine;
using System.Collections;

public class ScreenTransition : MonoBehaviour
{
    [SerializeField] private RectTransform transitionPanel;
    [SerializeField] private float transitionDuration = 0.5f;

    private float PanelWidth => transitionPanel.rect.width;

    private void Awake()
    {
        // El Canvas debe permanecer entre escenas.
        DontDestroyOnLoad(transform.root.gameObject);

        transitionPanel.gameObject.SetActive(true);

        // Posición inicial: fuera por la izquierda.
        transitionPanel.anchoredPosition =
            new Vector2(-PanelWidth, 0f);
    }

    public IEnumerator CoverScreen()
    {
        transitionPanel.gameObject.SetActive(true);

        yield return MovePanel(Vector2.zero);
    }

    public IEnumerator RevealScreen()
    {
        // Esperar un frame para que la nueva escena se inicialice.
        yield return null;

        // Barrido hacia la derecha.
        yield return MovePanel(
            new Vector2(PanelWidth, 0f)
        );

        transitionPanel.gameObject.SetActive(false);
    }

    private IEnumerator MovePanel(Vector2 target)
    {
        Vector2 start = transitionPanel.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / transitionDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            transitionPanel.anchoredPosition =
                Vector2.Lerp(start, target, t);

            yield return null;
        }

        transitionPanel.anchoredPosition = target;
    }
}
