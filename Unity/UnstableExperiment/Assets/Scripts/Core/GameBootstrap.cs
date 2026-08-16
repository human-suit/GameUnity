using UnityEngine;
using UnstableExperiment.Combat;
using UnstableExperiment.Data;
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
            ConfigureMainCamera();

            try
            {
                GameState.Reset();
                GameDatabase.LoadAll();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UnstableExperiment] Failed to load data: {ex.Message}");
                return;
            }

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
            if (!autoStart || _rooms == null) return;
            _rooms.EnterRoom(GameState.Instance.CurrentRoomId, null);
            Debug.Log("[UnstableExperiment] Sector A started — a_plaza");
        }

        private static void ConfigureMainCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
    }
}
