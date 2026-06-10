using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRenderer : MonoBehaviour
{
    [SerializeField] Sprite openSprite;
    [SerializeField] Sprite closedSprite;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Door door;

    void Update()
    {
        var sprite = door.IsOpen ? openSprite : closedSprite;
        spriteRenderer.sprite = sprite;
    }
}
