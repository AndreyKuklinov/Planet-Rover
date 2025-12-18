using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] Grid grid;

    public readonly Map<Vector2, LevelObject> Objects = new();
    public Vector2Int MinBounds = new(-99, -99);
    public Vector2Int MaxBounds = new(99, 99);

    public void PlaceObject(LevelObject obj)
    {
        var cell = WorldToCell(obj.transform.position);
        obj.transform.position = CellToWorld(cell);
        Objects.Add(obj, new Vector2(cell.x, cell.y));
    }

    public void RemoveObject(LevelObject obj)
    {
        Objects.Remove(obj);
    }

    public Vector3 SnapToGrid(Vector3 position)
    {
        var cell = WorldToCell(position);
        return CellToWorld(cell);
    }

    public Vector2Int WorldToCell(Vector3 position)
    {
        var pos = grid.WorldToCell(position);
        return new Vector2Int(pos.x, pos.y);
    }

    public Vector3 CellToWorld(Vector2Int position)
    {
        var pos = grid.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        return pos;
    }

    public static IEnumerable<Vector2Int> PointsOnLine(Vector2Int from, Vector2Int to)
    {
        if (from.x != to.x && from.y != to.y)
            throw new ArgumentException("Only horizontal or vertical lines supported.");

        var step = new Vector2Int(
            Mathf.Clamp(to.x - from.x, -1, 1),
            Mathf.Clamp(to.y - from.y, -1, 1)
        );

        for (var p = from; ; p += step)
        {
            yield return p;
            if (p == to)
                yield break;
        }
    }

    public List<LevelObject> GetObjectsOnLine(Vector2Int pos1, Vector2Int pos2)
    {
        return PointsOnLine(pos1, pos2)
            .Select(p => Objects.GetObject(p))
            .Where(o => o != null)
            .ToList();
    }

    public Vector2Int GetStoppingSquare(Vector2Int from, Direction dir, LevelObject grabbedObj)
    {
        var prev = from;
        var step = DirectionVector.GetVector2Int(dir);
        var point = from;

        while (IsWithinBounds(point))
        {
            var obj = Objects.GetObject(point);
            if (obj == null || obj.CanHandGoThrough)
            {
                prev = point;
                point += step;
                continue;
            }

            if (grabbedObj == null && obj.CanBeGrabbed || obj.CanBeDroppedOnto(grabbedObj))
            {
                return point;
            }

            return prev;
        }

        return point;
    }

    bool IsWithinBounds(Vector2Int square)
        => square.x > MinBounds.x && square.y > MinBounds.y && square.x < MaxBounds.x && square.y < MaxBounds.y;
}
