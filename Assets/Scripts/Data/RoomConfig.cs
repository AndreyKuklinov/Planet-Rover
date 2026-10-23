using UnityEngine;

[CreateAssetMenu(fileName = "Room Config", menuName = "Rooms/Room Config")]
public class RoomConfig : ScriptableObject
{
    [SerializeField] bool isTimed = true;
    [SerializeField] bool canUseReset = true;
    public bool IsTimed => isTimed;
    public bool CanUseReset => canUseReset;
}
