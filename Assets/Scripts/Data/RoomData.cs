using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Room Data")]
public class RoomData : ScriptableObject
{
    [field: SerializeField] public Room Room { get; private set; }
    [field: SerializeField] public float BaseTimeLimit { get; private set; }
}
