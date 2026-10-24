using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public bool IsRandomOrder { get; private set; }
    [SerializeField, Min(1)] private int roomCountToComplete = 1;
    [field: SerializeField] public Room[] Rooms { get; private set; }
    public int RoomCountToComplete => roomCountToComplete;
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
