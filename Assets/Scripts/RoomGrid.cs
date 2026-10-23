using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
            TrackObject(obj);
        }
    }

    public void TrackObject(GridObject obj)
    {
        obj.AttachToGrid(this);
        obj.Destroyed -= OnRoomObjectDestroyed;
        obj.Destroyed += OnRoomObjectDestroyed;
    }

    public void ForgetObject(GridObject obj)
    {
        obj.Destroyed -= OnRoomObjectDestroyed;
        if (Objects.Contains(obj)) Objects.Remove(obj);
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
        var point = from + step;

        while (IsWithinBounds(point))
        {
            var obj = Objects.GetObject(point);

            if (obj != null && obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough)
            {
                prev = point;
                point += step;
                continue;
            }

            if (obj != null && (
                (obj.TryGetComponent<IGrabbable>(out var grabb) && grabb.CanBeGrabbed) ||
                (obj.TryGetComponent<IInteractable>(out var rec) && rec.CanInteractWith(heldObj))
               ))
            {
                //Debug.Log("Stopped properly!");
                return point;
            }

            if (obj == null)
            {
                prev = point;
                point += step;
                continue;
            }

            return prev;
        }

        return prev;
    }

    public bool TryGetNextFreeSquare(Vector2Int from, Direction dir, out Vector2Int freeCell)
    {
        var step = DirectionVector.GetVector2Int(dir);
        freeCell = from + step;

        while (IsWithinBounds(freeCell))
        {
            var obj = Objects.GetObject(freeCell);
            if (obj == null)
                return true;

            if (!obj.TryGetComponent<IPassable>(out var pass) || !pass.CanHandPassThrough)
                return false;

            freeCell += step;
        }

        return false;
    }

    private void OnRoomObjectDestroyed(GridObject obj)
    {
        if (Objects.Contains(obj))
        {
            Objects.Remove(obj);
        }
    }
}
