using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Grid Object Data")]
public class GridObjectData : ScriptableObject
{
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
