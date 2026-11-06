
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    public List<CharacterData> party = new List<CharacterData>();

    // Crear el PartyManager antes de cargar la primera escena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreatePartyManager()
    {
        if (Instance != null)
            return;

        GameObject manager = new GameObject("PartyManager");
        manager.AddComponent<PartyManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        Debug.Log("PARTYMANAGER CREADO Y PERSISTENTE");
    }

    public void SetParty(List<CharacterData> characters)
    {
        party = new List<CharacterData>(characters);

        Debug.Log("EQUIPO GUARDADO: " + party.Count);
    }
}
