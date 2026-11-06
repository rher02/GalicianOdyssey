
using UnityEngine;

[System.Serializable]
public class BattleEnemy
{
    public EnemyData data;

    public int currentHP;
    public bool isDefending;

    public bool IsDead => currentHP <= 0;

    public BattleEnemy(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError("BattleEnemy: EnemyData es NULL.");
            data = null;
            currentHP = 0;
            isDefending = false;
            return;
        }

        data = enemyData;
        currentHP = data.maxHP;
        isDefending = false;
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }
}
