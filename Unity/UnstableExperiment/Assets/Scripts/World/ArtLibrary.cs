using UnityEngine;

namespace UnstableExperiment.World
{
    public static class ArtLibrary
    {
        private static Sprite _player;
        private static Sprite _enemy;
        private static Sprite _routeMap;

        public static Sprite Player =>
            _player ??= LoadSprite("Art/Characters/subject_07_icon")
                   ?? SpriteFactory.FallbackPlayer;

        public static Sprite Enemy =>
            _enemy ??= LoadSprite("Art/Characters/subject_03_icon")
                   ?? SpriteFactory.FallbackEnemy;

        public static Texture2D RouteMap =>
            LoadTexture("Art/UI/route_map_sector_a");

        public static Sprite GetRoomBackground(string roomId)
        {
            return LoadSprite($"Art/Rooms/{roomId}_room");
        }

        public static Sprite GetDoorSprite(string doorId)
        {
            // Optional per-door art from sheet later
            return null;
        }

        private static Sprite LoadSprite(string path)
        {
            var s = Resources.Load<Sprite>(path);
            if (s != null) return s;
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                Mathf.Max(tex.width, tex.height) / 14f);
        }

        private static Texture2D LoadTexture(string path) => Resources.Load<Texture2D>(path);
    }
}
