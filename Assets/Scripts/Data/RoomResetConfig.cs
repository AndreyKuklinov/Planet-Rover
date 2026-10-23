using UnityEngine;

[CreateAssetMenu(fileName = "Room Reset Config", menuName = "Rover/Room Reset Config")]
public class RoomResetConfig : ScriptableObject
{
    [SerializeField, Min(0.01f)] float holdDuration = 2f;
    public float HoldDuration => holdDuration;
}
