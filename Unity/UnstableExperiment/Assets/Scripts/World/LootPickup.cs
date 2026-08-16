using UnityEngine;
using UnstableExperiment.Core;
using UnstableExperiment.Data;

namespace UnstableExperiment.World
{
    public class LootPickup : MonoBehaviour
    {
        public bool Picked { get; private set; }
        private LootDef _def;
        private RoomManager _rooms;
        private TextMesh _label;

        public static LootPickup Create(Transform root, LootDef def, Vector3 pos, RoomManager rooms)
        {
            var go = new GameObject($"Loot_{def.id}");
            go.transform.SetParent(root);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Loot;
            sr.sortingOrder = 7;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;

            var loot = go.AddComponent<LootPickup>();
            loot._def = def;
            loot._rooms = rooms;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0, 0.5f, 0);
            loot._label = labelGo.AddComponent<TextMesh>();
            loot._label.text = def.id;
            loot._label.fontSize = 20;
            loot._label.characterSize = 0.05f;
            loot._label.anchor = TextAnchor.MiddleCenter;

            return loot;
        }

        public void Pickup()
        {
            if (Picked) return;
            Picked = true;
            var state = GameState.Instance;

            switch (_def.type)
            {
                case "key":
                    state.AddKey(_def.id);
                    _rooms.ShowMessage($"Получен ключ: {_def.id}", 2.5f);
                    break;
                case "map_unlock":
                    var sector = GameDatabase.GetSectorForRoom(state.CurrentRoomId);
                    state.UnlockedMaps.Add(_def.id);
                    _rooms.ShowMessage($"Карта: {sector.mapItemNameRu} (Tab)", 3f);
                    break;
                case "consumable":
                    _rooms.ShowMessage($"Предмет: {_def.id}", 2f);
                    break;
            }

            state.CollectedLoot.Add(_def.id);
            _rooms.OnLootCollected(this);
            Destroy(gameObject);
        }
    }
}
