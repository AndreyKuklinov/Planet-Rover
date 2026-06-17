using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Containable Data")]
public class ContainableData : ScriptableObject
{
    [field: SerializeField] public bool IsLiquid { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
