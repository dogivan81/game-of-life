using UnityEngine;

public class CameraToUIViewport : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform boardArea;

    Camera cam; Rect lastGood = new Rect(0, 0, 1, 1); bool hasGood;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.rect = lastGood;
    }

    void LateUpdate()
    {
        Canvas.ForceUpdateCanvases();

        Vector3[] w = new Vector3[4];
        boardArea.GetWorldCorners(w);
        Vector2 s0 = RectTransformUtility.WorldToScreenPoint(null, w[0]);
        Vector2 s2 = RectTransformUtility.WorldToScreenPoint(null, w[2]);

        float sw = Mathf.Max(1, Screen.width), sh = Mathf.Max(1, Screen.height);
        float vx = Mathf.Clamp01(Mathf.Min(s0.x, s2.x) / sw);
        float vy = Mathf.Clamp01(Mathf.Min(s0.y, s2.y) / sh);
        float vw = Mathf.Clamp01(Mathf.Abs(s2.x - s0.x) / sw);
        float vh = Mathf.Clamp01(Mathf.Abs(s2.y - s0.y) / sh);

        Rect vr = new Rect(vx, vy, vw, vh);
        cam.rect = vr; lastGood = vr; hasGood = true;

        var engine = FindObjectOfType<LifeTilemapNoPreview>();
        if (engine)
        {
            engine.SendMessage("FitCameraToBoard", SendMessageOptions.DontRequireReceiver);
        }
    }
}
