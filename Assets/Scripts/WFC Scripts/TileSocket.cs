// Assets/Scripts/WFC/TileSockets.cs
/*using System;
using UnityEngine;
using System.Collections.Generic;

public enum TerrainKind { 
    Grass,
    Sand,
    Transition,
    Road
}

[Serializable]
public struct EdgeSocket
{
    public TerrainKind inner;
    public TerrainKind outer;

    public bool MatchesOpposite(EdgeSocket other)
    {
        return this.outer == other.inner && other.outer == this.inner;
    }

    public override string ToString() => $"({inner}->{outer})";
}

// Describe un "arquetipo" de tile sin rotación (base)
[CreateAssetMenu(fileName = "TilesCatalog", menuName = "WFC/Tiles Catalog")]
public class TilesCatalog : ScriptableObject
{
    [Serializable]
    public class TilePrototype
    {
        public string id;              // ej: "grass_center", "trans_gs_edge"
        public TerrainKind kind;       // Grass, Sand, Transition (para clasificar)
        public Sprite sprite;          // sprite base (0°)
        public bool allowRotate = true;

        // sockets en orientación 0° (N,S,E,O)
        public EdgeSocket north;
        public EdgeSocket south;
        public EdgeSocket east;
        public EdgeSocket west;
    }

    public List<TilePrototype> prototypes = new();
}*/