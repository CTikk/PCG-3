// Assets/Scripts/WFC/AutoTileSet.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuiltTile
{
    public string id;           // único; incluye rotación: "grass_full@r0", "@r90", etc.
    public Sprite sprite;
    public int rotation;        // 0, 90, 180, 270 (grados)
    public float weight = 1f;   // para elección ponderada

    // sockets ya rotados
    public EdgeSocket2 north, south, east, west;

    // compatibilidades precalculadas (listas de IDs)
    public List<string> northOk = new();
    public List<string> southOk = new();
    public List<string> eastOk = new();
    public List<string> westOk = new();
}

[CreateAssetMenu(fileName = "AutoTileSet", menuName = "WFC/Auto TileSet")]
public class AutoTileSet : ScriptableObject
{
    public List<BuiltTile> tiles = new();

    private Dictionary<string, BuiltTile> _byId;
    public BuiltTile Get(string id)
    {
        _byId ??= BuildIndex();
        return _byId[id];
    }

    private Dictionary<string, BuiltTile> BuildIndex()
    {
        var d = new Dictionary<string, BuiltTile>();
        foreach (var t in tiles) d[t.id] = t;
        return d;
    }
}

public static class TileSetAutoBuilder
{
    public static AutoTileSet BuildFromCatalog(TilesCatalog catalog)
    {
        var result = ScriptableObject.CreateInstance<AutoTileSet>();
        var temp = new List<BuiltTile>();

        foreach (var p in catalog.prototypes)
        {
            // genera rotaciones según allowRotate
            if (p.allowRotate)
            {
                temp.Add(MakeRot(p, 0));
                temp.Add(MakeRot(p, 90));
                temp.Add(MakeRot(p, 180));
                temp.Add(MakeRot(p, 270));
            }
            else
            {
                temp.Add(MakeRot(p, 0));
            }
        }

        // precalcular compatibilidades
        for (int i = 0; i < temp.Count; i++)
        {
            for (int j = 0; j < temp.Count; j++)
            {
                if (i == j) continue;
                var A = temp[i];
                var B = temp[j];

                if (A.north.MatchesOpposite(B.south)) A.northOk.Add(B.id);
                if (A.south.MatchesOpposite(B.north)) A.southOk.Add(B.id);
                if (A.east.MatchesOpposite(B.west)) A.eastOk.Add(B.id);
                if (A.west.MatchesOpposite(B.east)) A.westOk.Add(B.id);
            }
        }

        result.tiles = temp;
        return result;
    }

    // rota sockets 90° CW repetidamente hasta 'rot' (0/90/180/270)
    private static BuiltTile MakeRot(TilesCatalog.TilePrototype p, int rot)
    {
        EdgeSocket2 n = p.north, s = p.south, e = p.east, w = p.west;

        int times = (rot / 90) % 4;
        for (int k = 0; k < times; k++)
        {
            // Rotación 90° clockwise:
            // nuevoN = viejoW, nuevoE = viejoN, nuevoS = viejoE, nuevoW = viejoS
            var oldN = n;
            n = w;
            w = s;
            s = e;
            e = oldN;
        }

        return new BuiltTile
        {
            id = $"{p.id}@r{rot}",
            sprite = p.sprite,
            rotation = rot,
            weight = p.weight,
            north = n,
            south = s,
            east = e,
            west = w
        };
    }
}