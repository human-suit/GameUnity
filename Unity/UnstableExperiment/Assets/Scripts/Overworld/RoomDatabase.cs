using System;
using UnityEngine;

[Serializable]
public class RoomsGraphFile
{
    public int version;
    public SectorDef[] sectors;
}

[Serializable]
public class SectorDef
{
    public string id;
    public string nameRu;
    public string startRoom;
    public RoomDef[] rooms;
}

[Serializable]
public class RoomDef
{
    public string id;
    public string type;
    public string nameRu;
    public LootDef[] loot;
}

[Serializable]
public class LootDef
{
    public string id;
    public string type;
}

public static class RoomDatabase
{
    private static RoomsGraphFile _graph;

    public static void Load()
    {
        if (_graph != null) return;

        var asset = Resources.Load<TextAsset>("Data/rooms_graph");
        if (asset == null)
        {
            Debug.LogError("RoomDatabase: не найден Resources/Data/rooms_graph.json");
            return;
        }

        _graph = JsonUtility.FromJson<RoomsGraphFile>(asset.text);
    }

    public static RoomDef GetRoom(string roomId)
    {
        Load();
        if (_graph?.sectors == null) return null;

        foreach (var sector in _graph.sectors)
        {
            if (sector.rooms == null) continue;
            foreach (var room in sector.rooms)
                if (room.id == roomId) return room;
        }

        return null;
    }
}
