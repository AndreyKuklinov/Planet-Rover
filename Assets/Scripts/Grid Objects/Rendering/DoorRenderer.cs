using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRenderer : MonoBehaviour
{
    [SerializeField] Sprite openSprite;
    [SerializeField] Sprite closedSprite;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PassableWithSignal passable;

    void Update()
    {
        var sprite = passable.CanHandPassThrough ? openSprite : closedSprite;
        spriteRenderer.sprite = sprite;
    }
}
