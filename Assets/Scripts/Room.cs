using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Options")]
    [SerializeField] bool isTimed = true;
    [SerializeField] bool canUseReset = true;
    public bool IsTimed => isTimed;
    public bool CanUseReset => canUseReset;
    [field: SerializeField, Min(0.01f)] public float StandardTime { get; private set; } = 30f;

    [Header("Startup Reflections")]
    [SerializeField, Tooltip("50% chance to mirror the layout left-to-right each time this room starts.")]
    bool allowHorizontalReflection = true;
    [SerializeField, Tooltip("50% chance to mirror the layout top-to-bottom each time this room starts.")]
    bool allowVerticalReflection = true;
    public bool AllowHorizontalReflection => allowHorizontalReflection;
    public bool AllowVerticalReflection => allowVerticalReflection;
    public static event Action<Room> RoomStarted;

    [SerializeField] RoomGrid grid;
    public RoomGrid Grid => grid;
    bool restoredFromSnapshot;

    public void PrepareForReset() => restoredFromSnapshot = true;
    [SerializeField] Swapper swapper;
    [SerializeField] ItemColorDataEventChannel signalEmitted;
    [SerializeField] ItemColorDataEventChannel roomSignalChanged;

    void Awake()
    {
        signalEmitted.Raised += OnSignalEmitted;
    }

    void OnDestroy()
    {
        signalEmitted.Raised -= OnSignalEmitted;
    }

    void Start()
    {
        if (restoredFromSnapshot) return;
        InitRoom();
        RoomStarted?.Invoke(this);
    }

    void InitRoom()
    {
        if(swapper != null)
            swapper.SwapAllObjects();
        ApplyReflections(allowHorizontalReflection && UnityEngine.Random.value < 0.5f,
            allowVerticalReflection && UnityEngine.Random.value < 0.5f);
        grid.AttachAllObjects();
    }

    void ApplyReflections(bool horizontal, bool vertical)
    {
        if (!horizontal && !vertical) return;
        // Reflect cell indices, rather than scaling the room or using unsnapped marker positions.
        var min = grid.WorldToCell(grid.Bounds.MinCoords);
        var max = grid.WorldToCell(grid.Bounds.MaxCoords);
        var firstCenter = grid.CellToWorld(min);
        var lastCenter = grid.CellToWorld(max);
        if (firstCenter.x < grid.Bounds.MinCoords.x) min.x++;
        if (firstCenter.y < grid.Bounds.MinCoords.y) min.y++;
        if (lastCenter.x > grid.Bounds.MaxCoords.x) max.x--;
        if (lastCenter.y > grid.Bounds.MaxCoords.y) max.y--;
        var transforms = new HashSet<Transform>(grid.GetComponentsInChildren<GridObject>(true).Select(obj => obj.transform));
        foreach (var marker in grid.GetComponentsInChildren<SwapMarker>(true)) transforms.Add(marker.transform);
        // Snapshot all targets first so moving a parent cannot move a child twice.
        var targets = new List<(Transform transform, Vector3 position)>();
        foreach (var item in transforms)
        {
            var cell = grid.WorldToCell(item.position);
            if (horizontal) cell.x = min.x + max.x - cell.x;
            if (vertical) cell.y = min.y + max.y - cell.y;
            var position = grid.CellToWorld(cell);
            position.z = item.position.z;
            targets.Add((item, position));
        }
        foreach (var target in targets.OrderBy(target => GetDepth(target.transform)))
            target.transform.position = target.position;
    }

    static int GetDepth(Transform item)
    {
        int depth = 0;
        for (; item.parent != null; item = item.parent) depth++;
        return depth;
    }

    private void OnSignalEmitted(ItemColorData obj)
    {
        roomSignalChanged.Raise(obj);
    }
}
