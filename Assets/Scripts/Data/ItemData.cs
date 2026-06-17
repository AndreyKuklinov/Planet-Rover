using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [field: SerializeField] public ItemTypeData TypeData {  get; private set; }
    [field: SerializeField] public ItemColorData ColorData { get; private set; }
    [field: SerializeField] public Sprite MainSprite { get; private set; }
}
