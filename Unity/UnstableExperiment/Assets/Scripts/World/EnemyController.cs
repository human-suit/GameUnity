using UnityEngine;
using UnstableExperiment.Combat;
using UnstableExperiment.Core;
using UnstableExperiment.Data;

namespace UnstableExperiment.World
{
    public class EnemyController : MonoBehaviour
    {
        public string EnemyId { get; private set; }
        public string BlockId { get; private set; }
        public string DisplayName { get; private set; }
        public int Hp { get; private set; }

        private RoomManager _rooms;
        private CombatManager _combat;
        private float _aggroRadius = 1.1f;
        private bool _triggered;
        private Transform _player;

        public static EnemyController Create(
            Transform root, string enemyId, string blockId, Vector3 pos,
            RoomManager rooms, CombatManager combat)
        {
            var go = new GameObject($"Enemy_{enemyId}");
            go.transform.SetParent(root);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Enemy;
            sr.sortingOrder = 8;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;

            var e = go.AddComponent<EnemyController>();
            e.EnemyId = enemyId;
            e.BlockId = blockId;
            e.DisplayName = GameDatabase.GetEnemyNameRu(enemyId);
            e.Hp = GameDatabase.GetEnemyHp(enemyId);
            e._rooms = rooms;
            e._combat = combat;
            return e;
        }

        private void Start() => _player = _rooms?.Player;

        private void Update()
        {
            if (_triggered || _combat.IsActive || _player == null) return;
            if (Vector2.Distance(transform.position, _player.position) <= _aggroRadius)
            {
                _triggered = true;
                _combat.StartCombat(this);
            }
        }

        public void OnCombatEnded(bool playerWon)
        {
            if (playerWon)
            {
                _rooms.OnEnemyDefeated(this);
                Destroy(gameObject);
            }
            else
            {
                _triggered = false;
            }
        }
    }
}
