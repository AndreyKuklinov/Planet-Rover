using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounds : MonoBehaviour
{
    [SerializeField] Transform bottomLeftMarker;
    [SerializeField] Transform topRightMarker;

    public Vector2 MaxCoords
        => topRightMarker.transform.position;

    public Vector2 MinCoords
        => bottomLeftMarker.transform.position;

    public Vector2 Size
        => MaxCoords - MinCoords;

    public Vector2 Center
        => (MinCoords + MaxCoords) / 2f;

    public bool IsWithinBounds(Vector2 cell)
        => cell.x <= MaxCoords.x
        && cell.y <= MaxCoords.y
        && cell.x >= MinCoords.x
        && cell.y >= MinCoords.y;
}
