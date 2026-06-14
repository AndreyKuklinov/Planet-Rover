using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Item Container Data")]
public class ItemContainerData : ScriptableObject
{
    [field: SerializeField] public Sprite EmptySprite { get; private set; }
}
