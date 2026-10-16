using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass",
    menuName = "DRPG/Character Class")]
public class CharacterClassData : ScriptableObject
{
    [Header("Información")]
    public string className;
    public Sprite portrait;
    public Sprite partyIcon;

    [Header("Grupo y habilidades")]
    public SkillData[] skills;

    [Header("Estadísticas iniciales")]
    public int maxHP = 100;
    public int maxMP = 30;
    public int attack = 10;
    public int magic = 10;
    public int defense = 10;
    public int evade = 5;
}
