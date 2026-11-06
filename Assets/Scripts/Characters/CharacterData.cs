using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public CharacterClassData characterClass;

    [Header("Estadísticas actuales")]
    public int level;
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;
    public int attack;
    public int magic;
    public int defense;
    public int evasion;

    [Header("Equipamiento")]
    public EquipmentData weapon;
    public EquipmentData armor;

    [Header("Estado de combate")]
    public bool defending;

    [Header("Habilidades")]
    public List<SkillData> skills = new List<SkillData>();

    public CharacterData(CharacterClassData classData)
    {
        characterClass = classData;

        level = 1;

        maxHP = classData.maxHP;
        currentHP = maxHP;

        maxMP = classData.maxMP;
        currentMP = maxMP;

        defending = false;

        attack = classData.attack;
        magic = classData.magic;
        defense = classData.defense;
        evasion = classData.evade;

        weapon = null;
        armor = null;
        
        skills = new List<SkillData>();

        if (classData.skills != null)
        {
            skills.AddRange(classData.skills);
        }
    }

    public bool LearnSkill(SkillData skill)
    {
        if (skill == null || skills.Count >= 3)
            return false;

        if (skills.Contains(skill))
            return false;

        skills.Add(skill);
        return true;
    }
}
