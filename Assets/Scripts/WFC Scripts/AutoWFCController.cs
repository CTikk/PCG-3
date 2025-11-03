// Assets/Scripts/WFC/AutoWFCController.cs
using UnityEngine;
using System.Linq;

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

    private void Start()
    {
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
}