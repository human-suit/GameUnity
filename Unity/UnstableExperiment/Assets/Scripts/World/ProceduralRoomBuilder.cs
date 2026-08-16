using System.Collections.Generic;
using UnityEngine;
using UnstableExperiment.Data;

namespace UnstableExperiment.World
{
    public static class SpriteFactory
    {
        private static Sprite _player;
        private static Sprite _enemy;
        private static Sprite _floor;
        private static Sprite _wall;
        private static Sprite _door;
        private static Sprite _loot;

        public static Sprite FallbackPlayer => _player ??= CreateCircle(new Color(0.55f, 0.65f, 0.75f), 16);
        public static Sprite FallbackEnemy => _enemy ??= CreateCircle(new Color(0.75f, 0.35f, 0.35f), 14);
        public static Sprite Floor => _floor ??= CreateSolid(new Color(0.28f, 0.32f, 0.26f));
        public static Sprite Wall => _wall ??= CreateSolid(new Color(0.12f, 0.12f, 0.14f));
        public static Sprite Door => _door ??= CreateSolid(new Color(0.55f, 0.42f, 0.22f));
        public static Sprite Loot => _loot ??= CreateCircle(new Color(0.85f, 0.75f, 0.25f), 10);

        private static Sprite CreateSolid(Color c)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        private static Sprite CreateCircle(Color c, int radius)
        {
            int size = radius * 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = radius - 0.5f;
            var center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= r ? c : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }

    public static class ProceduralRoomBuilder
    {
        public static void Build(RoomDef room, Transform root, List<DoorInteractable> doors, float tileSize)
        {
            int w = room.sizeTiles[0];
            int h = room.sizeTiles[1];
            float roomW = w * tileSize;
            float roomH = h * tileSize;

            var roomBg = ArtLibrary.GetRoomBackground(room.id);
            if (roomBg != null)
                CreateRoomBackground(root, roomBg, roomW, roomH);

            var origin = new Vector2(-roomW * 0.5f, -roomH * 0.5f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool edge = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                bool isDoorCell = IsDoorGap(room, x, y, w, h);
                var pos = origin + new Vector2((x + 0.5f) * tileSize, (y + 0.5f) * tileSize);

                if (edge && !isDoorCell)
                {
                    if (roomBg == null)
                        CreateTile(root, "Wall", pos, SpriteFactory.Wall, true);
                    else
                        CreateInvisibleWall(root, pos);
                }
                else if (roomBg == null)
                    CreateTile(root, "Floor", pos, SpriteFactory.Floor, edge && !isDoorCell);
            }

            CreateLabel(root, room.nameRu, new Vector3(0, h * tileSize * 0.5f + 0.6f, 0));

            if (room.doors != null)
            {
                foreach (var door in room.doors)
                {
                    var worldPos = DoorWorldPosition(room, door.id, tileSize);
                    var doorGo = DoorInteractable.Create(root, door, worldPos, tileSize);
                    doors.Add(doorGo);
                }
            }
        }

        private static void CreateRoomBackground(Transform root, Sprite bg, float roomW, float roomH)
        {
            var go = new GameObject("RoomBackground");
            go.transform.SetParent(root);
            go.transform.localPosition = Vector3.zero;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bg;
            sr.sortingOrder = -10;
            float sx = roomW / sr.bounds.size.x;
            float sy = roomH / sr.bounds.size.y;
            go.transform.localScale = new Vector3(sx, sy, 1f);
        }

        private static void CreateInvisibleWall(Transform root, Vector2 pos)
        {
            var go = new GameObject("WallCollider");
            go.transform.SetParent(root);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * RoomManager.TileSize;
        }

        private static void CreateTile(Transform root, string name, Vector2 pos, Sprite sprite, bool solid, float alpha = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1, 1, 1, alpha);
            sr.drawMode = SpriteDrawMode.Simple;
            go.transform.localScale = Vector3.one * RoomManager.TileSize;
            if (solid)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
            }
        }

        private static void CreateTile(Transform root, string name, Vector2 pos, Sprite sprite, bool solid)
            => CreateTile(root, name, pos, sprite, solid, 1f);

        private static void CreateLabel(Transform root, string text, Vector3 pos)
        {
            var go = new GameObject("RoomLabel");
            go.transform.SetParent(root);
            go.transform.position = pos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 32;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.75f, 0.7f, 0.65f);
        }

        public static Vector3 GetSpawnPosition(RoomDef room, string entryDoorId, float tileSize)
        {
            if (string.IsNullOrEmpty(entryDoorId))
                return new Vector3(0, -room.sizeTiles[1] * tileSize * 0.25f, 0);

            var doorPos = DoorWorldPosition(room, entryDoorId, tileSize);
            var offset = entryDoorId switch
            {
                "north" => Vector3.down,
                "south" => Vector3.up,
                "east" => Vector3.left,
                "west" => Vector3.right,
                _ => Vector3.zero
            };
            return doorPos + offset * 1.2f;
        }

        public static Vector3 GetEnemyPosition(RoomDef room, int index, float tileSize)
        {
            float angle = index * 137.5f * Mathf.Deg2Rad;
            float r = Mathf.Min(room.sizeTiles[0], room.sizeTiles[1]) * tileSize * 0.15f;
            return new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
        }

        public static Vector3 GetLootPosition(RoomDef room, int index, float tileSize)
        {
            return new Vector3((index - 0.5f) * 1.5f, 0.5f, 0);
        }

        private static Vector3 DoorWorldPosition(RoomDef room, string doorId, float tileSize)
        {
            int w = room.sizeTiles[0];
            int h = room.sizeTiles[1];
            var origin = new Vector2(-w * tileSize * 0.5f, -h * tileSize * 0.5f);
            return doorId switch
            {
                "north" => origin + new Vector2(w * tileSize * 0.5f, h * tileSize - tileSize * 0.5f),
                "south" => origin + new Vector2(w * tileSize * 0.5f, tileSize * 0.5f),
                "east" => origin + new Vector2(w * tileSize - tileSize * 0.5f, h * tileSize * 0.5f),
                "west" => origin + new Vector2(tileSize * 0.5f, h * tileSize * 0.5f),
                _ => Vector3.zero
            };
        }

        private static bool IsDoorGap(RoomDef room, int x, int y, int w, int h)
        {
            if (room.doors == null) return false;
            int cx = w / 2;
            int cy = h / 2;
            foreach (var door in room.doors)
            {
                switch (door.id)
                {
                    case "north" when y == h - 1 && x >= cx - 1 && x <= cx + 1: return true;
                    case "south" when y == 0 && x >= cx - 1 && x <= cx + 1: return true;
                    case "east" when x == w - 1 && y >= cy - 1 && y <= cy + 1: return true;
                    case "west" when x == 0 && y >= cy - 1 && y <= cy + 1: return true;
                }
            }
            return false;
        }
    }
}
