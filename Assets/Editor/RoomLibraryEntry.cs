using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal sealed class RoomLibraryEntry
{
    internal readonly Room room;
    internal readonly string id;
    internal readonly DateTime? date;
    internal string Title => string.IsNullOrWhiteSpace(room.LibraryTitle) ? id : room.LibraryTitle;
    sealed class Layer
    {
        public Texture2D texture;
        public Rect uv;
        public Vector2 topLeft, right, down;
        public Color tint;
        public int sortingLayer, order;
        public bool wall;
    }

    List<Layer> layers;
    Vector2Int min, max;
    string contents;
    internal string Summary
    {
        get
        {
            ReadLayout();
            return contents;
        }
    }
    internal string Size
    {
        get
        {
            ReadLayout();
            return $"{max.x - min.x + 1} × {max.y - min.y + 1}";
        }
    }

    internal RoomLibraryEntry(Room room, string path)
    {
        this.room = room;
        id = Path.GetFileNameWithoutExtension(path);
        var match = Regex.Match(id, @"^(\d{2}_\d{2}_\d{2})_\d{3,}(?:$|\D)");
        if (match.Success && DateTime.TryParseExact("20" + match.Groups[1].Value, "yyyy_MM_dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            date = parsed;
    }

    internal bool MatchesTags(IReadOnlyList<RoomLibraryTags.Category> categories, ICollection<string> filter)
    {
        foreach (var category in categories)
        {
            bool required = false, found = false;
            foreach (var tag in category.tags)
                if (filter.Contains(tag.id))
                {
                    required = true;
                    found |= tag.IsAssigned(room);
                }
            if (required && !found)
                return false;
        }
        return true;
    }

    void ReadLayout()
    {
        if (layers != null)
            return;
        layers = new List<Layer>();
        var counts = new SortedDictionary<string, int>();
        var grid = room.Grid;
        if (grid == null)
        {
            contents = "";
            return;
        }
        bool haveBounds = false;
        if (grid.Bounds != null)
        {
            // Bounds are world-space markers; only include cells with a center inside them.
            try
            {
                min = grid.WorldToCell(grid.Bounds.MinCoords);
                max = grid.WorldToCell(grid.Bounds.MaxCoords);
                var lo = grid.CellToWorld(min);
                var hi = grid.CellToWorld(max);
                if (lo.x < grid.Bounds.MinCoords.x)
                    min.x++;
                if (lo.y < grid.Bounds.MinCoords.y)
                    min.y++;
                if (hi.x > grid.Bounds.MaxCoords.x)
                    max.x--;
                if (hi.y > grid.Bounds.MaxCoords.y)
                    max.y--;
                haveBounds = max.x >= min.x && max.y >= min.y;
            }
            catch (NullReferenceException) { /* An unfinished room may have unassigned markers. */ }
        }
        foreach (var obj in grid.GetComponentsInChildren<GridObject>())
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(obj.gameObject);
            string name = source != null ? source.name : obj.name;
            bool wall = name == "Obstacle";
            var pos = grid.WorldToCell(obj.transform.position);
            if (!haveBounds)
            {
                min = max = pos;
                haveBounds = true;
            }
            min = Vector2Int.Min(min, pos);
            max = Vector2Int.Max(max, pos);
            foreach (var renderer in obj.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!renderer.enabled || renderer.forceRenderingOff || renderer.sprite == null
                    || renderer.GetComponentInParent<GridObject>(true) != obj)
                    continue;
                var sprite = renderer.sprite;
                Vector2 size = sprite.rect.size / sprite.pixelsPerUnit;
                Vector2 pivot = sprite.pivot / sprite.pixelsPerUnit;
                Vector2 flip = new Vector2(renderer.flipX ? -1 : 1, renderer.flipY ? -1 : 1);
                AddLayer(sprite, renderer.transform, Vector2.Scale(new Vector2(-pivot.x, size.y - pivot.y), flip),
                    Vector2.right * size.x * flip.x, Vector2.down * size.y * flip.y, renderer.color,
                    SortingLayer.GetLayerValueFromID(renderer.sortingLayerID), renderer.sortingOrder, wall);
            }
            var requests = obj.GetComponentInChildren<RocketIcons>();
            if (requests != null)
                ReadRocketIcons(requests);
            if (!wall && !obj.GetComponent<Rover>())
                counts[name] = counts.TryGetValue(name, out int n) ? n + 1 : 1;
        }
        layers = layers.OrderBy(l => l.sortingLayer).ThenBy(l => l.order).ToList();
        contents = string.Join(" · ", counts.Select(kv => $"{kv.Value} × {kv.Key}"));
    }

    void AddLayer(Sprite sprite, Transform transform, Vector2 corner, Vector2 right, Vector2 down,
        Color tint, int sortingLayer, int order, bool wall)
    {
        var texture = sprite.packed ? AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(sprite)) : sprite.texture;
        if (texture == null) return;
        var rect = sprite.rect;
        layers.Add(new Layer {
            texture = texture,
            uv = new Rect(rect.x / texture.width, rect.y / texture.height, rect.width / texture.width, rect.height / texture.height),
            topLeft = transform.TransformPoint(corner),
            right = transform.TransformVector(right), down = transform.TransformVector(down),
            tint = tint, sortingLayer = sortingLayer, order = order, wall = wall
        });
    }

    // RocketIcons creates its images in Start. Read the same requests, icon repository and layout in edit mode.
    void ReadRocketIcons(RocketIcons requests)
    {
        var settings = new SerializedObject(requests);
        var repo = settings.FindProperty("deliveryIconRepo").objectReferenceValue as DeliveryIconRepo;
        var collector = settings.FindProperty("collector").objectReferenceValue as ItemCollector;
        var image = settings.FindProperty("iconPrefab").objectReferenceValue as Image;
        var layout = requests.GetComponent<VerticalLayoutGroup>();
        if (repo == null || collector == null || image == null || layout == null) return;
        var frame = (RectTransform)layout.transform;
        var slot = image.rectTransform.rect.size;
        float y = frame.rect.yMax - layout.padding.top - slot.y / 2;
        foreach (var item in collector.RequiredItems)
        {
            var sprite = item != null ? repo.GetIcon(item) : null;
            if (sprite != null)
            {
                var size = image.preserveAspect ? sprite.rect.size * Mathf.Min(slot.x / sprite.rect.width, slot.y / sprite.rect.height) : slot;
                var corner = new Vector2(frame.rect.center.x - size.x / 2, y + size.y / 2);
                AddLayer(sprite, frame, corner, Vector2.right * size.x, Vector2.down * size.y, image.color, int.MaxValue, 0, false);
            }
            y -= slot.y + layout.spacing;
        }
    }

    internal void Draw(Rect rect, bool dimWalls)
    {
        if (Event.current.type != EventType.Repaint) return;
        ReadLayout();
        EditorGUI.DrawRect(rect, new Color(.12f, .16f, .18f));
        if (room.Grid == null || layers.Count == 0) return;
        int w = max.x - min.x + 1, h = max.y - min.y + 1;
        float side = Mathf.Min((rect.width - 6) / w, (rect.height - 6) / h);
        if (side <= 0) return;
        GUI.BeginClip(rect);
        var board = new Rect((rect.width - side * w) / 2, (rect.height - side * h) / 2, w * side, h * side);
        EditorGUI.DrawRect(board, new Color(.21f, .28f, .29f));
        Vector2 origin = room.Grid.CellToWorld(new Vector2Int(min.x, max.y));
        Vector2 screenOrigin = board.position + Vector2.one * (side / 2);
        var matrix = GUI.matrix;
        var color = GUI.color;
        try
        {
            foreach (var layer in layers)
            {
                Vector2 offset = layer.topLeft - origin;
                var corner = screenOrigin + new Vector2(offset.x, -offset.y) * side;
                var fit = new Rect(corner, new Vector2(layer.right.magnitude, layer.down.magnitude) * side);
                var uv = layer.uv;
                if (layer.right.x * layer.down.y > layer.right.y * layer.down.x)
                {
                    fit.y -= fit.height;
                    uv = new Rect(uv.x, uv.yMax, uv.width, -uv.height);
                }
                float angle = Mathf.Atan2(-layer.right.y, layer.right.x) * Mathf.Rad2Deg;
                if (!Mathf.Approximately(angle, 0f))
                    GUIUtility.RotateAroundPivot(angle, corner);
                var tint = color * layer.tint;
                if (dimWalls && layer.wall) tint.a *= .35f;
                GUI.color = tint;
                GUI.DrawTextureWithTexCoords(fit, layer.texture, uv);
                GUI.matrix = matrix;
            }
        }
        finally
        {
            GUI.matrix = matrix;
            GUI.color = color;
            GUI.EndClip();
        }
    }
}
