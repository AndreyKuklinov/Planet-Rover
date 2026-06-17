using UnityEngine;

public class RecolorableItem : MonoBehaviour, IRecolorable
{
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ItemDataRepo itemDataRepo;

    public ItemColorData CurrentColor
        => item.ItemData.ColorData;

    public bool CanBeRecolored(ItemColorData _)
    {
        return true;
    }

    public void Recolor(ItemColorData newColor)
    {
        var t = item.ItemData.TypeData;
        var newItemData = itemDataRepo.GetItemData(t, newColor);
        item.ItemData = newItemData;
        spriteRenderer.sprite = item.ItemData.MainSprite;
    }
}