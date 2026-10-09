using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public bool IsRandomOrder { get; private set; }
    [SerializeField, Min(1)] private int roomCountToComplete = 1;
    [field: SerializeField] public Room[] Rooms { get; private set; }
    public int RoomCountToComplete => roomCountToComplete;
}