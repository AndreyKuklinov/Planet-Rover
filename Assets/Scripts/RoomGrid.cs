using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    [field: SerializeField] public Bounds Bounds { get; private set; }

    [SerializeField] Grid grid;

    public readonly Map<Vector2Int, LevelObject> Objects = new();

    void Start()
    {
        LevelObject.Destroyed += OnLevelObjectDestroyed;
    }

    public void AttachAllObjects()
    {
        var levelObjects = GetComponentsInChildren<LevelObject>(true);
        foreach (var obj in levelObjects)
            obj.AttachToGrid(this);
    }

    public void PlaceObject(LevelObject obj)
    {
        var cell = WorldToCell(obj.transform.position);

        if (!IsWithinBounds(cell))
            throw new InvalidOperationException("Trying to place an object outside of bounds: " + cell);

        obj.transform.position = CellToWorld(cell);
        Objects.Add(obj, cell);
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

    public bool IsWithinBounds(Vector2Int cell)
        => Bounds.IsWithinBounds(CellToWorld(cell));

    public Vector2Int GetStoppingSquare(Vector2Int from, Direction dir, LevelObject heldObj)
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

            if (obj.CanBeGrabbed || obj.CanReceive(heldObj))
            {
                return point;
            }

            return prev;
        }

        return prev;
    }

    private void OnLevelObjectDestroyed(LevelObject obj)
    {
        if(Objects.Contains(obj))
        {
            Objects.Remove(obj);
        }
    }
}
