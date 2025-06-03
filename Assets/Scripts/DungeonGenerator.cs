using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class DungeonGenerator : MonoBehaviour
{
    public Vector2Int startingRoomSize = new Vector2Int(100, 100);
    public Vector2Int minRoomSize = new Vector2Int(15, 15);
    public Vector2Int randomSplitOffset = new Vector2Int(-3, 3);
    public RectInt room;

    public List<RectInt> rooms = new List<RectInt>();
    public List<RectInt> completedRooms = new List<RectInt>();

    [Button("Split —")]
    public void ButtonSplitHorizontally()
    {
        if (rooms.Count > 0)
        {
            SplitHorizontally(rooms[0]);
        }
        else
        {
            Debug.LogWarning("No rooms to split. Create a main room.");
        }
    }

    [Button("Split |")]
    public void ButtonSplitVertically()
    {
        if (rooms.Count > 0)
        {
            SplitVertically(rooms[0]);
        }
        else
        {
            Debug.LogWarning("No rooms to split. Create a main room.");
        }
    }

    private WaitForSeconds oneSecondPause;
    private WaitForSeconds halfASecondPause;


    void Start()
    {
        oneSecondPause = new WaitForSeconds(1);
        halfASecondPause = new WaitForSeconds(0.5f);

        CreateMainRoom();
    }

    void Update()
    {
        foreach (var r in rooms)
        {
            AlgorithmsUtils.DebugRectInt(r, Color.red);
        }

        foreach (var r in completedRooms)
        {
            AlgorithmsUtils.DebugRectInt(r, Color.green);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SplitRooms();
        }
    }

    void CreateMainRoom()
    {
        room = new RectInt(0, 0, startingRoomSize.x, startingRoomSize.y);
        rooms.Add(room);
    }

    public void SplitRooms()
    {
        if (rooms[0].width > rooms[0].height)
        {
            // Split the room vertically if it is wider than it is tall
            SplitVertically(rooms[0]);
        }
        else
        {
            // Split the room horizontally if it is taller than it is wide
            SplitHorizontally(rooms[0]);
        }
    }

    public void SplitHorizontally(RectInt room)
    {
        int splitPoint;
        if (room.height <= 20)
        {
            splitPoint = room.y + room.height / 2 + Random.Range(randomSplitOffset.x / 2, randomSplitOffset.y / 2);
        }
        else
        {
            splitPoint = room.y + room.height / 2 + Random.Range(randomSplitOffset.x, randomSplitOffset.y);
        }

        RectInt topRoom = new RectInt(room.x, splitPoint, room.width, room.height - (splitPoint - room.y));
        RectInt bottomRoom = new RectInt(room.x, room.y, room.width, splitPoint - room.y + 1);
        rooms.RemoveAt(0);
        CheckRoomSize(topRoom);
        CheckRoomSize(bottomRoom);
    }

    public void SplitVertically(RectInt room)
    {
        int splitPoint;
        if (room.width <= 20)
        {
            splitPoint = room.x + room.width / 2 + Random.Range(randomSplitOffset.x / 2, randomSplitOffset.y / 2);
        }
        else
        {
            splitPoint = room.x + room.width / 2 + Random.Range(randomSplitOffset.x, randomSplitOffset.y);
        }
        
        RectInt leftRoom = new RectInt(room.x, room.y, splitPoint - room.x + 1, room.height);
        RectInt rightRoom = new RectInt(splitPoint, room.y, room.width - (splitPoint - room.x), room.height);
        rooms.RemoveAt(0);
        CheckRoomSize(leftRoom);
        CheckRoomSize(rightRoom);
    }

    public void CheckRoomSize(RectInt room)
    {
        if (room.width > minRoomSize.x && room.height > minRoomSize.y)
        {
            // If the room is larger than the minimum size, add it to the list of rooms to be split later
            rooms.Add(room);
            return;
        }
        completedRooms.Add(room);
        rooms.Remove(room);
    }
}
