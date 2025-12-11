using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] Grid grid;

    public readonly Map<Vector2, LevelObject> Objects = new();

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

    public List<LevelObject> GetObjectsOnLine(Vector2Int pos1, Vector2Int pos2)
    {
        var res = new List<LevelObject>();

        if (pos1.x == pos2.x)
        {
            for (var y = Mathf.Min(pos1.y, pos2.y); y <= Mathf.Max(pos1.y, pos2.y); y++)
            {
                var point = new Vector2(pos1.x, y);
                var obj = Objects.GetObject(point);
                if (obj != null)
                    res.Add(obj);
            }
        }

        else if (pos1.y == pos2.y)
        {
            for (var x = Mathf.Min(pos1.x, pos2.x); x <= Mathf.Max(pos1.x, pos2.x); x++)
            {
                var point = new Vector2(x, pos1.y);
                var obj = Objects.GetObject(point);
                if (obj != null)
                    res.Add(obj);
            }
        }

        else
        {
            throw new ArgumentException("Points are not on a line: " + pos1 + " " + pos2);
        }

        return res;
    }
}
