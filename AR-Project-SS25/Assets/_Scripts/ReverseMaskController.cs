using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ReverseMaskController : MonoBehaviour
{
    public RectTransform transparentRect; // The area to make see-through
    public Canvas canvas;

    private Material runtimeMat;

    void Start()
    {
        // Use a copy of the material
        RawImage img = GetComponent<RawImage>();
        runtimeMat = Instantiate(img.material);
        img.material = runtimeMat;
    }

    void Update()
    {
        if (!transparentRect || !canvas) return;

        Vector3[] corners = new Vector3[4];
        transparentRect.GetWorldCorners(corners);

        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;

        Vector2 bottomLeft = WorldToCanvasNormalized(corners[0]);
        Vector2 topRight = WorldToCanvasNormalized(corners[2]);

        runtimeMat.SetVector("_MaskRect", new Vector4(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x,
            topRight.y
        ));
    }

    Vector2 WorldToCanvasNormalized(Vector3 worldPos)
    {
        Vector2 viewportPoint = Camera.main.ScreenToViewportPoint(worldPos);
        return viewportPoint;
    }
}