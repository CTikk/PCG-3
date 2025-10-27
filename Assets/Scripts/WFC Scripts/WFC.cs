using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC
{
    private readonly TileSet _tileSet;
    private readonly WFCGrid _grid;
    private System.Random _rng;
    private Stack<(int x, int y, List<TileSet.TileDef> snapshot)> _stack = new();

    public WFC(TileSet tileSet, WFCGrid grid, int seed = 0)
    {
        _tileSet = tileSet;
        _grid = grid;
        _rng = seed == 0 ? new System.Random() : new System.Random(seed);
    }

    public bool Step() // ejecuta 1 colapso + propagación; retorna false si quedó terminado o sin solución
    {
        // 1) elegir celda de mínima entropía (no colapsada)
        var target = FindMinEntropy();
        if (target == null) return false; // ya no hay celdas por resolver
        var (tx, ty) = target.Value;
        var cell = _grid.cells[tx, ty];

        // snapshot para backtracking
        _stack.Push((tx, ty, new List<TileSet.TileDef>(cell.domain)));

        // 2) colapsar: elegir 1 opción (ponderada por 'weight')
        var choice = WeightedRandom(cell.domain);
        cell.domain = new List<TileSet.TileDef> { choice };

        // 3) propagar restricciones
        if (!PropagateFrom(tx, ty))
        {
            // contradicción — backtracking
            while (_stack.Count > 0)
            {
                var (bx, by, prevDomain) = _stack.Pop();
                var backCell = _grid.cells[bx, by];
                // si no hay más alternativas para probar en esa celda, seguimos subiendo
                // quitamos la opción que usamos y reintentamos
                if (prevDomain.Count <= 1) continue;
                var used = backCell.domain[0];
                var remaining = prevDomain.Where(t => t != used).ToList();
                if (remaining.Count == 0) continue;

                // restauramos el resto del estado de la grilla desde cero sería lo más seguro,
                // pero para simplicidad: reinicia grilla y re-fija los colapsos previos menos esa elección
                ResetDomainsToAll();
                // reaplicamos todos los colapsos de la pila (sin el último que falló)
                var replay = _stack.Reverse().ToList();
                _stack.Clear();
                foreach (var s in replay)
                {
                    var c = _grid.cells[s.x, s.y];
                    var ch = WeightedRandom(s.snapshot); // misma política
                    c.domain = new List<TileSet.TileDef> { ch };
                    _stack.Push((s.x, s.y, s.snapshot));
                    if (!PropagateFrom(s.x, s.y)) break;
                }

                // Ahora probamos una alternativa en (bx,by)
                backCell.domain = remaining;
                _stack.Push((bx, by, remaining));
                var chosen = WeightedRandom(backCell.domain);
                backCell.domain = new List<TileSet.TileDef> { chosen };
                return PropagateFrom(bx, by);
            }
            return false; // sin solución
        }

        return true;
    }

    public bool IsDone()
    {
        for (int x = 0; x < _grid.W; x++)
            for (int y = 0; y < _grid.H; y++)
                if (!_grid.cells[x, y].Collapsed) return false;
        return true;
    }

    private (int x, int y)? FindMinEntropy()
    {
        int bestX = -1, bestY = -1, best = int.MaxValue;

        for (int x = 0; x < _grid.W; x++)
            for (int y = 0; y < _grid.H; y++)
            {
                var c = _grid.cells[x, y];
                if (c.Collapsed) continue;
                int e = c.domain.Count;
                if (e < best)
                {
                    best = e;
                    bestX = x;
                    bestY = y;
                }
            }

        if (bestX == -1)
            return null; // no hay celdas disponibles
        else
            return (bestX, bestY);
    }

    private TileSet.TileDef WeightedRandom(List<TileSet.TileDef> domain)
    {
        float sum = domain.Sum(t => t.weight);
        float r = (float)(_rng.NextDouble() * sum);
        foreach (var t in domain)
        {
            r -= t.weight;
            if (r <= 0) return t;
        }
        return domain[domain.Count - 1];
    }

    private bool PropagateFrom(int sx, int sy)
    {
        var q = new Queue<(int x, int y)>();
        q.Enqueue((sx, sy));

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            var cur = _grid.cells[x, y];

            foreach (var (nx, ny) in _grid.Neighs4(x, y))
            {
                var ncell = _grid.cells[nx, ny];
                int before = ncell.domain.Count;

                // Filtrar dominio del vecino según la relación con la celda actual
                ncell.domain = ncell.domain.Where(tn => IsCompatible(tn, nx, ny, x, y, cur.domain)).ToList();
                if (ncell.domain.Count == 0) return false; // contradicción

                if (ncell.domain.Count < before) q.Enqueue((nx, ny));
            }
        }
        return true;
    }

    private bool IsCompatible(TileSet.TileDef neighborCandidate, int nx, int ny, int x, int y, List<TileSet.TileDef> centerDomain)
    {
        // Determina dirección: (nx,ny) es vecino de (x,y)
        Dir dirFromCenterToNeighbor;
        if (ny == y + 1) dirFromCenterToNeighbor = Dir.N;
        else if (ny == y - 1) dirFromCenterToNeighbor = Dir.S;
        else if (nx == x + 1) dirFromCenterToNeighbor = Dir.E;
        else dirFromCenterToNeighbor = Dir.O;

        // Para que neighborCandidate sea válido, debe ser aceptado por *alguna* opción del centro.
        foreach (var center in centerDomain)
        {
            if (dirFromCenterToNeighbor == Dir.N && center.north.Contains(neighborCandidate.id)) return true;
            if (dirFromCenterToNeighbor == Dir.S && center.south.Contains(neighborCandidate.id)) return true;
            if (dirFromCenterToNeighbor == Dir.E && center.east.Contains(neighborCandidate.id)) return true;
            if (dirFromCenterToNeighbor == Dir.O && center.west.Contains(neighborCandidate.id)) return true;
        }
        return false;
    }

    private void ResetDomainsToAll()
    {
        var all = GetAllTiles();
        for (int x = 0; x < _grid.W; x++)
            for (int y = 0; y < _grid.H; y++)
                _grid.cells[x, y].domain = new List<TileSet.TileDef>(all);
    }

    private List<TileSet.TileDef> GetAllTiles() =>
        _tileSet.tiles.ToList();
}