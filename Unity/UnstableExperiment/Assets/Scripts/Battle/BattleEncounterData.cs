using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Данные врага, с которым игрок столкнулся на карте.
/// Сохраняются между загрузками сцен.
/// </summary>
public static class BattleEncounterData
{
    public static string EnemyId { get; private set; }
    public static string EncounterId { get; private set; }
    public static string SourceScene { get; private set; }
    public static Vector2 ReturnPosition { get; private set; }
    public static bool ReturnPending { get; private set; }

    public static bool HasEncounter => !string.IsNullOrEmpty(EnemyId);

    public static void Begin(
        string enemyId,
        string encounterId,
        Vector2 returnPosition)
    {
        EnemyId = string.IsNullOrWhiteSpace(enemyId) ? "enemy" : enemyId;
        EncounterId = encounterId;
        SourceScene = SceneManager.GetActiveScene().name;
        ReturnPosition = returnPosition;
        ReturnPending = false;
    }

    public static void QueueReturn()
    {
        ReturnPending = !string.IsNullOrEmpty(SourceScene);
    }

    public static bool TryConsumeReturnPosition(out Vector2 position)
    {
        position = ReturnPosition;
        if (!ReturnPending)
            return false;

        Clear();
        return true;
    }

    public static void Clear()
    {
        EnemyId = null;
        EncounterId = null;
        SourceScene = null;
        ReturnPosition = Vector2.zero;
        ReturnPending = false;
    }
}
