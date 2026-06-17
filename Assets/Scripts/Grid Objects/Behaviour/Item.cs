using UnityEngine;

public class Item : MonoBehaviour, IHasItemData
{
    [field: SerializeField] public ItemData ItemData { get; private set; }
}