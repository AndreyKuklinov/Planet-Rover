using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Filled Bucket Sprite Repo")]
public class FilledBucketSpriteRepo : ScriptableObject
{
    [SerializeField] private List<Recipe> recipes = new();

    public Sprite GetSprite(LiquidData liquidData)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.Liquid == liquidData)
                return recipe.FilledBucketSprite;
        }

        throw new InvalidOperationException("No sprite found in repo to fill bucket with " + liquidData);
    }

    [Serializable]
    public struct Recipe
    {
        public LiquidData Liquid;
        public Sprite FilledBucketSprite;
    }
}

