using UnityEngine;
using UnityEngine.EventSystems;

public class WFCController : MonoBehaviour
{
    [Header("Config")]
    public TileSet tileSet;
    public int width = 16, height = 16;
    public float cellSize = 1f;
    public GameObject tilePrefab;

    [Header("Ejecución")]
    public bool autoRun = false;
    public float stepsPerSecond = 60f;

    private WFCGrid _grid;
    private WFC _wfc;
    private GameObject[,] _gos;
    private float _accum;

    void Start()
    {
        _grid = new WFCGrid(width, height, tileSet.tiles);
        _wfc = new WFC(tileSet, _grid);
        _gos = new GameObject[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var go = Instantiate(tilePrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity, transform);
                _gos[x, y] = go;
            }

        Redraw();
    }

    void Update()
    {
        // Pre-colapso con click (fija una celda a un tile si pasas el mouse por encima)
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(world.x / cellSize);
            int y = Mathf.RoundToInt(world.y / cellSize);
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                // fija el primer tile (o rota entre opciones con Shift)
                var c = _grid.cells[x, y];
                var idx = 0;
                if (Input.GetKey(KeyCode.LeftShift)) idx = (c.domain.Count > 1) ? 1 : 0;
                c.domain = new System.Collections.Generic.List<TileSet.TileDef> { c.domain[Mathf.Clamp(idx, 0, c.domain.Count - 1)] };
                Redraw();
            }
        }

        // Paso a paso
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_wfc.Step()) Redraw();
        }

        // Auto-run
        if (autoRun && !_wfc.IsDone())
        {
            _accum += Time.deltaTime * stepsPerSecond;
            while (_accum >= 1f && !_wfc.IsDone())
            {
                _wfc.Step();
                _accum -= 1f;
            }
            Redraw();
        }
    }

    private void Redraw()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var c = _grid.cells[x, y];
                var sr = _gos[x, y].GetComponent<SpriteRenderer>();
                if (c.domain.Count == 0) { sr.color = Color.red; sr.sprite = null; continue; }
                if (c.domain.Count == 1) { sr.color = Color.white; sr.sprite = c.domain[0].sprite; }
                else { sr.color = new Color(1f, 1f, 1f, 0.2f); sr.sprite = null; } // sin colapsar: “neblina”
            }
    }
}