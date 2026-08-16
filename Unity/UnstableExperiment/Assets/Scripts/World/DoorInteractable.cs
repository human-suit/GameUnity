using UnityEngine;
using UnstableExperiment.Core;
using UnstableExperiment.Data;

namespace UnstableExperiment.World
{
    public class DoorInteractable : MonoBehaviour
    {
        public DoorDef Definition { get; private set; }
        private RoomManager _rooms;
        private TextMesh _label;

        public static DoorInteractable Create(Transform root, DoorDef def, Vector3 pos, float tileSize)
        {
            var go = new GameObject($"Door_{def.id}");
            go.transform.SetParent(root);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Door;
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(tileSize * 2f, tileSize * 0.8f, 1f);

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 0.8f);

            var door = go.AddComponent<DoorInteractable>();
            door.Definition = def;
            door._rooms = root.GetComponentInParent<RoomManager>();
            if (door._rooms == null)
                door._rooms = Object.FindObjectOfType<RoomManager>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0, 0.6f, 0);
            door._label = labelGo.AddComponent<TextMesh>();
            door._label.text = def.labelRu;
            door._label.fontSize = 24;
            door._label.characterSize = 0.06f;
            door._label.anchor = TextAnchor.MiddleCenter;
            door._label.color = new Color(0.9f, 0.85f, 0.7f);

            return door;
        }

        public void TryEnter() => _rooms?.RequestDoorTravel(Definition);

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovement>() == null) return;
            if (!GameState.Instance.CanUseDoor(Definition, out _))
                _label.color = new Color(0.6f, 0.4f, 0.4f);
            else
                _label.color = new Color(0.9f, 0.85f, 0.7f);
        }
    }
}
