using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnstableExperiment.Data
{
    [Serializable]
    public class DoorDef
    {
        public string id;
        public string labelRu;
        public string targetRoom;
        public string mapEdgeId;
        public string requiresKey;
        public string lockedHintRu;
        public string requiresClear;
        public string sectorTransition;
    }

    [Serializable]
    public class SpawnDef
    {
        public string enemyId;
        public int count = 1;
        public bool patrol;
        public string blockId;
    }

    [Serializable]
    public class LootDef
    {
        public string id;
        public string type;
    }

    [Serializable]
    public class RoomDef
    {
        public string id;
        public string type;
        public string nameRu;
        public int[] sizeTiles;
        public DoorDef[] doors;
        public SpawnDef[] spawns;
        public LootDef[] loot;
        public string eventId;
        public int restHeal;
        public bool restOnce;
        public string shopId;
        public string endingId;
    }

    [Serializable]
    public class MapNodeDef
    {
        public string id;
        public string roomId;
        public string icon;
        public int[] pos;
    }

    [Serializable]
    public class MapEdgeDef
    {
        public string id;
        public string from;
        public string to;
        public string doorHintRu;
    }

    [Serializable]
    public class SectorDef
    {
        public string id;
        public string nameRu;
        public string startRoom;
        public string mapItemId;
        public string mapItemNameRu;
        public string transitionVoiceLine;
        public RoomDef[] rooms;
        public MapNodeDef[] mapNodes;
        public MapEdgeDef[] mapEdges;
    }

    [Serializable]
    public class RoomsGraphRoot
    {
        public int version;
        public string description;
        public SectorDef[] sectors;
    }

    [Serializable]
    public class EffectDef
    {
        public string type;
        public int value;
    }

    [Serializable]
    public class OutcomeDef
    {
        public string tier;
        public int weight;
        public EffectDef[] effects;
    }

    [Serializable]
    public class TicketDef
    {
        public string id;
        public string name;
        public string nameRu;
        public string color;
        public int cost;
        public OutcomeDef[] outcomes;
        public string[] wildPools;
    }

    [Serializable]
    public class CombatConfig
    {
        public int playerHp = 40;
        public int energyPerTurn = 3;
        public int handSize = 4;
        public float scratchAnimSeconds = 0.4f;
    }

    [Serializable]
    public class TicketsRoot
    {
        public int version;
        public CombatConfig combat;
        public TicketDef[] tickets;
        public string[] starterRoll;
        public string exhaustedRollFallback = "blank";
    }

    [Serializable]
    public class WorldRuleDef
    {
        public string id;
        public string descriptionRu;
        public EffectDef[] combatStartEffects;
    }

    [Serializable]
    public class TicketRuleDef
    {
        public string id;
        public string descriptionRu;
        public string type;
        public bool afterFirstCombat;
        public float chance;
        public EffectDef[] extraEffects;
        public string tier;
        public int countPerCombat;
        public string[] visualPool;
        public string mismatchFlashText;
    }

    [Serializable]
    public class UnstableSectorDef
    {
        public string id;
        public string nameRu;
        public WorldRuleDef worldRule;
        public TicketRuleDef[] ticketRules;
    }

    [Serializable]
    public class UnstableRulesRoot
    {
        public UnstableSectorDef[] sectors;
    }

    public static class GameDatabase
    {
        public static RoomsGraphRoot RoomsGraph { get; private set; }
        public static TicketsRoot Tickets { get; private set; }
        public static UnstableRulesRoot UnstableRules { get; private set; }

        private static readonly Dictionary<string, RoomDef> RoomById = new();
        private static readonly Dictionary<string, SectorDef> SectorByRoomId = new();
        private static readonly Dictionary<string, TicketDef> TicketById = new();

        public static void LoadAll()
        {
            RoomsGraph = LoadJson<RoomsGraphRoot>("Data/rooms_graph");
            Tickets = LoadJson<TicketsRoot>("Data/tickets");
            UnstableRules = LoadJson<UnstableRulesRoot>("Data/unstable_rules");

            RoomById.Clear();
            SectorByRoomId.Clear();
            TicketById.Clear();

            foreach (var sector in RoomsGraph.sectors)
            {
                foreach (var room in sector.rooms)
                {
                    RoomById[room.id] = room;
                    SectorByRoomId[room.id] = sector;
                }
            }

            foreach (var t in Tickets.tickets)
                TicketById[t.id] = t;
        }

        public static T LoadJson<T>(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
                throw new InvalidOperationException($"Missing Resources/{resourcePath}.json");
            return JsonUtility.FromJson<T>(asset.text);
        }

        public static RoomDef GetRoom(string roomId)
        {
            if (!RoomById.TryGetValue(roomId, out var room))
                throw new KeyNotFoundException($"Room not found: {roomId}");
            return room;
        }

        public static SectorDef GetSectorForRoom(string roomId) => SectorByRoomId[roomId];

        public static SectorDef GetSector(string sectorId)
        {
            foreach (var s in RoomsGraph.sectors)
                if (s.id == sectorId) return s;
            throw new KeyNotFoundException($"Sector {sectorId}");
        }

        public static TicketDef GetTicket(string id) => TicketById[id];

        public static UnstableSectorDef GetUnstable(string sectorId)
        {
            foreach (var s in UnstableRules.sectors)
                if (s.id == sectorId) return s;
            return null;
        }

        public static int GetEnemyHp(string enemyId) => enemyId switch
        {
            "subject_03" => 20,
            "ward_hulk" => 28,
            "mask_wretch" => 18,
            "plague" => 32,
            "subject_12" => 44,
            "pit_dweller" => 14,
            "patchwork_butcher" => 85,
            _ => 15
        };

        public static string GetEnemyNameRu(string enemyId) => enemyId switch
        {
            "subject_03" => "Subject 03",
            "plague" => "Plague",
            "subject_12" => "Subject 12",
            "patchwork_butcher" => "Patchwork Butcher",
            _ => enemyId
        };
    }
}
