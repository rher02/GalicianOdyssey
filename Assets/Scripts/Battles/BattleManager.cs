
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    [Header("Escenas")]
    public string mapSceneName = "MapScene";
    public string defeatSceneName = "GameOverScene";

    [Header("Enemigos")]
    public EnemyData[] possibleEnemies;
    public int minEnemies = 1;
    public int maxEnemies = 3;

    [Header("Interfaz")]
    public BattleCharacterUI[] partySlots;
    public BattleEnemyUI[] enemySlots;

    [Header("Texto inferior")]
    public TMP_Text battleLog;

    [Header("Paneles")]
    public GameObject resultPanel;
    public TMP_Text resultText;

    private List<CharacterData> party = new List<CharacterData>();
    private List<BattleEnemy> enemies = new List<BattleEnemy>();

    private int currentCharacterIndex;
    private int selectedEnemyIndex;

    private bool battleFinished;
    private bool waitingForEnemy;

    // Sistema de selección de acciones y objetivos.
    private bool selectingTarget;
    private int pendingActionKey;
    private int pendingSkillIndex = -1;

    private CharacterData CurrentCharacter =>
        party[currentCharacterIndex];

    private void Start()
    {
        Debug.Log("ESCENA DE BATALLA: " +
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        Debug.Log("PARTYMANAGER: " +
            (PartyManager.Instance == null ? "NULL" : "EXISTE"));

        if (PartyManager.Instance == null)
        {
            Debug.LogError("NO EXISTE PARTYMANAGER");
            return;
        }

        Debug.Log("Personajes en el equipo: " +
            PartyManager.Instance.party.Count);

        foreach (CharacterData character in PartyManager.Instance.party)
        {
            if (character != null && character.characterClass != null)
                Debug.Log("Personaje: " + character.characterClass.className);
        }

        StartBattle();
    }

    private void Update()
    {
        if (battleFinished || waitingForEnemy)
            return;

        if (party.Count == 0 ||
            currentCharacterIndex >= party.Count)
            return;

        if (CurrentCharacter.currentHP <= 0)
            return;

        // --------------------------------
        // MODO SELECCIÓN DE ENEMIGO
        // --------------------------------

        if (selectingTarget)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.A))
            {
                ChangeTarget(-1);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.D))
            {
                ChangeTarget(1);
            }

            // Confirmar ataque.
            if (Input.GetKeyDown(KeyCode.Alpha1) ||
                Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (pendingActionKey == 1)
                    ConfirmSelectedAction();
            }

            // Confirmar habilidad 1.
            if (Input.GetKeyDown(KeyCode.Alpha2) ||
                Input.GetKeyDown(KeyCode.Keypad2))
            {
                if (pendingActionKey == 2)
                    ConfirmSelectedAction();
            }

            // Confirmar habilidad 2.
            if (Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Keypad3))
            {
                if (pendingActionKey == 3)
                    ConfirmSelectedAction();
            }

            // Confirmar habilidad 3.
            if (Input.GetKeyDown(KeyCode.Alpha4) ||
                Input.GetKeyDown(KeyCode.Keypad4))
            {
                if (pendingActionKey == 4)
                    ConfirmSelectedAction();
            }

            // Cancelar selección.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTargetSelection();
            }

            return;
        }

        // --------------------------------
        // MODO NORMAL: ELEGIR ACCIÓN
        // --------------------------------

        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            BeginTargetSelection(1, -1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4))
        {
            UseSkill(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            Defend();
        }
    }

    // ====================================
    // INICIO DEL COMBATE
    // ====================================

    private void StartBattle()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("PartyManager no encontrado.");
            return;
        }

        party = PartyManager.Instance.party
            .Where(c => c != null)
            .Take(3)
            .ToList();

        if (party.Count == 0 ||
            party.All(c => c.currentHP <= 0))
        {
            Debug.LogError("No hay protagonistas disponibles.");
            return;
        }

        if (possibleEnemies == null ||
            possibleEnemies.Length == 0)
        {
            Debug.LogError("No hay enemigos configurados.");
            return;
        }

        if (enemySlots == null || enemySlots.Length < 3 ||
            partySlots == null || partySlots.Length < 3)
        {
            Debug.LogError("Necesitas tres espacios de protagonistas y enemigos.");
            return;
        }

        battleFinished = false;
        waitingForEnemy = false;
        selectingTarget = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        CreateEnemies();

        if (enemies.Count == 0)
        {
            Debug.LogError("No se han podido crear enemigos.");
            return;
        }

        currentCharacterIndex = 0;
        selectedEnemyIndex = 0;

        RefreshUI();

        Log("¡Un grupo de enemigos aparece!");

        StartPlayerTurn();
    }

    private void CreateEnemies()
    {
        enemies.Clear();

        int minimum = Mathf.Clamp(minEnemies, 1, 3);
        int maximum = Mathf.Clamp(maxEnemies, minimum, 3);

        int amount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < amount; i++)
        {
            EnemyData data = possibleEnemies[
                Random.Range(0, possibleEnemies.Length)
            ];

            if (data == null)
            {
                Debug.LogError("Hay un EnemyData sin asignar.");
                continue;
            }

            enemies.Add(new BattleEnemy(data));
        }

        for (int i = 0; i < enemySlots.Length; i++)
        {
            if (enemySlots[i] != null)
                enemySlots[i].gameObject.SetActive(i < enemies.Count);
        }
    }

    // ====================================
    // TURNOS DE LOS PERSONAJES
    // ====================================

    private void StartPlayerTurn()
    {
        if (CheckBattleEnd())
            return;

        while (currentCharacterIndex < party.Count &&
               party[currentCharacterIndex].currentHP <= 0)
        {
            currentCharacterIndex++;
        }

        if (currentCharacterIndex >= party.Count)
        {
            StartCoroutine(EnemyTurn());
            return;
        }

        selectingTarget = false;
        pendingActionKey = 0;
        pendingSkillIndex = -1;

        RefreshUI();

        Log(
            "Turno de " +
            CurrentCharacter.characterClass.className +
            " | 1 Atacar | 2-4 Habilidades | 5 Defender"
        );
    }

    // ====================================
    // SELECCIÓN DE OBJETIVOS
    // ====================================

    private void BeginTargetSelection(int actionKey, int skillIndex)
    {
        if (!enemies.Any(e => e != null && !e.IsDead))
        {
            Log("No hay enemigos disponibles.");
            return;
        }

        pendingActionKey = actionKey;
        pendingSkillIndex = skillIndex;

        selectingTarget = true;

        if (selectedEnemyIndex < 0 ||
            selectedEnemyIndex >= enemies.Count ||
            enemies[selectedEnemyIndex] == null ||
            enemies[selectedEnemyIndex].IsDead)
        {
            selectedEnemyIndex = enemies.FindIndex(
                e => e != null && !e.IsDead
            );
        }

        RefreshUI();

        Log(
            "Selecciona un enemigo con A/D o las flechas. " +
            "Pulsa " + actionKey + " para confirmar. Escape para cancelar."
        );
    }

    private void ChangeTarget(int direction)
    {
        if (enemies.Count == 0)
            return;

        int attempts = 0;

        do
        {
            selectedEnemyIndex += direction;

            if (selectedEnemyIndex < 0)
                selectedEnemyIndex = enemies.Count - 1;

            if (selectedEnemyIndex >= enemies.Count)
                selectedEnemyIndex = 0;

            attempts++;

        } while (
            (enemies[selectedEnemyIndex] == null ||
             enemies[selectedEnemyIndex].IsDead) &&
            attempts < enemies.Count
        );

        RefreshUI();

        if (HasValidTarget())
        {
            Log(
                "Objetivo: " +
                enemies[selectedEnemyIndex].data.enemyName
            );
        }
    }

    private void ConfirmSelectedAction()
    {
        if (!HasValidTarget())
            return;

        if (pendingSkillIndex == -1)
        {
            NormalAttack();
        }
        else
        {
            ConfirmSkillSelection();
        }
    }

    private void CancelTargetSelection()
    {
        selectingTarget = false;
        pendingActionKey = 0;
        pendingSkillIndex = -1;

        RefreshUI();

        Log("Turno de " +
            CurrentCharacter.characterClass.className);
    }

    // ====================================
    // ATAQUE NORMAL
    // ====================================

    private void NormalAttack()
    {
        if (!HasValidTarget())
            return;

        int targetIndex = selectedEnemyIndex;
        BattleEnemy enemy = enemies[targetIndex];

        int damage = CalculateDamage(
            CurrentCharacter.attack,
            enemy.data.defense
        );

        enemy.TakeDamage(damage);

        Log(
            CurrentCharacter.characterClass.className +
            " ataca a " + enemy.data.enemyName +
            " causando " + damage + " de daño."
        );

        selectingTarget = false;
        pendingActionKey = 0;
        pendingSkillIndex = -1;

        StartCoroutine(ResolveNormalAttack(targetIndex, damage));
    }

    private IEnumerator ResolveNormalAttack(int targetIndex, int damage)
    {
        waitingForEnemy = true;

        if (targetIndex < enemySlots.Length &&
            enemySlots[targetIndex] != null)
        {
            enemySlots[targetIndex].UpdateHealthText(
                enemies[targetIndex]
            );

            yield return StartCoroutine(
                enemySlots[targetIndex].PlayHitEffect(damage)
            );
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        RefreshUI();

        waitingForEnemy = false;

        EndPlayerAction();
    }

    // ====================================
    // HABILIDADES
    // ====================================

    private void UseSkill(int skillIndex)
    {
        CharacterData character = CurrentCharacter;

        if (character.skills == null ||
            skillIndex < 0 ||
            skillIndex >= character.skills.Count)
        {
            Log("Esta habilidad no está disponible.");
            return;
        }

        SkillData skill = character.skills[skillIndex];

        if (skill == null)
        {
            Log("Habilidad no válida.");
            return;
        }

        BeginTargetSelection(skillIndex + 2, skillIndex);
    }

    private void ConfirmSkillSelection()
    {
        SkillData skill = CurrentCharacter.skills[pendingSkillIndex];

        BattleEnemy enemy = enemies[selectedEnemyIndex];

        // Los efectos reales de SkillData todavía no están implementados.
        Log(
            "Objetivo seleccionado: " + enemy.data.enemyName +
            ". La habilidad " + skill.name +
            " todavía necesita implementar su efecto."
        );

        selectingTarget = false;
        pendingActionKey = 0;
        pendingSkillIndex = -1;

        RefreshUI();
    }

    // ====================================
    // DEFENSA
    // ====================================

    private void Defend()
    {
        CurrentCharacter.defending = true;

        Log(
            CurrentCharacter.characterClass.className +
            " se prepara para defenderse."
        );

        EndPlayerAction();
    }

    // ====================================
    // CÁLCULOS
    // ====================================

    private bool HasValidTarget()
    {
        if (enemies.Count == 0)
            return false;

        if (selectedEnemyIndex < 0 ||
            selectedEnemyIndex >= enemies.Count)
            return false;

        if (enemies[selectedEnemyIndex] == null ||
            enemies[selectedEnemyIndex].IsDead ||
            enemies[selectedEnemyIndex].data == null)
        {
            Log("Selecciona un enemigo vivo.");
            return false;
        }

        return true;
    }

    private int CalculateDamage(int attack, int defense)
    {
        return Mathf.Max(1, attack - defense / 2);
    }

    private void EndPlayerAction()
    {
        selectingTarget = false;
        pendingActionKey = 0;
        pendingSkillIndex = -1;

        if (CheckBattleEnd())
            return;

        currentCharacterIndex++;

        StartPlayerTurn();
    }

    // ====================================
    // TURNO DE LOS ENEMIGOS
    // ====================================

    private IEnumerator EnemyTurn()
    {
        waitingForEnemy = true;

        Log("Turno de los enemigos.");

        yield return new WaitForSeconds(0.7f);

        foreach (BattleEnemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead || enemy.data == null)
                continue;

            List<CharacterData> aliveParty = party
                .Where(c => c != null && c.currentHP > 0)
                .ToList();

            if (aliveParty.Count == 0)
                break;

            CharacterData target = aliveParty[
                Random.Range(0, aliveParty.Count)
            ];

            int damage = CalculateDamage(
                enemy.data.attack,
                target.defense
            );

            if (target.defending)
                damage = Mathf.Max(1, damage / 2);

            target.currentHP = Mathf.Max(
                0,
                target.currentHP - damage
            );

            Log(
                enemy.data.enemyName +
                " ataca a " +
                target.characterClass.className +
                " causando " + damage + " de daño."
            );

            if (target.currentHP <= 0)
            {
                Log(target.characterClass.className + " ha caído.");
            }

            RefreshUI();

            if (CheckBattleEnd())
                yield break;

            yield return new WaitForSeconds(0.8f);
        }

        foreach (CharacterData character in party)
        {
            if (character != null)
                character.defending = false;
        }

        currentCharacterIndex = 0;
        waitingForEnemy = false;

        StartPlayerTurn();
    }

    // ====================================
    // ACTUALIZAR INTERFAZ
    // ====================================

    private void RefreshUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (partySlots[i] == null)
                continue;

            bool active = i < party.Count;

            partySlots[i].gameObject.SetActive(active);

            if (active)
            {
                partySlots[i].UpdateUI(
                    party[i],
                    i == currentCharacterIndex
                );
            }
        }

        for (int i = 0; i < enemySlots.Length; i++)
        {
            if (enemySlots[i] == null)
                continue;

            if (i >= enemies.Count ||
                enemies[i] == null ||
                enemies[i].IsDead)
            {
                enemySlots[i].gameObject.SetActive(false);
                continue;
            }

            enemySlots[i].gameObject.SetActive(true);

            enemySlots[i].UpdateUI(
                enemies[i],
                selectingTarget && i == selectedEnemyIndex
            );
        }
    }

    // ====================================
    // FIN DEL COMBATE
    // ====================================

    private bool CheckBattleEnd()
    {
        if (battleFinished)
            return true;

        bool victory = enemies.All(e => e == null || e.IsDead);
        bool defeat = party.All(c => c == null || c.currentHP <= 0);

        if (victory)
        {
            EndBattle(true);
            return true;
        }

        if (defeat)
        {
            EndBattle(false);
            return true;
        }

        return false;
    }

    private void EndBattle(bool victory)
    {
        battleFinished = true;
        waitingForEnemy = true;

        if (victory)
        {
            Log("¡VICTORIA!");
            StartCoroutine(ReturnToMap());
        }
        else
        {
            Log("DERROTA...");
            StartCoroutine(GoToDefeatScene());
        }
    }

    private IEnumerator ReturnToMap()
    {
        yield return new WaitForSeconds(1.2f);

        SceneTransitionManager.Instance.ChangeScene(
            mapSceneName
        );
    }

    private IEnumerator GoToDefeatScene()
    {
        yield return new WaitForSeconds(1.2f);

        SceneTransitionManager.Instance.ChangeScene(
            defeatSceneName
        );
    }

    private void Log(string message)
    {
        if (battleLog != null)
            battleLog.text = message;
    }
}
