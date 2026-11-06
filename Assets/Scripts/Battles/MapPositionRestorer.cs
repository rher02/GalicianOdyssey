using UnityEngine;

public class MapPositionRestorer : MonoBehaviour
{
    private void Start()
    {
        if (BattleSession.hasSavedPosition)
        {
            transform.position = BattleSession.GetSavedPosition();
            BattleSession.hasSavedPosition = false;
        }
    }
}
