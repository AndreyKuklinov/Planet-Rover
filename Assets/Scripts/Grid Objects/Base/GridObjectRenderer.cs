using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridObjectRenderer : MonoBehaviour
{
    [SerializeField] GridObject gridObject;
    [SerializeField] SpriteRenderer spriteRenderer;

    GridObjectData Data
        => gridObject.Data;

    private void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += UpdateSprite;
#endif
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null || Data == null || Data.Sprite == null)
            return;

        spriteRenderer.sprite = Data.Sprite;
    }
}
