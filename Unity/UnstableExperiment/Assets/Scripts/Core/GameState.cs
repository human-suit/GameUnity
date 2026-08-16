using System.Collections.Generic;
using UnstableExperiment.Data;

namespace UnstableExperiment.Core
{
    public sealed class GameState
    {
        public static GameState Instance { get; private set; }

        public string CurrentRoomId { get; set; } = "a_plaza";
        public string CurrentSectorId { get; set; } = "A";
        public string EntryDoorId { get; set; }

        public int PlayerHp { get; set; } = 40;
        public int PlayerMaxHp { get; set; } = 40;

        public HashSet<string> Keys { get; } = new();
        public HashSet<string> ClearedBlocks { get; } = new();
        public HashSet<string> VisitedRooms { get; } = new();
        public HashSet<string> CollectedLoot { get; } = new();
        public HashSet<string> UnlockedMaps { get; } = new();

        public bool HasMapForCurrentSector =>
            UnlockedMaps.Contains(GameDatabase.GetSector(CurrentSectorId).mapItemId);

        public int CombatsWon { get; set; }
        public bool RevealOutcomes { get; set; }

        public static void Reset()
        {
            Instance = new GameState();
        }

        public bool HasKey(string keyId) => Keys.Contains(keyId);

        public void AddKey(string keyId) => Keys.Add(keyId);

        public bool IsBlockCleared(string blockId) =>
            string.IsNullOrEmpty(blockId) || ClearedBlocks.Contains(blockId);

        public void MarkBlockCleared(string blockId)
        {
            if (!string.IsNullOrEmpty(blockId))
                ClearedBlocks.Add(blockId);
        }

        public bool CanUseDoor(DoorDef door, out string reason)
        {
            reason = null;
            if (!string.IsNullOrEmpty(door.requiresKey) && !HasKey(door.requiresKey))
            {
                reason = string.IsNullOrEmpty(door.lockedHintRu)
                    ? $"Нужен ключ: {door.requiresKey}"
                    : door.lockedHintRu;
                return false;
            }

            if (!string.IsNullOrEmpty(door.requiresClear) && !IsBlockCleared(door.requiresClear))
            {
                reason = $"Сначала победите: {door.requiresClear}";
                return false;
            }

            return true;
        }
    }
}
