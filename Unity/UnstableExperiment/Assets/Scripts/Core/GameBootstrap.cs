using UnityEngine;
using UnstableExperiment.Combat;
using UnstableExperiment.UI;
using UnstableExperiment.World;

namespace UnstableExperiment.Core
{
    /// <summary>Entry point — attach to empty GameObject in scene.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool autoStart = true;

        private RoomManager _rooms;
        private CombatManager _combat;
        private GameHUD _hud;
        private RouteMapUI _routeMap;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (Object.FindObjectOfType<GameBootstrap>() != null) return;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 8f;
                cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
                if (cam.GetComponent<CameraFollow>() == null)
                    cam.gameObject.AddComponent<CameraFollow>();
            }

            var go = new GameObject("Game");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            GameState.Reset();
            GameDatabase.LoadAll();

            _rooms = gameObject.AddComponent<RoomManager>();
            _combat = gameObject.AddComponent<CombatManager>();
            _hud = gameObject.AddComponent<GameHUD>();
            _routeMap = gameObject.AddComponent<RouteMapUI>();

            _hud.Initialize(_rooms, _combat);
            _routeMap.Initialize(_rooms);
            _combat.Initialize(_rooms);
            _rooms.Initialize(_combat, _hud);
        }

        private void Start()
        {
            if (autoStart)
                _rooms.EnterRoom(GameState.Instance.CurrentRoomId, null);
        }
    }
}
