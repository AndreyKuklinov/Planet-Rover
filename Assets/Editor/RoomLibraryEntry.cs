using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal sealed class RoomLibraryEntry
{
    internal readonly Room room;
    internal readonly string id;
    internal readonly DateTime? date;
    internal string Title => string.IsNullOrWhiteSpace(room.LibraryTitle) ? id : room.LibraryTitle;
    readonly struct Cell
    {
        public readonly Vector2Int pos;
        public readonly bool wall, planter;
        public readonly Sprite sprite;
        public Cell(Vector2Int pos, bool wall, bool planter, Sprite sprite)
        {
            this.pos = pos;
            this.wall = wall;
            this.planter = planter;
            this.sprite = sprite;
        }
    }
    List<Cell> cells;
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
        if (cells != null)
            return;
        cells = new List<Cell>();
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
            bool planter = obj.TryGetComponent<PlantingPlatform>(out _);
            Sprite sprite = null;
            if (planter && obj.TryGetComponent<ItemCollector>(out var collector))
                sprite = collector.RequiredItems?.FirstOrDefault()?.MainSprite;
            else if (obj.TryGetComponent<Item>(out var item))
                sprite = item.ItemData?.MainSprite;
            if (sprite == null)
                sprite = obj.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r => r.enabled && r.sprite != null)?.sprite;
            var pos = grid.WorldToCell(obj.transform.position);
            if (!haveBounds && cells.Count == 0)
                min = max = pos;
            min = Vector2Int.Min(min, pos);
            max = Vector2Int.Max(max, pos);
            cells.Add(new Cell(pos, wall, planter, sprite));
            if (!wall && !obj.GetComponent<Rover>())
                counts[name] = counts.TryGetValue(name, out int n) ? n + 1 : 1;
        }
        contents = string.Join(" · ", counts.Select(kv => $"{kv.Value} × {kv.Key}"));
    }

    internal void Draw(Rect rect, bool dimWalls)
    {
        if (Event.current.type != EventType.Repaint)
            return;
        ReadLayout();
        EditorGUI.DrawRect(rect, new Color(.12f, .16f, .18f));
        int w = max.x - min.x + 1, h = max.y - min.y + 1;
        float side = Mathf.Min((rect.width - 6) / Mathf.Max(1, w), (rect.height - 6) / Mathf.Max(1, h));
        if (side <= 0)
            return;
        var board = new Rect(rect.center.x - side * w / 2, rect.center.y - side * h / 2, w * side, h * side);
        EditorGUI.DrawRect(board, new Color(.21f, .28f, .29f));
        foreach (var cell in cells)
        {
            var r = new Rect(board.x + (cell.pos.x - min.x) * side, board.y + (max.y - cell.pos.y) * side, side, side);
            if (cell.wall)
            {
                EditorGUI.DrawRect(r, dimWalls ? new Color(.27f, .33f, .34f) : new Color(.48f, .56f, .57f));
                continue;
            }
            if (cell.planter)
            {
                EditorGUI.DrawRect(r, new Color(.93f, .84f, .67f));
                r = new Rect(r.x + side * .12f, r.y + side * .12f, side * .76f, side * .76f);
                EditorGUI.DrawRect(r, new Color(.35f, .28f, .22f));
            }
            if (cell.sprite == null)
            {
                EditorGUI.DrawRect(new Rect(r.center.x - side * .2f, r.center.y - side * .2f, side * .4f, side * .4f), Color.white);
                continue;
            }
            var sprite = cell.sprite;
            var uv = sprite.rect;
            var texture = sprite.texture;
            float scale = Mathf.Min(r.width / uv.width, r.height / uv.height);
            var fit = new Rect(r.center.x - uv.width * scale / 2, r.center.y - uv.height * scale / 2, uv.width * scale, uv.height * scale);
            GUI.DrawTextureWithTexCoords(fit, texture, new Rect(uv.x / texture.width, uv.y / texture.height, uv.width / texture.width, uv.height / texture.height));
        }
    }
}
