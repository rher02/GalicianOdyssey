using UnityEngine;

public enum EquipmentType
{
    Weapon,
    Armor
}

[CreateAssetMenu(fileName = "NewEquipment",
    menuName = "DRPG/Equipment")]
public class EquipmentData : ScriptableObject
{
    public string equipmentName;
    public EquipmentType equipmentType;
    public Sprite icon;

    [Header("Bonificaciones")]
    public int attackBonus;
    public int magicBonus;
    public int defenseBonus;
}
