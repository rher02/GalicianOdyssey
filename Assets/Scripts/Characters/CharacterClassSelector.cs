
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Animación")]
    public float animationDuration = 0.25f;
    public float portraitMoveDistance = 800f;
    public float statsMoveDistance = 500f;

    [Header("Selección del equipo")]
    public int maxPartySize = 3;
    public Image[] partyIcons;

    private int currentIndex = 0;
    private bool isAnimating = false;
    private bool changingScene = false;

    private RectTransform[] icons;
    private Color[] originalIconColors;

    private Vector2 portraitOriginalPosition;
    private Vector2 statsOriginalPosition;

    private readonly List<int> selectedClasses = new List<int>();

    private void Start()
    {
        if (!ValidateReferences())
            return;

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

        statsOriginalPosition = statsPanel.anchoredPosition;

        if (classes.Length == 0 || icons.Length == 0)
        {
            Debug.LogError("No hay clases o iconos asignados.");
            return;
        }

        if (classes.Length != icons.Length)
        {
            Debug.LogError("El número de clases e iconos no coincide.");
            return;
        }

        UpdateCursor();
        UpdateCharacterDisplay();
        UpdatePartyIcons();

        Debug.Log("CHARACTER SELECTOR INICIADO");
    }

    private bool ValidateReferences()
    {
        if (classes == null || classIcons == null ||
            cursor == null || characterPortrait == null ||
            statsPanel == null || partyIcons == null)
        {
            Debug.LogError(
                "Faltan referencias en CharacterClassSelector."
            );

            return false;
        }

        return true;
    }

    private void Update()
    {
        if (isAnimating || changingScene ||
            classes == null || classes.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeClass(-1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeClass(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) ||
                 Input.GetKeyDown(KeyCode.KeypadEnter))
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

        Debug.Log("Seleccionado: " + classes[currentIndex].className);

        UpdateSelectedClassIcons();
        UpdatePartyIcons();

        if (selectedClasses.Count >= maxPartySize)
        {
            if (!SaveParty())
                return;

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                changingScene = true;

                if (SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.ChangeScene(sceneToLoad);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("No has indicado la escena de destino.");
            }
        }
    }

    private bool SaveParty()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("PARTYMANAGER NO EXISTE.");
            return false;
        }

        List<CharacterData> party = new List<CharacterData>();

        foreach (int index in selectedClasses)
        {
            CharacterData character =
                new CharacterData(classes[index]);

            party.Add(character);
        }

        PartyManager.Instance.SetParty(party);

        Debug.Log("EQUIPO GUARDADO: " + party.Count);

        return true;
    }

    private void CancelSelection()
    {
        if (selectedClasses.Count == 0)
            return;

        int lastSelected =
            selectedClasses[selectedClasses.Count - 1];

        selectedClasses.RemoveAt(selectedClasses.Count - 1);

        currentIndex = lastSelected;

        UpdateCursor();
        UpdateSelectedClassIcons();
        UpdatePartyIcons();
        UpdateCharacterDisplay();
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

    private void UpdateSelectedClassIcons()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            Image image = icons[i].GetComponent<Image>();

            if (image == null)
                continue;

            image.color = selectedClasses.Contains(i)
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

    private IEnumerator ChangeCharacterAnimation()
    {
        isAnimating = true;

        Vector2 portraitStart =
            characterPortrait.rectTransform.anchoredPosition;

        Vector2 statsStart = statsPanel.anchoredPosition;

        yield return AnimatePanels(
            portraitStart,
            portraitStart + Vector2.right * portraitMoveDistance,
            statsStart,
            statsStart + Vector2.down * statsMoveDistance
        );

        UpdateCharacterDisplay();

        characterPortrait.rectTransform.anchoredPosition =
            portraitOriginalPosition +
            Vector2.left * portraitMoveDistance;

        statsPanel.anchoredPosition =
            statsOriginalPosition +
            Vector2.up * statsMoveDistance;

        yield return AnimatePanels(
            characterPortrait.rectTransform.anchoredPosition,
            portraitOriginalPosition,
            statsPanel.anchoredPosition,
            statsOriginalPosition
        );

        isAnimating = false;
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
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            characterPortrait.rectTransform.anchoredPosition =
                Vector2.Lerp(portraitStart, portraitEnd, t);

            statsPanel.anchoredPosition =
                Vector2.Lerp(statsStart, statsEnd, t);

            yield return null;
        }

        characterPortrait.rectTransform.anchoredPosition = portraitEnd;
        statsPanel.anchoredPosition = statsEnd;
    }
}
