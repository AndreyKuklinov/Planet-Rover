using NaughtyAttributes;
using UnityEngine;

public class Item : MonoBehaviour, IHasItemData, IRecolorable
{
    [field: SerializeField] public ItemData ItemData { get; private set; }
    [SerializeField] ItemDataRepo itemDataRepo;
    [SerializeField] SpriteRenderer spriteRenderer;

    public ItemColorData ColorData
        => ItemData.ColorData;

    public bool CanBeRecolored(ItemColorData _)
    {
        return true;
    }

    public void Recolor(ItemColorData newColor)
    {
        ChangeData(ItemData.TypeData, newColor);
    }

    public void ChangeData(ItemTypeData typeData, ItemColorData colorData)
    {
        var newItemData = itemDataRepo.GetItemData(typeData, colorData);
        ItemData = newItemData;
        spriteRenderer.sprite = ItemData.MainSprite;
    }
}