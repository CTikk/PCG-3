using System.Collections.Generic;
using UnityEngine;

public enum Dir { N, S, E, O }

[CreateAssetMenu(fileName = "TileSet", menuName = "WFC/TileSet")]
public class TileSet : ScriptableObject
{
    [System.Serializable]
    public class TileDef
    {
        public string id;                // ej: "grass", "road_ns", "corner_ne"
        public Sprite sprite;            // para visualizar en 2D
        public bool rotatable = false;   // opcional: si habilitas rotaciones
        public List<string> north;       // qué IDs pueden estar al norte
        public List<string> south;       // al sur
        public List<string> east;        // al este
        public List<string> west;        // al oeste
        public float weight = 1f;        // para aleatoriedad ponderada
    }

    public List<TileDef> tiles;

    // Acceso rápido por ID
    private Dictionary<string, TileDef> _byId;
    public TileDef Get(string id)
    {
        _byId ??= Build();
        return _byId[id];
    }

    private Dictionary<string, TileDef> Build()
    {
        var d = new Dictionary<string, TileDef>();
        foreach (var t in tiles) d[t.id] = t;
        return d;
    }
}