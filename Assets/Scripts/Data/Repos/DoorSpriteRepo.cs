using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Door Sprite Repo")]
public class DoorSpriteRepo : ScriptableObject
{
    [SerializeField] private List<DoorSprites> doorSprites = new();

    public DoorSprites GetSprites(ItemColorData colorData)
    {
        foreach (var doorSprite in doorSprites)
        {
            if (doorSprite.ColorData == colorData)
                return doorSprite;
        }

        throw new InvalidOperationException("No sprite found for door of color " + colorData);
    }

    [Serializable]
    public struct DoorSprites
    {
        public ItemColorData ColorData;
        public Sprite OpenDoorSprite;
        public Sprite ClosedDoorSprite;
    }
}

