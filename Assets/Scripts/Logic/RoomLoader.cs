using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RoomLoader : MonoBehaviour
{
    public Room CurrentRoom { get; private set; }

    public void LoadRoom(Room room)
    {
        if (CurrentRoom != null)
            Destroy(CurrentRoom.gameObject);

        CurrentRoom = Instantiate(room, transform);
    }
}
