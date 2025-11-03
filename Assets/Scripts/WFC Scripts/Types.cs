// Assets / Scripts / WFC / Types.cs
using System;
using UnityEngine;

public enum GroundKind { Empty, Grass, Sand }
public enum RoadKind { None, Road }

[Serializable]
public struct EdgeSocket2
{
    public GroundKind groundInner;  // base dentro del tile (este lado)
    public GroundKind groundOuter;  // base esperada en el vecino (lado opuesto)
    public RoadKind roadInner;    // camino en este borde
    public RoadKind roadOuter;    // camino esperado en el vecino

    public bool MatchesOpposite(EdgeSocket2 other)
    {
        bool groundOK = this.groundOuter == other.groundInner
                     && other.groundOuter == this.groundInner;
        bool roadOK = this.roadOuter == other.roadInner
                     && other.roadOuter == this.roadInner;
        return groundOK && roadOK;
    }

    public override string ToString()
        => $"G({groundInner}->{groundOuter}) R({roadInner}->{roadOuter})";
}

