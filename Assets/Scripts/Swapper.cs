using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Swapper : MonoBehaviour
{
    [SerializeField] private RoomGrid levelGrid;

    public void SwapAllObjects()
    {
        var markersByIndex = FindObjectsOfType<SwapMarker>()
            .GroupBy(m => m.Index)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m => levelGrid.WorldToCell(m.transform.position)).ToList()
            );

        var levelObjects = FindObjectsOfType<LevelObject>();

        foreach (var markerGroup in markersByIndex.Values)
        {
            var markerSet = new HashSet<Vector2Int>(markerGroup);

            var objectsInGroup = levelObjects
                .Where(o => markerSet.Contains(levelGrid.WorldToCell(o.transform.position)))
                .ToList();

            if (objectsInGroup.Count == 0) continue;

            var shuffledCells = new List<Vector2Int>(markerGroup);
            Shuffle(shuffledCells);

            int count = Mathf.Min(objectsInGroup.Count, shuffledCells.Count);

            for (int i = 0; i < count; i++)
            {
                var pos = levelGrid.CellToWorld(shuffledCells[i]);
                objectsInGroup[i].transform.position = pos;
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}