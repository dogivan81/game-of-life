using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class LifeTilemapNoPreview : MonoBehaviour
{
    [Serializable]
    public class PlayerStats
    {
        public int Start_token = 10;
        public int Score = 0;

    }

    [SerializeField] public Tilemap currentMap;
    [SerializeField] public Tilemap nextMap;

    [SerializeField] public Tile firstPlayerTile;
    [SerializeField] public Tile secondPlayerTile;
    private Tile aliveTile;
    private Tile deadTile;

    [SerializeField] public float tickSeconds = 0.2f;
    [SerializeField] public float minTick = 0.1f;
    [SerializeField] public float maxTick = 1.0f;
    private float time_step = 0.02f;

    [SerializeField] int width = 64;
    [SerializeField] int height = 64;
    [SerializeField] float randomScalar = 0.01f;

    [SerializeField] private RectTransform fieldUI;
    [SerializeField] private bool useUISpacePicking = true;

    private HashSet<Vector3Int> aliveCells = new();
    private HashSet<Vector3Int> cellsToCheck = new();

    private bool running = false;
    private float acc = 0f;

    public PlayerStats[] playerStats = { new PlayerStats(), new PlayerStats() };
    private bool beforeStart = true;


    private void Start()
    {
        aliveTile = firstPlayerTile;

        AlignTilemap(currentMap);
        AlignTilemap(nextMap);

        Clear();
        FitCameraToBoard();

        SetOnlyCurrentVisible();
    }


    private void Update()
    {
        InputHandler();
        if (!running) return;

        beforeStart = false;

        acc += Time.deltaTime;
        while (acc >= tickSeconds)
        {
            Step();
            acc -= tickSeconds;
        }
    }

    private void Step()
    {
        PrepareCellsToCheck();
        UpdateAndCheckcells();
    }

    private void PrepareCellsToCheck()
    {
        cellsToCheck.Clear();

        foreach (Vector3Int cell in aliveCells)
        {
            for (int dx = -1; dx <= 1; ++dx)
            {
                for (int dy = -1; dy <= 1; ++dy)
                {
                    var n = new Vector3Int(cell.x + dx, cell.y + dy, cell.z);
                    n = WrapCell(n);
                    cellsToCheck.Add(n);
                }
            }
        }
    }


    private void UpdateAndCheckcells()
    {
        foreach (Vector3Int cell in cellsToCheck)
        {
            (int, int) neighbours = CountAliveNeighbors(cell);
            int type = CheckCellType(cell);
            int count = neighbours.Item2 + neighbours.Item1;
            if ((type != 0) && (count > 3 || count < 2))
            {
                nextMap.SetTile(cell, deadTile);
                aliveCells.Remove(cell);
            }
            else if (type == 0 && (count == 3))
            {
                PaintCellWithFitColour(cell, neighbours.Item1, neighbours.Item2);
            }
            else if ((type != 0) && (count == 2 || count == 3))
            {
                PaintCellWithFitColour(cell, neighbours.Item1, neighbours.Item2);
            }
            else
            {

                nextMap.SetTile(cell, currentMap.GetTile(cell));
            }
        }

        Tilemap temp = currentMap;
        currentMap = nextMap;
        nextMap = temp;
        nextMap.ClearAllTiles();

        SetOnlyCurrentVisible();

    }

    (int, int) CountAliveNeighbors(Vector3Int cell)
    {
        int count_first = 0;
        int count_second = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int xnew = Wrap(cell.x + dx, width);
                int ynew = Wrap(cell.y + dy, height);

                int type = CheckCellType(new Vector3Int(xnew, ynew, cell.z));
                if (type == 1) count_first++;
                else if (type == 2) count_second++;
            }
        }
        return (count_first, count_second);
    }


    private int CheckCellType(Vector3Int cell)
    {
        if (currentMap.GetTile(cell) == firstPlayerTile) return 1;
        if (currentMap.GetTile(cell) == secondPlayerTile) return 2;
        return 0;
    }

    private void PaintCellWithFitColour(Vector3Int cell, int first_count, int second_count)
    {
        aliveTile = firstPlayerTile;
        if (first_count < second_count) aliveTile = secondPlayerTile;

        int type = -1;
        if (aliveTile == firstPlayerTile) type = 0;
        else if (aliveTile == secondPlayerTile) type = 1;

        if (currentMap.GetTile(cell) != aliveTile) playerStats[type].Score++;


        // Debug.Log($"Player 1: {playerStats[0].Score}, Player 2: {playerStats[1].Score}");
        nextMap.SetTile(cell, aliveTile);
        aliveCells.Add(cell);

    }

    private void Clear()
    {
        aliveCells.Clear();
        cellsToCheck.Clear();
        currentMap.ClearAllTiles();
        nextMap.ClearAllTiles();
        running = false;

        foreach (var ps in playerStats)
        {
            ps.Start_token = 10;
            ps.Score = 0;
        }

    }

    private void InputHandler()
    {

        if (Input.GetKeyDown(KeyCode.P)) running = !running;

        if (Input.GetKeyDown(KeyCode.R)) Randomize();

        if (Input.GetKeyDown(KeyCode.C)) Clear();

        if (Input.GetKeyDown(KeyCode.N)) Step();

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTileplayer(1);

        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTileplayer(2);


        if (Input.GetKeyDown(KeyCode.RightBracket)) SpeedUp();
        if (Input.GetKeyDown(KeyCode.LeftBracket)) SlowDown();

        if (!running)
        {
            if (Input.GetMouseButtonDown(0)) PaintUnderMouse(true);
            if (Input.GetMouseButtonDown(1)) PaintUnderMouse(false);
        }


    }

    private void SetTileplayer(int player_number)
    {
        if (player_number == 1) aliveTile = firstPlayerTile;
        if (player_number == 2) aliveTile = secondPlayerTile;

    }

    private void Randomize()
    {
        Clear();

        var rand = new System.Random();
        var cells = new List<Vector3Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells.Add(new Vector3Int(x, y, 0));

        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        foreach (var cell in cells)
        {
            if (rand.NextDouble() < randomScalar)
                changeTileByHand(true, cell);
        }

    }

    void SpeedUp()
    {
        if (tickSeconds - time_step > minTick) tickSeconds -= time_step;
        else tickSeconds = minTick;

    }

    void SlowDown()
    {
        if (tickSeconds + time_step < maxTick) tickSeconds += time_step;
        else tickSeconds = maxTick;
    }

    bool TryGetCellUnderMouse(out Vector3Int cell)
    {
        cell = Vector3Int.zero;

        var cam = Camera.main;
        if (!cam || !currentMap) return false;

        Vector3 vp = cam.ScreenToViewportPoint(Input.mousePosition);
        if (vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) return false;

        float z = Mathf.Abs(cam.transform.position.z);
        Vector3 world = cam.ViewportToWorldPoint(new Vector3(vp.x, vp.y, z));
        world.z = 0f;


        Grid grid = currentMap.layoutGrid ? currentMap.layoutGrid : GetComponentInParent<Grid>();
        Vector3 cs = grid ? (Vector3)grid.cellSize : Vector3.one;

        Vector3 c00 = currentMap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
        Vector3 bottomLeft = c00 - new Vector3(cs.x * 0.5f, cs.y * 0.5f, 0f);

        float dx = (world.x - bottomLeft.x) / cs.x;
        float dy = (world.y - bottomLeft.y) / cs.y;

        int x = Mathf.FloorToInt(dx);
        int y = Mathf.FloorToInt(dy);

        if (x < 0 || y < 0 || x >= width || y >= height) return false;

        cell = new Vector3Int(x, y, 0);
        return true;
    }



    void PaintUnderMouse(bool alive)
    {
        if (!TryGetCellUnderMouse(out var cell)) return;


        changeTileByHand(alive, cell);

    }

    private void changeTileByHand(bool alive, Vector3Int cell)
    {
        cell = WrapCell(cell);

        int type = -1;
        if (aliveTile == firstPlayerTile) type = 0;
        else if (aliveTile == secondPlayerTile) type = 1;

        if (alive)
        {
            if (currentMap.GetTile(cell) == aliveTile) return;
            if (beforeStart && playerStats[type].Start_token == 0) return;
            if (beforeStart) playerStats[type].Start_token--;
            currentMap.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }
        else
        {
            if (currentMap.GetTile(cell) == null) return;
            if (beforeStart) playerStats[type].Start_token++;
            if (beforeStart && playerStats[type].Start_token == 0) return;
            currentMap.SetTile(cell, null);
            aliveCells.Remove(cell);
        }
    }


    void FitCameraToBoard()
    {
        var cam = Camera.main;

        float cx = width * 0.5f - 0.5f;
        float cy = height * 0.5f - 0.5f;

        float sw = Screen.width, sh = Screen.height;
        var r = cam.rect;
        float aspect = (r.width * sw) / (r.height * sh);
        float sizeByH = height * 0.5f;
        float sizeByW = (width * 0.5f) / Mathf.Max(0.0001f, aspect);
        cam.orthographicSize = Mathf.Max(1f, Mathf.Max(sizeByH, sizeByW));

        int PPU = 32;
        float snapX = Mathf.Round(cx * PPU) / PPU;
        float snapY = Mathf.Round(cy * PPU) / PPU;
        cam.transform.position = new Vector3(snapX, snapY, -10f);
    }


    public bool IsRunning => running;
    public float TickSeconds
    {
        get => tickSeconds;
        set => tickSeconds = Mathf.Clamp(value, minTick, maxTick);
    }
    public float MinTick => minTick;
    public float MaxTick => maxTick;

    public void Play() { running = true; beforeStart = false; }
    public void Pause() { running = false; }
    public void TogglePlay() { running = !running; if (running) beforeStart = false; }

    public void DoStep()
    {
        PrepareCellsToCheck();
        UpdateAndCheckcells();
    }

    public void DoRandomize()
    {
        Randomize();
    }
    public void DoClear()
    {
        Clear();
    }

    public void SelectPlayer(int playerNumber)
    {
        SetTileplayer(playerNumber);
    }

    public void SetRandomScalar(float value) { randomScalar = Mathf.Clamp01(value); }

    public (int tokens1, int score1, int tokens2, int score2) GetStats()
    {
        return (playerStats[0].Start_token, playerStats[0].Score,
                playerStats[1].Start_token, playerStats[1].Score);
    }

    void AlignTilemap(Tilemap tm)
    {

        var tr = tm.transform;
        tr.position = Vector3.zero;
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;

        tm.tileAnchor = Vector3.zero;
        tm.origin = Vector3Int.zero;
        tm.orientation = Tilemap.Orientation.XY;
        tm.orientationMatrix = Matrix4x4.identity;

        var r = tm.GetComponent<TilemapRenderer>();
        if (r)
        {
            r.mode = TilemapRenderer.Mode.Chunk;
        }
    }

    void SetOnlyCurrentVisible()
    {
        var rCur = currentMap ? currentMap.GetComponent<TilemapRenderer>() : null;
        var rNext = nextMap ? nextMap.GetComponent<TilemapRenderer>() : null;

        if (rCur) rCur.enabled = true;
        if (rNext) rNext.enabled = false;
    }

    int Wrap(int v, int max) { v %= max; if (v < 0) v += max; return v; }
    Vector3Int WrapCell(Vector3Int c) => new Vector3Int(Wrap(c.x, width), Wrap(c.y, height), 0);


}