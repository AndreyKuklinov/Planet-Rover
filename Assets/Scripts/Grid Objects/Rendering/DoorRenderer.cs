using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRenderer : MonoBehaviour
{
    [SerializeField] DoorSpriteRepo spriteRepo;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Door door;
    [SerializeField] Item item;

    void Update()
    {
        var color = item.ColorData;
        var doorSprites = spriteRepo.GetSprites(color);
        var sprite = door.IsOpen ? doorSprites.OpenDoorSprite : doorSprites.ClosedDoorSprite;
        spriteRenderer.sprite = sprite;
    }
}
