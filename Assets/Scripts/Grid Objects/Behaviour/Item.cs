using UnityEngine;
using static UnityEditor.Progress;

public class Item : MonoBehaviour, IHasItemData, IRecolorable
{
    [field: SerializeField] public ItemData ItemData { get; private set; }
    [SerializeField] ItemDataRepo itemDataRepo;
    [SerializeField] SpriteRenderer spriteRenderer;

    public ItemColorData CurrentColor
        => ItemData.ColorData;

    public bool CanBeRecolored(ItemColorData _)
    {
        return true;
    }

    public void Recolor(ItemColorData newColor)
    {
        var t = ItemData.TypeData;
        var newItemData = itemDataRepo.GetItemData(t, newColor);
        ItemData = newItemData;
        spriteRenderer.sprite = ItemData.MainSprite;
    }
}