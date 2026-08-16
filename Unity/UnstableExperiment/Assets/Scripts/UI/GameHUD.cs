using UnityEngine;
using UnstableExperiment.Combat;
using UnstableExperiment.Core;
using UnstableExperiment.Data;
using UnstableExperiment.World;

namespace UnstableExperiment.UI
{
    public class GameHUD : MonoBehaviour
    {
        private RoomManager _rooms;
        private CombatManager _combat;
        private GUIStyle _box;
        private GUIStyle _label;
        private bool _stylesReady;

        public void Initialize(RoomManager rooms, CombatManager combat)
        {
            _rooms = rooms;
            _combat = combat;
        }

        public void Refresh() { }

        private void InitStyles()
        {
            if (_stylesReady) return;
            _box = new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.UpperLeft };
            _label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            _stylesReady = true;
        }

        private void OnGUI()
        {
            InitStyles();
            var state = GameState.Instance;

            GUILayout.BeginArea(new Rect(10, 10, 420, 140), _box);
            GUILayout.Label($"Subject 07 · HP {state.PlayerHp}/{state.PlayerMaxHp} · Sector {state.CurrentSectorId}", _label);
            var room = GameDatabase.GetRoom(state.CurrentRoomId);
            GUILayout.Label($"Комната: {room.nameRu} ({room.id})", _label);
            if (state.Keys.Count > 0)
                GUILayout.Label($"Ключи: {string.Join(", ", state.Keys)}", _label);
            if (_rooms.StatusMessage != null)
                GUILayout.Label(_rooms.StatusMessage, _label);
            GUILayout.Label("WASD — ход · E — дверь/лут · Tab — карта", _label);
            GUILayout.EndArea();

            if (_combat != null && _combat.IsActive)
                DrawCombatUI();
        }

        private void DrawCombatUI()
        {
            GUILayout.BeginArea(new Rect(10, 160, Screen.width - 20, Screen.height - 180), _box);
            GUILayout.Label($"⚔ {_combat.EnemyName}  HP {_combat.EnemyHp}  |  You HP {_combat.PlayerHp} Block {_combat.PlayerBlock} Poison {_combat.PlayerPoison}", _label);
            GUILayout.Label($"Energy {_combat.Energy} · Turn {_combat.Turn} · Roll left {_combat.RollRemaining}", _label);
            GUILayout.Label(_combat.LastLog, _label);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < _combat.GetHandCount(); i++)
            {
                if (GUILayout.Button($"REDEEM [{i+1}] {_combat.GetHandLabel(i)}", GUILayout.Height(36)))
                    _combat.RedeemTicket(i);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("End Turn", GUILayout.Height(32)))
                _combat.EndPlayerTurn();

            GUILayout.EndArea();
        }
    }
}
