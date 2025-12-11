using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] Grid grid;

    public readonly Map<Vector3, LevelObject> Objects = new();

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

    public Vector3Int WorldToCell(Vector3 position)
        => grid.WorldToCell(position);

    public Vector3 CellToWorld(Vector3Int position)
        => grid.GetCellCenterWorld(position);
}
