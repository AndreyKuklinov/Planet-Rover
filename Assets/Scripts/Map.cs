using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Map<TPosition, TObject>
{
    private readonly Dictionary<TPosition, TObject> posToObj = new();
    private readonly Dictionary<TObject, TPosition> objToPos = new();
    
    public void Add(TObject obj, TPosition pos)
    {
        posToObj[pos] = obj;
        objToPos[obj] = pos;
    }

    public void Remove(TObject obj)
    {
        if (!Contains(obj))
            throw new ArgumentException("Trying to delete an object that doesn't exist");

        posToObj.Remove(objToPos[obj]);
        objToPos.Remove(obj);
    }

    public bool Contains(TObject obj)
    {
        return objToPos.ContainsKey(obj);
    }

    public bool IsEmpty(TPosition pos)
    {
        return !posToObj.ContainsKey(pos) || posToObj[pos] == null;
    }

    public TObject GetObject(TPosition pos)
    {
        if (!posToObj.ContainsKey(pos))
            return default;

        return posToObj[pos];
    }

    public TPosition GetPosition(TObject obj)
    {
        if (!Contains(obj))
            throw new ArgumentException("Trying to get a position of an object that doesn't exist");

        return objToPos[obj];
    }
}
