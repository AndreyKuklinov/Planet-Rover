using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] RoomLoader roomLoader;

    public RoomData CurrentRoomData { get; private set; }

    private LevelData currentLevel;
    private Queue<RoomData> roomQueue;

    public void SetLevelData(LevelData levelData)
    {
        currentLevel = levelData;
        roomQueue = CreateRoomQueue();
    }

    public void LoadNextRoom()
    {
        if (roomQueue == null || roomQueue.Count == 0)
            roomQueue = CreateRoomQueue();

        CurrentRoomData = roomQueue.Dequeue();
        roomLoader.LoadRoom(CurrentRoomData.Room);
    }

    private Queue<RoomData> CreateRoomQueue()
    {
        if (!currentLevel.IsOrderRandom)
        {
            return new Queue<RoomData>(currentLevel.Rooms);
        }

        var rooms = currentLevel.Rooms.OrderBy(x => Random.value).ToArray();

        if (rooms.Length > 1 && CurrentRoomData != null && rooms[0] == CurrentRoomData)
        {
            var lastIndex = rooms.Length - 1;
            (rooms[lastIndex], rooms[0]) = (rooms[0], rooms[lastIndex]);
        }

        return new Queue<RoomData>(rooms);
    }
}