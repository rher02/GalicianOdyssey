using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterClassSelector : MonoBehaviour
{
    [Header("Clases")]
    public CharacterClassData[] classes;

    [Header("Lista de imágenes")]
    public Transform classIcons;
    public RectTransform cursor;

    [Header("Retrato")]
    public Image characterPortrait;

    [Header("Estadísticas")]
    public RectTransform statsPanel;
    public TMP_Text classNameText;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public TMP_Text attackText;
    public TMP_Text magicText;
    public TMP_Text defenseText;
    public TMP_Text evadeText;

    [Header("Cambio de escena")]
    public string sceneToLoad;
    public float fadeDuration = 1f;

     private bool changingScene = false;

    [Header("Animación")]
    public float animationDuration = 0.25f;
    public float portraitMoveDistance = 800f;
    public float statsMoveDistance = 500f;

    private int currentIndex = 0;
    private bool isAnimating = false;

    private RectTransform[] icons;

    [Header("Selección del equipo")]
    public int maxPartySize = 3;
    public Image[] partyIcons;

    private readonly System.Collections.Generic.List<int> selectedClasses = new System.Collections.Generic.List<int>();

    private Color[] originalIconColors;

    private Vector2 portraitOriginalPosition;
    private Vector2 statsOriginalPosition;

    private void Start()
    {
        icons = classIcons.GetComponentsInChildren<RectTransform>();

        icons = System.Array.FindAll(
            icons,
            icon => icon != classIcons
        );

        originalIconColors = new Color[icons.Length];

        for (int i = 0; i < icons.Length; i++)
        {
            Image iconImage = icons[i].GetComponent<Image>();

            if (iconImage != null)
                originalIconColors[i] = iconImage.color;
        }

        portraitOriginalPosition =
            characterPortrait.rectTransform.anchoredPosition;

        statsOriginalPosition =
            statsPanel.anchoredPosition;

        if (classes.Length == 0 || icons.Length == 0)
            return;

        int total = Mathf.Min(classes.Length, icons.Length);

        if (total != classes.Length || total != icons.Length)
        {
            Debug.LogWarning(
                "El número de clases e imágenes no coincide.");
        }

        UpdateCursor();
        UpdateCharacterDisplay();
    }

    private void Update()
    {
        if (isAnimating || classes.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeClass(-1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeClass(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSelection();
        }
    }

    private void ConfirmSelection()
    {
        if (selectedClasses.Contains(currentIndex))
            return;

        if (selectedClasses.Count >= maxPartySize)
            return;

        selectedClasses.Add(currentIndex);

        UpdateSelectedClassIcons();
        UpdatePartyIcons();

         if (selectedClasses.Count >= maxPartySize)
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                StartCoroutine(FadeAndLoadScene());
            }
            else
            {
                Debug.LogError(
                    "No has indicado la escena de destino :(("
                );
            }
        }
    }

    private void CancelSelection()
    {
        if (selectedClasses.Count == 0)
            return;

        int lastSelected = selectedClasses[selectedClasses.Count - 1];

        selectedClasses.RemoveAt(selectedClasses.Count - 1);

        currentIndex = lastSelected;

        UpdateCursor();
        UpdateSelectedClassIcons();
        UpdatePartyIcons();

        UpdateCharacterDisplay();
    }

    private void UpdateSelectedClassIcons()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            Image iconImage = icons[i].GetComponent<Image>();

            if (iconImage == null)
                continue;

            bool selected = selectedClasses.Contains(i);

            iconImage.color = selected
                ? new Color(0.35f, 0.35f, 0.35f, 1f)
                : originalIconColors[i];
        }
    }

    private void UpdatePartyIcons()
    {
        for (int i = 0; i < partyIcons.Length; i++)
        {
            if (i < selectedClasses.Count)
            {
                partyIcons[i].sprite =
                    classes[selectedClasses[i]].portrait;

                partyIcons[i].enabled = true;
            }
            else
            {
                partyIcons[i].sprite = null;
                partyIcons[i].enabled = false;
            }
        }
    }

    private void ChangeClass(int direction)
    {
        int newIndex = currentIndex + direction;

        if (newIndex < 0 || newIndex >= classes.Length)
            return;

        currentIndex = newIndex;

        UpdateCursor();

        StartCoroutine(ChangeCharacterAnimation());
    }

    private void UpdateCursor()
    {
        cursor.position = icons[currentIndex].position;
    }

    private void UpdateCharacterDisplay()
    {
        CharacterClassData data = classes[currentIndex];

        characterPortrait.sprite = data.portrait;

        classNameText.text = data.className;
        hpText.text = "PV: " + data.maxHP;
        mpText.text = "PM: " + data.maxMP;

        attackText.text = "Ataque: " + data.attack;
        magicText.text = "Magia: " + data.magic;
        defenseText.text = "Defensa: " + data.defense;
        evadeText.text = "Evasión: " + data.evade;
    }

    private IEnumerator ChangeCharacterAnimation()
    {
        isAnimating = true;

        Vector2 portraitStart =
            characterPortrait.rectTransform.anchoredPosition;

        Vector2 statsStart =
            statsPanel.anchoredPosition;

        yield return StartCoroutine(AnimatePanels(
            portraitStart,
            portraitStart + Vector2.right * portraitMoveDistance,
            statsStart,
            statsStart + Vector2.down * statsMoveDistance
        ));

        UpdateCharacterDisplay();

        characterPortrait.rectTransform.anchoredPosition =
            portraitOriginalPosition +
            Vector2.left * portraitMoveDistance;

        statsPanel.anchoredPosition =
            statsOriginalPosition +
            Vector2.up * statsMoveDistance;

        yield return StartCoroutine(AnimatePanels(
            characterPortrait.rectTransform.anchoredPosition,
            portraitOriginalPosition,
            statsPanel.anchoredPosition,
            statsOriginalPosition
        ));

        isAnimating = false;
    }

    private IEnumerator FadeAndLoadScene()
    {
        changingScene = true;

        // Crear un panel negro que cubra la pantalla.
        GameObject fadeObject = new GameObject(
            "FadePanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        fadeObject.transform.SetParent(
            transform.root,
            false
        );

        RectTransform fadeRect =
            fadeObject.GetComponent<RectTransform>();

        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        Image fadeImage = fadeObject.GetComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        Canvas fadeCanvas = fadeObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        fadeObject.AddComponent<GraphicRaycaster>();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            fadeImage.color = new Color(0f, 0f, 0f, alpha);

            yield return null;
        }

        fadeImage.color = Color.black;

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator AnimatePanels(
        Vector2 portraitStart,
        Vector2 portraitEnd,
        Vector2 statsStart,
        Vector2 statsEnd)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / animationDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            characterPortrait.rectTransform.anchoredPosition =
                Vector2.Lerp(portraitStart, portraitEnd, t);

            statsPanel.anchoredPosition =
                Vector2.Lerp(statsStart, statsEnd, t);

            yield return null;
        }

        characterPortrait.rectTransform.anchoredPosition =
            portraitEnd;

        statsPanel.anchoredPosition =
            statsEnd;
    }
}