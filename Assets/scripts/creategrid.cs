using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardFrameWorld : MonoBehaviour
{
    public LifeTilemapNoPreview engine;
    public Grid grid;
    public Tilemap tilemap;

    [Range(0.5f, 2.5f)] public float pixelThickness = 1.2f;
    public Color color = new(1, 1, 1, 0.35f);
    public int sortingOrder = 10;

    private LineRenderer lr;

    void Awake()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.alignment = LineAlignment.View;
        lr.numCornerVertices = 0;
        lr.numCapVertices = 0;
        lr.startColor = lr.endColor = color;

        var tr = tilemap.GetComponent<TilemapRenderer>();
        lr.sortingLayerID = tr ? tr.sortingLayerID : 0;
        lr.sortingOrder = sortingOrder;

        Rebuild();
    }

    void LateUpdate()
    {
        if (lr) lr.startWidth = lr.endWidth = pixelThickness;
    }

    public void Rebuild()
    {

        int w = Mathf.Max(1, GetInt(engine, "width", 64));
        int h = Mathf.Max(1, GetInt(engine, "height", 64));

        Vector3 cell = grid.cellSize;
        Vector3 anchor = tilemap.tileAnchor;

        Vector3 c00 = tilemap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
        Vector3 bl = c00 - new Vector3(cell.x * (0.5f - anchor.x), cell.y * (0.5f - anchor.y), 0f);
        Vector3 br = bl + new Vector3(w * cell.x, 0f, 0f);
        Vector3 trp = bl + new Vector3(w * cell.x, h * cell.y, 0f);
        Vector3 tl = bl + new Vector3(0f, h * cell.y, 0f);

        lr.positionCount = 5;
        lr.SetPosition(0, bl);
        lr.SetPosition(1, br);
        lr.SetPosition(2, trp);
        lr.SetPosition(3, tl);
        lr.SetPosition(4, bl);
    }

    int GetInt(object obj, string field, int def)
    {
        var fi = obj.GetType().GetField(field,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        return def;
    }
}
