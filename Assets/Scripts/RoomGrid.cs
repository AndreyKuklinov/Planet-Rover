using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    [field: SerializeField] public Bounds Bounds { get; private set; }

    [SerializeField] Grid grid;

    public readonly Map<Vector2Int, GridObject> Objects = new();

    public void AttachAllObjects()
    {
        var gridObjects = GetComponentsInChildren<GridObject>(true);
        foreach (var obj in gridObjects)
        {
            if (!obj.isActiveAndEnabled)
                continue;
            obj.AttachToGrid(this);
            obj.Destroyed += OnRoomObjectDestroyed;
        }
    }

    public void PlaceObject(GridObject obj)
    {
        var cell = WorldToCell(obj.transform.position);

        if (!IsWithinBounds(cell))
            throw new InvalidOperationException("Trying to place an object outside of bounds: " + cell);

        obj.transform.position = CellToWorld(cell);
        Objects.Add(obj, cell);
    }

    public void RemoveObject(GridObject obj)
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

    public Vector2Int GetStoppingSquare(Vector2Int from, Direction dir, IGrabbable heldObj)
    {
        var prev = from;
        var step = DirectionVector.GetVector2Int(dir);
        var point = from;

        while (IsWithinBounds(point))
        {
            var obj = Objects.GetObject(point);
            if (obj == null 
                || obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough)
            {
                prev = point;
                point += step;
                continue;
            }

            if (obj.TryGetComponent<IGrabbable>(out var grabb) && grabb.CanBeGrabbed 
                || obj.TryGetComponent<IGrabbableReceived>(out var rec) && rec.CanReceive(heldObj))
            {
                return point;
            }

            return prev;
        }

        return prev;
    }

    private void OnRoomObjectDestroyed(GridObject obj)
    {
        if(Objects.Contains(obj))
        {
            Objects.Remove(obj);
        }
    }
}
