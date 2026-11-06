
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject pressEnterText;
    public GameObject menuPanel;

    [Header("Opciones")]
    public TMP_Text[] menuOptions;

    [Header("Escenas")]
    public string newGameScene = "MapScene";
    public string optionsScene = "OptionsScene";
    public string creditsScene = "CreditsScene";

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int selectedOption = 0;
    private bool menuOpened = false;
    private bool loadingScene = false;

    private void Start()
    {
        menuOpened = false;
        loadingScene = false;

        pressEnterText.SetActive(true);
        menuPanel.SetActive(false);

        selectedOption = 0;

        UpdateSelection();
    }

    private void Update()
    {
        if (loadingScene)
            return;

        if (!menuOpened)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenMenu();
            }

            return;
        }

        // Navegar hacia arriba.
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W))
        {
            selectedOption--;

            if (selectedOption < 0)
                selectedOption = menuOptions.Length - 1;

            UpdateSelection();
        }

        // Navegar hacia abajo.
        if (Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S))
        {
            selectedOption++;

            if (selectedOption >= menuOptions.Length)
                selectedOption = 0;

            UpdateSelection();
        }

        // Confirmar opción.
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SelectOption();
        }
    }

    private void OpenMenu()
    {
        menuOpened = true;

        pressEnterText.SetActive(false);
        menuPanel.SetActive(true);

        selectedOption = 0;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            menuOptions[i].color =
                i == selectedOption ? selectedColor : normalColor;

            // Añade un indicador visual a la opción seleccionada.
            menuOptions[i].text =
                GetOptionName(i);
        }
    }

    private string GetOptionName(int index)
    {
        switch (index)
        {
            case 0:
                return (selectedOption == index ? "> " : "") +
                       "Nueva partida";

            case 1:
                return (selectedOption == index ? "> " : "") +
                       "Opciones";

            case 2:
                return (selectedOption == index ? "> " : "") +
                       "Créditos";

            default:
                return "";
        }
    }

    private void SelectOption()
    {
        switch (selectedOption)
        {
            case 0:
                LoadScene(newGameScene);
                break;

            case 1:
                LoadScene(optionsScene);
                break;

            case 2:
                LoadScene(creditsScene);
                break;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        loadingScene = true;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ChangeScene(sceneName);
        }
        else
        {
            Debug.LogWarning(
                "No existe SceneTransitionManager. Cargando directamente."
            );

            SceneManager.LoadScene(sceneName);
        }
    }
}
