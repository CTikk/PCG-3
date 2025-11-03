using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilesCatalog", menuName = "WFC/Tiles Catalog")]
public class TilesCatalog : ScriptableObject
{
    [Serializable]
    public class TilePrototype
    {
        public string id;            // ej: "grass_full", "road_straight_grass"
        public Sprite sprite;        // sprite base (0°). El GO se rotará en runtime.
        public bool allowRotate = true;
        public float weight = 1f;    // opcional: para aleatoriedad ponderada

        // Sockets en orientación 0° (N, S, E, O)
        public EdgeSocket2 north;
        public EdgeSocket2 south;
        public EdgeSocket2 east;
        public EdgeSocket2 west;
    }

    public List<TilePrototype> prototypes = new();
}