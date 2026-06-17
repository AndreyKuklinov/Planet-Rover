using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Container Fill Repo")]
public class FilledContainerSpriteRepo : ScriptableObject
{
    [SerializeField] private List<Recipe> recipes = new();

    public Sprite GetSprite(ItemContainerData container, ContainableData content)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.Container == container && recipe.Content == content)
                return recipe.Result;
        }

        throw new InvalidOperationException("No recipe found in repo to fill " 
            + container + " with " + content);
    }

    [Serializable]
    public struct Recipe
    {
        public ItemContainerData Container;
        public ContainableData Content;
        public Sprite Result;
    }
}

