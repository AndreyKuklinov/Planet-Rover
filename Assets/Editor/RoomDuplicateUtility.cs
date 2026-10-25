using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RoomDuplicateUtility
{
    public sealed class Overlap
    {
        public Vector2Int Cell;
        public readonly List<GridObject> Objects = new();
    }

    public static List<Overlap> FindOverlaps(Room room)
    {
        var cells = new Dictionary<Vector2Int, Overlap>();
        if (room == null || room.Grid == null
            || new SerializedObject(room.Grid).FindProperty("grid").objectReferenceValue == null)
            return new List<Overlap>();
        foreach (var obj in room.Grid.GetComponentsInChildren<GridObject>(true))
        {
            if (!WillBeActive(obj, room.transform)) continue;
            var cell = room.Grid.WorldToCell(obj.transform.position);
            if (!cells.TryGetValue(cell, out var overlap))
            {
                overlap = new Overlap { Cell = cell };
                cells.Add(cell, overlap);
            }
            overlap.Objects.Add(obj);
        }
        return cells.Values.Where(cell => cell.Objects.Count > 1).OrderBy(cell => cell.Cell.y).ThenBy(cell => cell.Cell.x).ToList();
    }

    static bool WillBeActive(GridObject obj, Transform room)
    {
        if (!obj.enabled) return false;
        for (var parent = obj.transform; parent != null; parent = parent.parent)
        {
            if (!parent.gameObject.activeSelf) return false;
            if (parent == room) break;
        }
        return true;
    }

    public static string ObjectPath(GridObject obj, Room room)
    {
        var parts = new List<string>();
        for (var item = obj.transform; item != null && item != room.transform; item = item.parent)
            parts.Add(item.name + " [" + item.GetSiblingIndex() + "]");
        parts.Reverse();
        return string.Join("/", parts);
    }

    public static string Describe(Room room, IEnumerable<Overlap> overlaps)
        => string.Join("\n", overlaps.Select(overlap => $"Cell ({overlap.Cell.x}, {overlap.Cell.y}): "
            + string.Join(" overlaps ", overlap.Objects.Select(obj => "'" + ObjectPath(obj, room) + "'"))));

}
