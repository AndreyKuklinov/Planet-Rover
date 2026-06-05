using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SolidSample : GridObject, IGrabbable
{
    [field: SerializeField] public SampleData SampleData { get; private set; }

    [SerializeField] SpriteRenderer spriteRenderer;

    public GridObject GridObject =>
        this;

    private void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += UpdateSprite;
#endif
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null || SampleData == null || SampleData.Sprite == null )
            return;

//#if UNITY_EDITOR
//        Undo.RecordObject(spriteRenderer, "Update Sprite from ScriptableObject");
//#endif
        spriteRenderer.sprite = SampleData.Sprite;

//#if UNITY_EDITOR
//        PrefabUtility.RecordPrefabInstancePropertyModifications(spriteRenderer);
//#endif
    }
}
