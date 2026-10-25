using UnityEngine;
using UnityEngine.UI;

// A world-space circle: a faint outline with a clockwise white pie fill.
[RequireComponent(typeof(CanvasRenderer))]
public class HandResetProgressIndicator : MaskableGraphic
{
    const int Segments = 64;
    float progress;
    public float Progress => progress;

    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(progress, value)) return;
        progress = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var rect = GetPixelAdjustedRect();
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        float innerRadius = radius * 0.88f;
        var outline = color;
        outline.a *= 0.4f;
        for (int i = 0; i < Segments; i++)
        {
            int vertex = vh.currentVertCount;
            vh.AddVert(Point(rect.center, radius, (float)i / Segments), outline, Vector2.zero);
            vh.AddVert(Point(rect.center, radius, (float)(i + 1) / Segments), outline, Vector2.zero);
            vh.AddVert(Point(rect.center, innerRadius, (float)(i + 1) / Segments), outline, Vector2.zero);
            vh.AddVert(Point(rect.center, innerRadius, (float)i / Segments), outline, Vector2.zero);
            vh.AddTriangle(vertex, vertex + 1, vertex + 2);
            vh.AddTriangle(vertex, vertex + 2, vertex + 3);
        }
        int count = Mathf.CeilToInt(progress * Segments);
        if (count == 0) return;
        int center = vh.currentVertCount;
        vh.AddVert(rect.center, color, Vector2.zero);
        for (int i = 0; i <= count; i++)
            vh.AddVert(Point(rect.center, innerRadius * 0.94f, Mathf.Min((float)i / Segments, progress)), color, Vector2.zero);
        for (int i = 0; i < count; i++)
            vh.AddTriangle(center, center + i + 1, center + i + 2);
    }

    static Vector3 Point(Vector2 center, float radius, float fraction)
    {
        float angle = Mathf.PI * 0.5f - fraction * Mathf.PI * 2f;
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
