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
        if (roomQueue.Count == 0)
            roomQueue = CreateRoomQueue();

        CurrentRoomData = roomQueue.Dequeue();
        roomLoader.LoadRoom(CurrentRoomData.Room);
    }

    private Queue<RoomData> CreateRoomQueue()
    {
        var rooms = currentLevel.IsOrderRandom
            ? currentLevel.Rooms.OrderBy(x => Random.value).ToArray()
            : currentLevel.Rooms;

        return new Queue<RoomData>(rooms);
    }
}
