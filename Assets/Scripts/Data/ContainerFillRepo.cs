using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Container Fill Repo")]
public class ContainerFillRepo : ScriptableObject
{
    [SerializeField] private List<Recipe> recipes = new();

    public FillData GetFillData(ItemContainerData container, ContainableData content)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.Container == container && recipe.Container == container)
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
        public FillData Result;
    }

    [Serializable]
    public struct FillData
    {
        public Sprite FilledSprite;
        public DeliverableData DeliverableData;
    }
}

