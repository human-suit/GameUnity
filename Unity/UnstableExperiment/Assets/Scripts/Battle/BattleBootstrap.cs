using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Простой пошаговый бой: игрок атакует, затем отвечает враг.
/// </summary>
public class BattleBootstrap : MonoBehaviour
{
    private enum BattleState
    {
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat,
        Invalid
    }

    private const int PlayerMinDamage = 6;
    private const int PlayerMaxDamage = 10;

    private EnemyBattleStats _enemy;
    private int _enemyHealth;
    private BattleState _state;
    private string _message;
    private bool _leavingScene;

    private void Awake()
    {
        PlayerMove.Frozen = false;

        if (!BattleEncounterData.HasEncounter)
        {
            _state = BattleState.Invalid;
            _message = "Нет данных о противнике.";
            Debug.LogWarning("Battle opened without encounter data.");
            return;
        }

        _enemy = EnemyBattleDatabase.Get(BattleEncounterData.EnemyId);
        _enemyHealth = _enemy.MaxHealth;
        _state = BattleState.PlayerTurn;
        _message = $"На пути появляется {_enemy.DisplayName}.";

        Debug.Log($"Battle started. Enemy: {BattleEncounterData.EnemyId}");
    }

    private void OnGUI()
    {
        const float width = 520f;
        const float height = 300f;
        float left = (Screen.width - width) * 0.5f;
        float top = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(left, top, width, height), "БОЙ");

        if (_state == BattleState.Invalid)
        {
            GUI.Label(new Rect(left + 30f, top + 60f, 460f, 30f), _message);

            if (GUI.Button(
                    new Rect(left + 160f, top + 220f, 200f, 45f),
                    "В главное меню"))
            {
                ReturnToMainMenu();
            }

            return;
        }

        GUI.Label(
            new Rect(left + 30f, top + 50f, 460f, 25f),
            $"Игрок: {GameState.PlayerHealth} / {GameState.PlayerMaxHealth} HP");
        GUI.Label(
            new Rect(left + 30f, top + 80f, 460f, 25f),
            $"{_enemy.DisplayName}: {_enemyHealth} / {_enemy.MaxHealth} HP");
        GUI.Label(
            new Rect(left + 30f, top + 125f, 460f, 45f),
            _message);

        switch (_state)
        {
            case BattleState.PlayerTurn:
                GUI.Label(
                    new Rect(left + 30f, top + 175f, 460f, 25f),
                    "Ваш ход");

                if (GUI.Button(
                        new Rect(left + 160f, top + 220f, 200f, 45f),
                        "Атаковать"))
                {
                    PlayerAttack();
                }
                break;

            case BattleState.EnemyTurn:
                GUI.Label(
                    new Rect(left + 30f, top + 175f, 460f, 25f),
                    "Ход противника...");
                break;

            case BattleState.Victory:
                GUI.Label(
                    new Rect(left + 30f, top + 175f, 460f, 25f),
                    "Победа");

                if (GUI.Button(
                        new Rect(left + 160f, top + 220f, 200f, 45f),
                        "Вернуться на карту"))
                {
                    FinishVictory();
                }
                break;

            case BattleState.Defeat:
                GUI.Label(
                    new Rect(left + 30f, top + 175f, 460f, 25f),
                    "Вы погибли");

                if (GUI.Button(
                        new Rect(left + 160f, top + 220f, 200f, 45f),
                        "В главное меню"))
                {
                    ReturnToMainMenu();
                }
                break;
        }
    }

    private void PlayerAttack()
    {
        if (_state != BattleState.PlayerTurn)
            return;

        int damage = Random.Range(PlayerMinDamage, PlayerMaxDamage + 1);
        _enemyHealth = Mathf.Max(0, _enemyHealth - damage);
        _message = $"Вы наносите {damage} урона.";

        if (_enemyHealth <= 0)
        {
            _state = BattleState.Victory;
            _message = $"{_enemy.DisplayName} повержен.";
            return;
        }

        _state = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.8f);

        if (_state != BattleState.EnemyTurn)
            yield break;

        int damage = Random.Range(_enemy.MinDamage, _enemy.MaxDamage + 1);
        GameState.DamagePlayer(damage);
        _message = $"{_enemy.DisplayName} наносит {damage} урона.";

        if (GameState.PlayerHealth <= 0)
        {
            _state = BattleState.Defeat;
            yield break;
        }

        yield return new WaitForSeconds(0.45f);

        if (_state == BattleState.EnemyTurn)
            _state = BattleState.PlayerTurn;
    }

    private void FinishVictory()
    {
        if (_leavingScene)
            return;

        _leavingScene = true;
        GameState.MarkEnemyDefeated(BattleEncounterData.EncounterId);

        string sourceScene = BattleEncounterData.SourceScene;
        if (string.IsNullOrEmpty(sourceScene) ||
            !Application.CanStreamedLevelBeLoaded(sourceScene))
        {
            Debug.LogError($"Battle: cannot return to scene '{sourceScene}'.");
            BattleEncounterData.Clear();
            SceneManager.LoadScene("MainMenu");
            return;
        }

        BattleEncounterData.QueueReturn();
        SceneManager.LoadScene(sourceScene);
    }

    private void ReturnToMainMenu()
    {
        if (_leavingScene)
            return;

        _leavingScene = true;
        BattleEncounterData.Clear();
        SceneManager.LoadScene("MainMenu");
    }
}
