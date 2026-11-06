using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    [Header("Encounter Settings")]
    [SerializeField] private int minSteps = 3;
    [SerializeField] private int maxSteps = 5;

    [Range(0f, 1f)]
    [SerializeField] private float encounterChance = 0.5f;

    [Header("Scene Settings")]
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string mapSceneName = "MapScene";

    [Header("Transition")]
    [SerializeField] private ScreenTransition screenTransition;

    private int stepsUntilEncounter;
    private bool encounterStarted = false;

    public bool IsEncounterStarted => encounterStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SetNextEncounter();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mapSceneName)
        {
            encounterStarted = false;
            SetNextEncounter();

            Movement movement = FindFirstObjectByType<Movement>();

            if (movement != null)
            {
                movement.UnlockMovement();
            }

            Debug.Log("MAPA CARGADO. Encuentro desbloqueado.");
        }
    }

    public void RegisterStep()
    {
        if (encounterStarted)
            return;

        stepsUntilEncounter--;

        if (stepsUntilEncounter <= 0)
        {
            if (Random.value <= encounterChance)
            {
                encounterStarted = true;

                // Conservamos la posición antes de entrar en batalla.
                BattleSession.SavePosition(transform.position);

                // IMPORTANTE: no llamar también a SceneTransitionManager.
                StartCoroutine(StartEncounter());
            }
            else
            {
                SetNextEncounter();
            }
        }
    }

    private void SetNextEncounter()
    {
        stepsUntilEncounter = Random.Range(minSteps, maxSteps + 1);
    }

    private IEnumerator StartEncounter()
    {
        if (screenTransition == null)
        {
            Debug.LogError("ScreenTransition no está asignado.");
            encounterStarted = false;
            yield break;
        }

        yield return screenTransition.CoverScreen();

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(battleSceneName);

        if (operation == null)
        {
            Debug.LogError("No se pudo cargar: " + battleSceneName);
            encounterStarted = false;
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return screenTransition.RevealScreen();
    }
}