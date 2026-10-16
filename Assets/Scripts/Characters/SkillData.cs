using UnityEngine;

public enum SkillEffectType
{
    Damage,
    DamageOverTime,
    Heal,
    Buff,
    Debuff,
    Revive
}

public enum SkillTarget
{
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies,
    Self
}

[System.Serializable]
public class StatModifier
{
    public int attack;
    public int magic;
    public int defense;
    public int evade;
}

[CreateAssetMenu(fileName = "NewSkill",
    menuName = "DRPG/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Información")]
    public string skillName;
    public Sprite icon;

    [TextArea]
    public string description;

    [Header("Coste")]
    public int mpCost = 5;

    [Header("Efecto")]
    public SkillEffectType effectType;
    public SkillTarget target;

    [Header("Potencia")]
    public int power = 10;

    [Header("Modificadores de estadísticas")]
    public StatModifier statModifier;

    [Header("Duración en turnos")]
    public int duration = 3;
}