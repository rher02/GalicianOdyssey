using UnityEngine;

public static class BattleSession
{
    public static Vector3 mapPosition;
    public static bool hasSavedPosition;

    public static void SavePosition(Vector3 position)
    {
        mapPosition = position;
        hasSavedPosition = true;
    }

    public static Vector3 GetSavedPosition()
    {
        return mapPosition;
    }
}
