using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Color Data")]
public class ItemColorData : ScriptableObject
{
    [field: SerializeField] public Color Color { get; private set; } 
}
