// Assets/Scripts/WFC/WFCGrid.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WFCGrid
{
    public readonly int W, H;

    public class Cell
    {
        public List<string> domain;
        public bool Collapsed => domain.Count == 1;
        public Cell(List<string> all) { domain = new List<string>(all); }
    }

    public readonly Cell[,] cells;

    public WFCGrid(int w, int h, List<string> all)
    {
        W = w; H = h;
        cells = new Cell[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                cells[x, y] = new Cell(all);
    }

    public IEnumerable<(int x, int y)> Neighs4(int x, int y)
    {
        if (y + 1 < H) yield return (x, y + 1); // N
        if (y - 1 >= 0) yield return (x, y - 1); // S
        if (x + 1 < W) yield return (x + 1, y); // E
        if (x - 1 >= 0) yield return (x - 1, y); // W
    }

    public bool AllCollapsed()
    {
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                if (!cells[x, y].Collapsed) return false;
        return true;
    }
}