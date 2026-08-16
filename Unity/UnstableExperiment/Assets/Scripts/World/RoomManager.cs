using System.Collections.Generic;
using UnityEngine;
using UnstableExperiment.Combat;
using UnstableExperiment.Core;
using UnstableExperiment.Data;
using UnstableExperiment.UI;

namespace UnstableExperiment.World
{
    public class RoomManager : MonoBehaviour
    {
        public const float TileSize = 1f;

        private Transform _roomRoot;
        private Transform _player;
        private CombatManager _combat;
        private GameHUD _hud;
        private readonly List<EnemyController> _enemies = new();
        private readonly List<LootPickup> _loot = new();
        private readonly List<DoorInteractable> _doors = new();
        private string _message;
        private float _messageUntil;

        public string StatusMessage =>
            Time.time < _messageUntil ? _message : null;

        public void Initialize(CombatManager combat, GameHUD hud)
        {
            _combat = combat;
            _hud = hud;
            _roomRoot = new GameObject("RoomRoot").transform;
            _roomRoot.SetParent(transform);
        }

        public void ShowMessage(string text, float seconds = 3f)
        {
            _message = text;
            _messageUntil = Time.time + seconds;
        }

        public void EnterRoom(string roomId, string entryDoorId)
        {
            if (_combat != null && _combat.IsActive) return;

            var state = GameState.Instance;
            var room = GameDatabase.GetRoom(roomId);
            var sector = GameDatabase.GetSectorForRoom(roomId);

            state.CurrentRoomId = roomId;
            state.CurrentSectorId = sector.id;
            state.EntryDoorId = entryDoorId;
            state.VisitedRooms.Add(roomId);

            ClearRoom();
            ProceduralRoomBuilder.Build(room, _roomRoot, _doors, TileSize);
            SpawnPlayer(room, entryDoorId);
            SpawnEnemies(room);
            SpawnLoot(room);

            if (room.type == "rest" && room.restOnce && !state.CollectedLoot.Contains(room.id + "_rest"))
            {
                state.PlayerHp = Mathf.Min(state.PlayerMaxHp, state.PlayerHp + room.restHeal);
                state.CollectedLoot.Add(room.id + "_rest");
                ShowMessage($"+{room.restHeal} HP (костёр)", 2.5f);
            }

            _hud?.Refresh();
        }

        private void ClearRoom()
        {
            foreach (var e in _enemies) if (e) Destroy(e.gameObject);
            foreach (var l in _loot) if (l) Destroy(l.gameObject);
            foreach (var d in _doors) if (d) Destroy(d.gameObject);
            _enemies.Clear();
            _loot.Clear();
            _doors.Clear();
            if (_player) Destroy(_player.gameObject);
            for (int i = _roomRoot.childCount - 1; i >= 0; i--)
                Destroy(_roomRoot.GetChild(i).gameObject);
        }

        private void SpawnPlayer(RoomDef room, string entryDoorId)
        {
            var go = new GameObject("Subject07");
            go.transform.SetParent(_roomRoot);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtLibrary.Player;
            sr.sortingOrder = 10;
            FitCharacterScale(go.transform, 0.9f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;

            go.AddComponent<PlayerMovement>();
            _player = go.transform;
            _player.position = ProceduralRoomBuilder.GetSpawnPosition(room, entryDoorId, TileSize);
            SetupCamera(_player);
        }

        private void SetupCamera(Transform target)
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.Target = target;
        }

        public static void FitCharacterScale(Transform t, float targetHeight)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            float h = sr.sprite.bounds.size.y;
            if (h <= 0.01f) return;
            float s = targetHeight / h;
            t.localScale = new Vector3(s, s, 1f);
        }

        private void SpawnEnemies(RoomDef room)
        {
            if (room.spawns == null) return;
            int idx = 0;
            foreach (var spawn in room.spawns)
            {
                for (int i = 0; i < spawn.count; i++)
                {
                    var pos = ProceduralRoomBuilder.GetEnemyPosition(room, idx++, TileSize);
                    var enemy = EnemyController.Create(_roomRoot, spawn.enemyId, spawn.blockId, pos, this, _combat);
                    _enemies.Add(enemy);
                }
            }
        }

        private void SpawnLoot(RoomDef room)
        {
            if (room.loot == null) return;
            var state = GameState.Instance;
            for (int i = 0; i < room.loot.Length; i++)
            {
                var loot = room.loot[i];
                if (state.CollectedLoot.Contains(loot.id)) continue;
                var pos = ProceduralRoomBuilder.GetLootPosition(room, i, TileSize);
                var pickup = LootPickup.Create(_roomRoot, loot, pos, this);
                _loot.Add(pickup);
            }
        }

        public void TryUseNearbyDoor()
        {
            if (_player == null) return;
            DoorInteractable best = null;
            float bestDist = 1.2f;
            foreach (var door in _doors)
            {
                if (door == null) continue;
                float d = Vector2.Distance(_player.position, door.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = door;
                }
            }

            best?.TryEnter();
        }

        public void TryPickupNearby()
        {
            if (_player == null) return;
            foreach (var loot in _loot)
            {
                if (loot == null || loot.Picked) continue;
                if (Vector2.Distance(_player.position, loot.transform.position) < 0.8f)
                {
                    loot.Pickup();
                    return;
                }
            }
        }

        public void OnEnemyDefeated(EnemyController enemy)
        {
            _enemies.Remove(enemy);
            var state = GameState.Instance;
            state.MarkBlockCleared(enemy.BlockId);
            ShowMessage($"Победа: {enemy.DisplayName}", 2f);
        }

        public void OnLootCollected(LootPickup loot)
        {
            _loot.Remove(loot);
            _hud?.Refresh();
        }

        public void RequestDoorTravel(DoorDef doorDef)
        {
            var state = GameState.Instance;
            if (!state.CanUseDoor(doorDef, out var reason))
            {
                ShowMessage(reason, 2.5f);
                return;
            }

            if (!string.IsNullOrEmpty(doorDef.sectorTransition))
            {
                var fromSector = GameDatabase.GetSectorForRoom(state.CurrentRoomId);
                if (!string.IsNullOrEmpty(fromSector.transitionVoiceLine))
                    ShowMessage(fromSector.transitionVoiceLine, 4f);
            }

            var target = GameDatabase.GetRoom(doorDef.targetRoom);
            string reverseDoor = FindReverseDoorId(target, state.CurrentRoomId);
            EnterRoom(doorDef.targetRoom, reverseDoor);

            var newSector = GameDatabase.GetSectorForRoom(doorDef.targetRoom);
            if (newSector.id != "A" && !string.IsNullOrEmpty(newSector.mapItemId))
                state.UnlockedMaps.Add(newSector.mapItemId);
        }

        private static string FindReverseDoorId(RoomDef targetRoom, string fromRoomId)
        {
            if (targetRoom.doors == null) return null;
            foreach (var d in targetRoom.doors)
                if (d.targetRoom == fromRoomId)
                    return d.id;
            return null;
        }

        public Transform Player => _player;
    }
}
