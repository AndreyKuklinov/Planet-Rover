using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] RoomLoader roomLoader;

    private LevelData currentLevel;
    private Queue<Room> roomQueue;

    public void SetLevelData(LevelData levelData)
    {
        currentLevel = levelData;
        roomQueue = CreateRoomQueue();
    }

    public void LoadNextRoom()
    {
        if (roomQueue.Count == 0)
            roomQueue = CreateRoomQueue();

        roomLoader.LoadRoom(roomQueue.Dequeue());
    }

    private Queue<Room> CreateRoomQueue()
    {
        var levels = currentLevel.IsOrderRandom
            ? currentLevel.Levels.OrderBy(x => Random.value).ToArray()
            : currentLevel.Levels;

        return new Queue<Room>(levels);
    }
}
