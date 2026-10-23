using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RoomLoader : MonoBehaviour
{
    public event Action LoadedRoom;

    public Room CurrentRoom { get; private set; }
    [SerializeField] RoomResetConfig resetConfig;
    public RoomResetController ResetController { get; private set; }

    void Awake()
    {
        ResetController = GetComponent<RoomResetController>();
        if (ResetController == null) ResetController = gameObject.AddComponent<RoomResetController>();
        ResetController.Initialize(this, resetConfig);
    }

    public void SetResetRoom(Room room) => CurrentRoom = room;

    public void LoadRoom(Room room)
    {
        ResetController.ClearSnapshot();
        if (CurrentRoom != null)
            Destroy(CurrentRoom.gameObject);

        CurrentRoom = Instantiate(room, transform);
        LoadedRoom?.Invoke();
    }
}
