using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] Grid grid;

    public readonly Map<Vector2, LevelObject> Objects = new();

    public void PlaceObject(LevelObject obj)
    {
        var cell = grid.WorldToCell(obj.transform.position);
        obj.transform.position = grid.CellToWorld(cell);
        Objects.Add(obj, new Vector2(cell.x, cell.y));
    }
}
