using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour 
{
    [field: SerializeField] public LevelObjectData Data { get; private set; }

    [SerializeField] SpriteRenderer spriteRenderer;

    public bool CanHandGoThrough 
        => Data.CanHandGoThrough;
    public bool CanBeGrabbed
        => Data.CanBeGrabbed;

    public virtual bool CanBeDroppedOnThis(LevelObject levelObject)
        => false;

    public virtual void DropOnThis(LevelObject levelObject) { }

    protected LevelGrid grid;

    protected virtual void Start()
    {
        spriteRenderer.sprite = Data.Sprite;
        grid = FindObjectOfType<LevelGrid>();
        AttachToGrid();
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
