// Assets/Scripts/WFC/AutoWFCController.cs
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AutoWFCController : MonoBehaviour
{
    [Header("Datos")]
    public TilesCatalog catalog;       // define pocos arquetipos
    public AutoTileSet autoTileSet;

    [Header("Visual")]
    public GameObject tilePrefab;      // prefab con SpriteRenderer
    public int width = 16;
    public int height = 16;
    public float cellSize = 1f;

    [Header("Ejecución")]
    public bool autoRun = true;
    public float stepsPerSecond = 60f;
    public int seed = 0;

    private WFCGrid _grid;
    private WFC_Auto _wfc;
    private GameObject[,] _gos;
    private float _accum;

    [Header("Generación Desde Imagen")]
    public Texture2D inputImage;
    public int patternSize = 3;

    private void Start()
    {
        if (inputImage != null)
        {
            autoTileSet = BuildFromImage(inputImage, patternSize);
        }
        else if (autoTileSet == null)
        {
            if (catalog == null) { Debug.LogError("Falta TilesCatalog"); return; }
            autoTileSet = TileSetAutoBuilder.BuildFromCatalog(catalog);
        }

        if (autoTileSet == null)
        {
            if (catalog == null) { Debug.LogError("Falta TilesCatalog"); return; }
            autoTileSet = TileSetAutoBuilder.BuildFromCatalog(catalog);
        }

        var allIds = autoTileSet.tiles.Select(t => t.id).ToList();
        _grid = new WFCGrid(width, height, allIds);
        _wfc = new WFC_Auto(autoTileSet, _grid, seed);

        _gos = new GameObject[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _gos[x, y] = Instantiate(tilePrefab, new Vector3(x * cellSize, y * cellSize, 0f), Quaternion.identity, transform);

        Redraw();
    }

    private void Update()
    {
        if (autoRun && !_wfc.IsDone())
        {
            _accum += Time.deltaTime * stepsPerSecond;
            while (_accum >= 1f && !_wfc.IsDone())
            {
                if (!_wfc.Step()) break;
                _accum -= 1f;
            }
            Redraw();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_wfc.Step()) Redraw();
        }
    }

    private void Redraw()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var go = _gos[x, y];
                var sr = go.GetComponent<SpriteRenderer>();
                var domain = _grid.cells[x, y].domain;

                if (domain.Count == 0)
                {
                    sr.sprite = null;
                    sr.color = Color.red;
                    go.transform.rotation = Quaternion.identity;
                }
                else if (domain.Count == 1)
                {
                    var tile = autoTileSet.Get(domain[0]);
                    sr.sprite = tile.sprite;
                    sr.color = Color.white;
                    go.transform.rotation = Quaternion.Euler(0, 0, tile.rotation);
                }
                else
                {
                    sr.sprite = null;
                    sr.color = new Color(1f, 1f, 1f, 0.15f);
                    go.transform.rotation = Quaternion.identity;
                }
            }
    }

    public static AutoTileSet BuildFromImage(Texture2D image, int patternSize)
    {
        var patterns = PatternExtractor.ExtractPatterns(image, patternSize);
        var patternHashes = new Dictionary<string, int>();
        var uniquePatterns = new List<Color[,]>();
        var patternWeights = new List<int>();

        // Extraer patrones únicos considerando rotaciones
        foreach (var p in patterns)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                var rotated = RotatePattern(p, rot);
                string hash = PatternHash(rotated);
                if (!patternHashes.TryGetValue(hash, out int idx))
                {
                    idx = uniquePatterns.Count;
                    patternHashes[hash] = idx;
                    uniquePatterns.Add(rotated);
                    patternWeights.Add(1);
                }
                else
                {
                    patternWeights[idx]++;
                }
            }
        }

        // Extraer sockets (bordes) de cada patrón
        List<Color[]> norths = new(), souths = new(), easts = new(), wests = new();
        foreach (var pat in uniquePatterns)
        {
            norths.Add(GetRow(pat, 0));
            souths.Add(GetRow(pat, patternSize - 1));
            wests.Add(GetCol(pat, 0));
            easts.Add(GetCol(pat, patternSize - 1));
        }

        // Crear BuiltTiles con sprites y rotación
        var autoTileSet = ScriptableObject.CreateInstance<AutoTileSet>();
        var tiles = new List<BuiltTile>();
        for (int i = 0; i < uniquePatterns.Count; i++)
        {
            var tile = new BuiltTile
            {
                id = $"pattern_{i}",
                sprite = PatternToSprite(uniquePatterns[i]),
                rotation = 0, // El sprite ya está rotado, así que puedes dejarlo en 0
                weight = patternWeights[i],
            };
            tiles.Add(tile);
        }

        // Calcular compatibilidades
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles.Count; j++)
            {
                if (RowsEqual(souths[i], norths[j]))
                    tiles[i].southOk.Add(tiles[j].id);
                if (RowsEqual(norths[i], souths[j]))
                    tiles[i].northOk.Add(tiles[j].id);
                if (ColsEqual(easts[i], wests[j]))
                    tiles[i].eastOk.Add(tiles[j].id);
                if (ColsEqual(wests[i], easts[j]))
                    tiles[i].westOk.Add(tiles[j].id);
            }
        }

        autoTileSet.tiles = tiles;
        return autoTileSet;
    }

    // Gira un patrón 90° * rot veces
    private static Color[,] RotatePattern(Color[,] pattern, int rot)
    {
        int N = pattern.GetLength(0);
        Color[,] result = (Color[,])pattern.Clone();
        for (int r = 0; r < rot; r++)
        {
            Color[,] temp = new Color[N, N];
            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    temp[y, N - 1 - x] = result[x, y];
            result = temp;
        }
        return result;
    }

    // Convierte un patrón a Sprite
    private static Sprite PatternToSprite(Color[,] pattern)
    {
        int N = pattern.GetLength(0);
        Texture2D tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.anisoLevel = 0;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                tex.SetPixel(x, y, pattern[x, N - 1 - y]);
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
    }



    // Helpers para extraer filas/columnas y comparar
    private static Color[] GetRow(Color[,] pattern, int row)
    {
        int N = pattern.GetLength(0);
        var arr = new Color[N];
        for (int i = 0; i < N; i++) arr[i] = pattern[i, row];
        return arr;
    }
    private static Color[] GetCol(Color[,] pattern, int col)
    {
        int N = pattern.GetLength(1);
        var arr = new Color[N];
        for (int i = 0; i < N; i++) arr[i] = pattern[col, i];
        return arr;
    }
    private static bool RowsEqual(Color[] a, Color[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }
    private static bool ColsEqual(Color[] a, Color[] b) => RowsEqual(a, b);

    private static string PatternHash(Color[,] pattern)
    {
        var sb = new System.Text.StringBuilder();
        for (int y = 0; y < pattern.GetLength(1); y++)
            for (int x = 0; x < pattern.GetLength(0); x++)
                sb.Append(pattern[x, y].ToString());
        return sb.ToString();
    }
}