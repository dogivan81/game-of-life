using UnityEngine;
using UnityEngine.Tilemaps;

public class GridToFieldFitter : MonoBehaviour
{
    [SerializeField] private LifeTilemapNoPreview engine;


    void LateUpdate()
    {

        var cur = engine.currentMap;
        var nxt = engine.nextMap;

        AlignTilemap(cur);
        AlignTilemap(nxt);

        SetOnlyCurrentVisible(cur, nxt);
    }

    private void AlignTilemap(Tilemap tm)
    {
        var tr = tm.transform;
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;

        tm.tileAnchor = Vector3.zero;
        tm.origin = Vector3Int.zero;
        tm.orientation = Tilemap.Orientation.XY;
        tm.orientationMatrix = Matrix4x4.identity;

        var r = tm.GetComponent<TilemapRenderer>();
        if (r) r.mode = TilemapRenderer.Mode.Chunk;
    }

    private void SetOnlyCurrentVisible(Tilemap cur, Tilemap nxt)
    {
        var rc = cur.GetComponent<TilemapRenderer>();
        var rn = nxt.GetComponent<TilemapRenderer>();
        if (rc) rc.enabled = true;
        if (rn) rn.enabled = false;
    }
}
