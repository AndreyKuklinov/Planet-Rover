using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Item Data Repo")]
public class ItemDataRepo : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    public ItemData GetItemData(ItemTypeData itemType, ItemColorData itemColor)
    {
        foreach (var item in items)
        {
            if(item.TypeData == itemType && item.ColorData == itemColor)
            {
                return item;
            }
        }

        throw new InvalidOperationException("No item found in repo with " + itemType + ", " + itemColor);
    }
}

