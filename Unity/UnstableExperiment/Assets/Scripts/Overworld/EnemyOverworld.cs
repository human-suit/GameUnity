using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Враг на карте: патрулирует свою территорию, замечает игрока
/// по прямой видимости и начинает бой при столкновении.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyOverworld : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        StartingBattle
    }

    [Header("Враг")]
    [SerializeField] private string enemyId = "test_enemy";
    [SerializeField] private string encounterId = "enemy_main_01";
    [SerializeField] private string battleScene = "Battle";
    [SerializeField] private BattleEnemyDefinition battleDefinition;
    [SerializeField] private Sprite battleBackground;

    [Header("Патруль")]
    [Min(0.1f)]
    [SerializeField] private float patrolRadius = 3f;
    [Min(0.1f)]
    [SerializeField] private float patrolSpeed = 1.2f;
    [Min(0f)]
    [SerializeField] private float patrolPause = 1f;
    [Min(0.1f)]
    [SerializeField] private float patrolPointTimeout = 5f;

    [Header("Обзор и погоня")]
    [Min(0.1f)]
    [SerializeField] private float visionDistance = 5f;
    [Min(0.1f)]
    [SerializeField] private float chaseRadius = 8f;
    [Min(0.1f)]
    [SerializeField] private float chaseSpeed = 2.5f;
    [Min(0f)]
    [SerializeField] private float loseSightDelay = 1.2f;
    [SerializeField] private LayerMask obstacleMask;

    private const float ArrivalDistance = 0.12f;

    private Rigidbody2D _body;
    private PlayerMove _player;
    private Vector2 _patrolCenter;
    private Vector2 _patrolTarget;
    private float _waitUntil;
    private float _patrolTargetExpiresAt;
    private float _lastSeenAt;
    private bool _hasPatrolTarget;
    private EnemyState _state;

    private void Awake()
    {
        if (GameState.IsEnemyDefeated(encounterId))
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        _body = GetComponent<Rigidbody2D>();
        _body.gravityScale = 0f;
        _body.constraints = RigidbodyConstraints2D.FreezeRotation;
        _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _body.interpolation = RigidbodyInterpolation2D.Interpolate;

        _patrolCenter = _body.position;
    }

    private void Start()
    {
        FindPlayer();
        ChoosePatrolTarget();
    }

    private void FixedUpdate()
    {
        if (_state == EnemyState.StartingBattle)
        {
            StopMoving();
            return;
        }

        if (_player == null)
            FindPlayer();

        bool seesPlayer = CanSeePlayer();
        if (seesPlayer)
        {
            _lastSeenAt = Time.time;
            _state = EnemyState.Chase;
        }
        else if (_state == EnemyState.Chase &&
                 Time.time - _lastSeenAt >= loseSightDelay)
        {
            _state = EnemyState.Patrol;
            ChoosePatrolTarget();
        }

        if (_state == EnemyState.Chase && _player != null)
            MoveTowards(_player.transform.position, chaseSpeed);
        else
            Patrol();
    }

    private void FindPlayer()
    {
        _player = FindObjectOfType<PlayerMove>();
    }

    private void Patrol()
    {
        if (!_hasPatrolTarget)
        {
            StopMoving();

            if (Time.time >= _waitUntil)
                ChoosePatrolTarget();

            return;
        }

        if (Time.time >= _patrolTargetExpiresAt)
        {
            ChoosePatrolTarget();
            return;
        }

        if (Vector2.Distance(_body.position, _patrolTarget) <= ArrivalDistance)
        {
            _hasPatrolTarget = false;
            _waitUntil = Time.time + patrolPause;
            StopMoving();
            return;
        }

        MoveTowards(_patrolTarget, patrolSpeed);
    }

    private void ChoosePatrolTarget()
    {
        _patrolTarget = _patrolCenter + Random.insideUnitCircle * patrolRadius;
        _patrolTargetExpiresAt = Time.time + patrolPointTimeout;
        _hasPatrolTarget = true;
    }

    private bool CanSeePlayer()
    {
        if (_player == null)
            return false;

        Vector2 playerPosition = _player.transform.position;

        if (Vector2.Distance(_patrolCenter, playerPosition) > chaseRadius)
            return false;

        if (Vector2.Distance(_body.position, playerPosition) > visionDistance)
            return false;

        RaycastHit2D wall = Physics2D.Linecast(
            _body.position,
            playerPosition,
            obstacleMask);

        return wall.collider == null;
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = target - _body.position;

        if (direction.sqrMagnitude <= ArrivalDistance * ArrivalDistance)
        {
            StopMoving();
            return;
        }

        _body.velocity = direction.normalized * speed;
    }

    private void StopMoving()
    {
        _body.velocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMove player = collision.collider.GetComponent<PlayerMove>();
        if (player == null)
            player = collision.collider.GetComponentInParent<PlayerMove>();

        if (player != null)
            StartBattle();
    }

    private void StartBattle()
    {
        if (_state == EnemyState.StartingBattle)
            return;

        if (!Application.CanStreamedLevelBeLoaded(battleScene))
        {
            Debug.LogError(
                $"EnemyOverworld: scene '{battleScene}' is not in Build Settings.",
                this);
            return;
        }

        _state = EnemyState.StartingBattle;
        StopMoving();
        PlayerMove.Frozen = true;
        BattleEncounterData.Begin(
            enemyId,
            encounterId,
            _player.transform.position,
            battleDefinition,
            battleBackground);
        SceneManager.LoadScene(battleScene);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying
            ? (Vector3)_patrolCenter
            : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, chaseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
#endif
}
