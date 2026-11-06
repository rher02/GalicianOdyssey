using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "DRPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Información")]
    public string enemyName;
    public Sprite portrait;

    [Header("Estadísticas")]
    public int maxHP = 100;
    public int attack = 10;
    public int defense = 5;
}