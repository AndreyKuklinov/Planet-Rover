using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounds : MonoBehaviour
{
    [SerializeField] Transform bottomLeftMarker;
    [SerializeField] Transform topRightMarker;
    [SerializeField] Grid grid;

    public Vector2Int MaxCoords
        => GetCell(topRightMarker.transform.position);

    public Vector2Int MinCoords
        => GetCell(bottomLeftMarker.transform.position);

    public bool IsWithinBounds(Vector2Int cell)
        => cell.x <= MaxCoords.x
        && cell.y <= MaxCoords.y
        && cell.x >= MinCoords.x
        && cell.y >= MinCoords.y;

    Vector2Int GetCell(Vector2 pos)
    {
        var cell = grid.WorldToCell(pos);
        return new Vector2Int(cell.x, cell.y);
    }
}
