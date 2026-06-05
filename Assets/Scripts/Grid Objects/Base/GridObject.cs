using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public event Action<GridObject> Destroyed;

    [field: SerializeField] public GridObjectData Data { get; private set; }

    public RoomGrid RoomGrid { get; private set; }

    public void AttachToGrid(RoomGrid grid = null)
    {
        if (grid != null)
            RoomGrid = grid;
        else if (RoomGrid == null)
            throw new ArgumentException("Level objects need a level grid when first created");

        transform.SetParent(RoomGrid.transform);
        RoomGrid.PlaceObject(this);
    }

    public void AttachToObject(Transform target)
    {
        transform.SetParent(target.transform);
        transform.position = target.position;
        RoomGrid.RemoveObject(this);
    }

    public bool TryRemoveFromGrid()
    {
        if (!RoomGrid.Objects.Contains(this))
            return false;

        RoomGrid.RemoveObject(this);
        return true;
    }

    private void OnDestroy()
    {
        Destroyed?.Invoke(this);
    }
}
