using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Deliverable Icon Repo")]
public class DeliveryIconRepo : ScriptableObject
{
    [SerializeField] private List<Recipe> recipes = new();

    public Sprite GetIcon(ItemData itemData)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.ItemData == itemData)
                return recipe.Icon;
        }

        return itemData.MainSprite;
    }

    [Serializable]
    public struct Recipe
    {
        public ItemData ItemData;
        public Sprite Icon;
    }
}

