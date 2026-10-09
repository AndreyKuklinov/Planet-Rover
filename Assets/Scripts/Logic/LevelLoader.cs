using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] RoomLoader roomLoader;
    private Queue<Room> roomQueue;
    public Room CurrentRoom { get; private set; }
    public float StandardTime { get; private set; }

    public void SetLevelData(LevelData level)
    {
        if (level.Rooms == null || level.Rooms.Length == 0
            || level.Rooms.Any(room => room == null) || level.RoomCountToComplete <= 0)
            throw new InvalidOperationException("Level must contain room prefabs and a positive room count.");

        var sequence = new List<Room>();
        Room previous = null;
        while (sequence.Count < level.RoomCountToComplete)
        {
            var batch = level.IsRandomOrder
                ? level.Rooms.OrderBy(_ => UnityEngine.Random.value).ToArray()
                : level.Rooms.ToArray();
            if (level.IsRandomOrder && batch.Length > 1 && batch[0] == previous)
                (batch[0], batch[batch.Length - 1]) = (batch[batch.Length - 1], batch[0]);
            foreach (var room in batch)
            {
                if (room.IsTimed && (room.StandardTime <= 0f
                    || float.IsNaN(room.StandardTime) || float.IsInfinity(room.StandardTime)))
                    throw new InvalidOperationException("Timed rooms must have a finite positive standard time.");
                sequence.Add(room);
                previous = room;
                if (sequence.Count == level.RoomCountToComplete)
                    break;
            }
        }
        StandardTime = sequence.Where(room => room.IsTimed).Sum(room => room.StandardTime);
        roomQueue = new Queue<Room>(sequence);
    }

    public void LoadNextRoom()
    {
        CurrentRoom = roomQueue.Dequeue();
        roomLoader.LoadRoom(CurrentRoom);
    }
}