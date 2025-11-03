// Assets/Scripts/WFC/WFC_Auto.cs
using System.Collections.Generic;
using System.Linq;

public class WFC_Auto
{
    private readonly AutoTileSet _set;
    private readonly WFCGrid _grid;
    private readonly System.Random _rng;

    public WFC_Auto(AutoTileSet set, WFCGrid grid, int seed = 0)
    {
        _set = set;
        _grid = grid;
        _rng = seed == 0 ? new System.Random() : new System.Random(seed);
    }

    public bool Step()
    {
        var t = FindMinEntropy();
        if (t == null) return false;
        var (x, y) = t.Value;

        var dom = _grid.cells[x, y].domain;
        var choice = WeightedRandom(dom);
        _grid.cells[x, y].domain = new List<string> { choice };

        return PropagateFrom(x, y);
    }

    public bool IsDone() => _grid.AllCollapsed();

    private (int x, int y)? FindMinEntropy()
    {
        int bx = -1, by = -1, best = int.MaxValue;
        for (int x = 0; x < _grid.W; x++)
            for (int y = 0; y < _grid.H; y++)
            {
                var c = _grid.cells[x, y];
                int e = c.domain.Count;
                if (e > 1 && e < best) { best = e; bx = x; by = y; }
            }
        if (bx == -1) return null;
        return (bx, by);
    }

    private string WeightedRandom(List<string> domain)
    {
        if (domain.Count == 1) return domain[0];
        float sum = 0f;
        for (int i = 0; i < domain.Count; i++)
            sum += _set.Get(domain[i]).weight;

        float r = (float)(_rng.NextDouble() * sum);
        for (int i = 0; i < domain.Count; i++)
        {
            var w = _set.Get(domain[i]).weight;
            r -= w;
            if (r <= 0) return domain[i];
        }
        return domain[domain.Count - 1];
    }

    private bool PropagateFrom(int sx, int sy)
    {
        var q = new Queue<(int, int)>();
        q.Enqueue((sx, sy));

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            var center = _grid.cells[x, y].domain; // lista de IDs del centro

            foreach (var (nx, ny) in _grid.Neighs4(x, y))
            {
                var before = _grid.cells[nx, ny].domain.Count;

                var filtered = _grid.cells[nx, ny].domain.Where(idN =>
                {
                    var N = _set.Get(idN);
                    // N debe ser aceptado por ALGUNA opción del centro
                    for (int i = 0; i < center.Count; i++)
                    {
                        var C = _set.Get(center[i]);
                        if (ny == y + 1 && C.northOk.Contains(N.id)) return true; // vecino es N
                        if (ny == y - 1 && C.southOk.Contains(N.id)) return true; // S
                        if (nx == x + 1 && C.eastOk.Contains(N.id)) return true; // E
                        if (nx == x - 1 && C.westOk.Contains(N.id)) return true; // W
                    }
                    return false;
                }).ToList();

                _grid.cells[nx, ny].domain = filtered;
                if (filtered.Count == 0) return false; // contradicción
                if (filtered.Count < before) q.Enqueue((nx, ny));
            }
        }
        return true;
    }
}
