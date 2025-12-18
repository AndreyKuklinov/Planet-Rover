using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Level Object")]
public class LevelObjectData : ScriptableObject
{
    [field: SerializeField] public bool CanHandGoThrough { get; private set; }
    [field: SerializeField] public bool CanBeGrabbed { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
