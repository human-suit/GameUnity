using System.Text;
using UnityEngine;
using UnstableExperiment.Core;
using UnstableExperiment.Data;
using UnstableExperiment.World;

namespace UnstableExperiment.UI
{
    public class RouteMapUI : MonoBehaviour
    {
        private RoomManager _rooms;
        private bool _visible;
        private GUIStyle _box;
        private GUIStyle _label;
        private bool _stylesReady;
        private Texture2D _routeMapTex;

        public void Initialize(RoomManager rooms)
        {
            _rooms = rooms;
            _routeMapTex = ArtLibrary.RouteMap;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;
            var state = GameState.Instance;
            if (!state.HasMapForCurrentSector)
            {
                _rooms?.ShowMessage("Нужна карта сектора (найди в Доме)", 2.5f);
                return;
            }
            _visible = !_visible;
        }

        private void InitStyles()
        {
            if (_stylesReady) return;
            _box = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.UpperLeft };
            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, normal = { textColor = new Color(0.9f, 0.85f, 0.75f) } };
            _stylesReady = true;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            InitStyles();

            var state = GameState.Instance;
            var sector = GameDatabase.GetSector(state.CurrentSectorId);
            float w = 520;
            float h = Screen.height - 40;
            GUILayout.BeginArea(new Rect(Screen.width - w - 10, 10, w, h), _box);

            if (_routeMapTex != null)
            {
                float imgW = w - 20;
                float aspect = (float)_routeMapTex.height / _routeMapTex.width;
                GUI.DrawTexture(new Rect(Screen.width - w, 10, imgW, imgW * aspect), _routeMapTex, ScaleMode.ScaleToFit);
                GUILayout.Space(imgW * aspect + 8);
            }

            GUILayout.Label($"FACILITY ROUTE — Sector {sector.id} {sector.nameRu}", _label);
            GUILayout.Label("(карта не телепортирует — иди к двери на краю комнаты)", _label);
            GUILayout.Space(8);

            foreach (var node in sector.mapNodes)
            {
                bool current = node.roomId == state.CurrentRoomId;
                bool visited = state.VisitedRooms.Contains(node.roomId);
                string status = current ? "◉ YOU" : visited ? "● visited" : "○ ???";
                var room = GameDatabase.GetRoom(node.roomId);
                GUILayout.Label($"{status}  [{node.icon}] {room.nameRu}", _label);
            }

            GUILayout.Space(8);
            GUILayout.Label("— связи —", _label);
            foreach (var edge in sector.mapEdges)
            {
                GUILayout.Label($"• {edge.doorHintRu}", _label);
            }

            if (!state.HasKey("rust_key") && state.CurrentSectorId == "A")
                GUILayout.Label("\n⚠ GATE locked — Rust Key в Доме (южная дверь)", _label);

            GUILayout.Space(12);
            if (GUILayout.Button("Close Map [Tab]"))
                _visible = false;

            GUILayout.EndArea();
        }
    }
}
