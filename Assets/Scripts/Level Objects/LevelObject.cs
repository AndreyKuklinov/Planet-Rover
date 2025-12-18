using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour 
{
    [SerializeField] LevelObjectData data;
    [SerializeField] SpriteRenderer spriteRenderer;

    public bool CanHandGoThrough 
        => data.CanHandGoThrough;
    public bool CanBeGrabbed
        => data.CanBeGrabbed;

    public virtual bool CanBeDroppedOnto(LevelObject levelObject)
        => false;

    private LevelGrid grid;

    void Start()
    {
        spriteRenderer.sprite = data.Sprite;
        grid = FindObjectOfType<LevelGrid>();
        AttachToGrid();
    }

    void OnValidate()
    {
        if (spriteRenderer == null || data == null)
            return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            spriteRenderer.sprite = data.Sprite;
        };
    }

    public void AttachToGrid()
    {
        if (grid == null)
            return;

        transform.SetParent(grid.transform);
        grid.PlaceObject(this);
    }
    
    public void AttachToObject(Transform target)
    {
        transform.SetParent(target.transform);
        transform.position = target.position;
        grid.RemoveObject(this);
    }
}
