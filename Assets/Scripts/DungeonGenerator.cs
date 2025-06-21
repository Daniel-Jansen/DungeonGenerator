using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public Vector2Int startingRoomSize = new Vector2Int(100, 100);
    public Vector2Int minRoomSize = new Vector2Int(10, 10);
    public int randomSplitOffset = 3;

    [Header("Dungeon Generation")]
    [OnValueChanged("GenerationTypeChanged")]
    [Dropdown("GetGenerationTypes")]
    public string generationType;

    private List<string> GetGenerationTypes()
    {
        return new List<string>
        {
            "Spacebar press",
            "Small delay",
            "Instant",
            "Test",
        };
    }


    [Header("Room Lists")]
    public List<RectInt> rooms = new List<RectInt>();
    public List<RectInt> completedRooms = new List<RectInt>();
    public List<RectInt> doors = new List<RectInt>();

    private RectInt room;
    private Coroutine splitDelayCoroutine;
    private Coroutine splitInstantCoroutine;
    private Tilemap tilemap;
    private WaitForSeconds oneSecondPause;
    private WaitForSeconds halfASecondPause;
    private bool roomGenerationComplete = false;


    void Start()
    {
        oneSecondPause = new WaitForSeconds(1);
        halfASecondPause = new WaitForSeconds(0.5f);

        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        CreateMainRoom();

        if (generationType == "Small delay")
        {
            StartCoroutine(SplitRoomsWithDelay());
        }
        else if (generationType == "Instant")
        {
            StartCoroutine(SplitRoomsInstantly());
        }
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

        foreach (var d in doors)
        {
            AlgorithmsUtils.DebugRectInt(d, Color.blue);
        }

        if (Input.GetKeyDown(KeyCode.Space) && generationType == "Spacebar press")
        {
            SplitRooms();

            if (rooms.Count == 0 && !roomGenerationComplete)
            {
                roomGenerationComplete = true;
                SetListToDefault();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(FindConnectedRooms(rooms[0]));
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
        else if (rooms[0].width <= rooms[0].height)
        {
            // Split the room horizontally if it is taller than it is wide
            SplitHorizontally(rooms[0]);
        }
    }

    public void SplitHorizontally(RectInt room)
    {
        int splitPoint;
        splitPoint = room.y + room.height / 2 + Random.Range(-randomSplitOffset, randomSplitOffset);

        RectInt topRoom = new RectInt(room.x, splitPoint, room.width, room.height - (splitPoint - room.y));
        RectInt bottomRoom = new RectInt(room.x, room.y, room.width, splitPoint - room.y + 1);
        rooms.RemoveAt(0);
        CheckRoomSize(topRoom);
        CheckRoomSize(bottomRoom);
    }

    public void SplitVertically(RectInt room)
    {
        int splitPoint;
        splitPoint = room.x + room.width / 2 + Random.Range(-randomSplitOffset, randomSplitOffset);

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

    public void SetListToDefault()
    {
        rooms.AddRange(completedRooms);
        completedRooms.Clear();
    }


    private IEnumerator FindConnectedRooms(RectInt startRoom)
    {
        Queue<RectInt> toProcess = new Queue<RectInt>();
        HashSet<RectInt> visited = new HashSet<RectInt>();

        toProcess.Enqueue(startRoom);
        visited.Add(startRoom);
        completedRooms.Add(startRoom);

        while (toProcess.Count > 0)
        {
            RectInt current = toProcess.Dequeue();
            int attemptsLeft = 10;

            while (attemptsLeft > 0)
            {
                bool foundConnection = false;

                foreach (var other in rooms)
                {
                    if (visited.Contains(other)) continue;

                    if (AlgorithmsUtils.Intersects(current, other) == true)
                    {
                        completedRooms.Add(other);
                        visited.Add(other);
                        toProcess.Enqueue(other);
                        foundConnection = true;
                        // Create a door between the two rooms
                        CreateDoorBetweenRooms(current, other);
                        yield return new WaitForSeconds(2); // Pause to visualize the door creation
                        break; // restart attempts
                    }
                }

                if (foundConnection)
                {
                    attemptsLeft = 10;
                }
                else
                {
                    attemptsLeft--;
                }
            }
        }

        yield break;
    }

    public void CreateDoorBetweenRooms(RectInt roomA, RectInt roomB)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        // Find the center of each room
        Vector3Int centerA = new Vector3Int(roomA.x + roomA.width / 2, roomA.y + roomA.height / 2, 0);
        Vector3Int centerB = new Vector3Int(roomB.x + roomB.width / 2, roomB.y + roomB.height / 2, 0);

        Debug.Log($"Creating centers {centerA} and {centerB}");
        // Create a door at the center point between the two rooms
        Vector3Int doorPosition = Vector3Int.RoundToInt((centerA + centerB) / 2);
        tilemap.SetTile(doorPosition, tile);
        doors.Add(new RectInt(doorPosition.x, doorPosition.y, 1, 1));
        Debug.Log($"Door created between {roomA} and {roomB} at position {doorPosition}");
    }

    private void GenerationTypeChanged()
    {
        if (generationType == "Small delay")
        {
            // Checks if Coroutine is null
            splitDelayCoroutine ??= StartCoroutine(SplitRoomsWithDelay());
        }
        else if (generationType == "Instant")
        {
            splitInstantCoroutine ??= StartCoroutine(SplitRoomsInstantly());
        }
        else
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator SplitRoomsWithDelay()
    {
        while (rooms.Count > 0)
        {
            SplitRooms();
            yield return new WaitForSeconds(0.2f);
        }
        if (!roomGenerationComplete)
        {
            roomGenerationComplete = true;
            SetListToDefault();
        }
    }

    private IEnumerator SplitRoomsInstantly()
    {
        while (rooms.Count > 0)
        {
            SplitRooms();
            yield return null;
        }
        if (!roomGenerationComplete)
        {
            roomGenerationComplete = true;
            SetListToDefault();
        }
    }
}
