using System.Collections.Generic;
using UnityEngine;

public enum LevelMode
{
    Tutorial = 1,
    Standard = 0,
    Challenge = 2
}

[CreateAssetMenu(menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, HideInInspector] List<string> libraryTagIds = new();
    public IReadOnlyList<string> LibraryTagIds => libraryTagIds;
#endif

    [SerializeField, Tooltip("Tutorial has no deadline. Standard uses the star thresholds. Challenge gives full credit for finishing within one standard time.")]
    LevelMode mode = LevelMode.Standard;
    public LevelMode Mode => mode;

    [field: SerializeField] public bool IsRandomOrder { get; private set; }
    [SerializeField] private bool beatAllRooms = true;
    [SerializeField, Min(1)] private int roomCountToComplete = 1;
    [field: SerializeField] public Room[] Rooms { get; private set; }
    public int RoomCountToComplete => beatAllRooms ? (Rooms?.Length ?? 0) : roomCountToComplete;
    public bool IsRoomTest { get; private set; }

    public static LevelData CreateRoomTest(Room room)
    {
        if (room == null) throw new System.ArgumentNullException(nameof(room));
        var level = CreateInstance<LevelData>();
        level.name = "Room Test - " + room.name;
        level.hideFlags = HideFlags.DontSave;
        level.Rooms = new[] { room };
        level.roomCountToComplete = 1;
        level.IsRoomTest = true;
        return level;
    }
}
