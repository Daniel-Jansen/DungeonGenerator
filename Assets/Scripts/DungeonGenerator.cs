using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class DungeonGenerator : MonoBehaviour
{
    private TileMapGenerator tileMapGenerator;
    private MarchingSquaresSpawner marchingSquaresSpawner;
    private FloodfillSpawner floodfillSpawner;

    [SerializeField]
    private NavMeshSurface navMeshSurface;

    [Header("Dungeon Settings")]
    public RectInt startingRoomSize = new RectInt(0, 0, 100, 100);
    public RectInt minRoomSize = new RectInt(0, 0, 10, 10);
    public int randomSplitOffset = 3;

    [Header("Dungeon Generation")]
    [OnValueChanged("GenerationTypeChanged")]
    [Dropdown("GetGenerationTypes")]
    public string generationType;

    private List<string> GetGenerationTypes()
    {
        return new List<string>
        {
            // "Spacebar press",
            // "Small delay",
            "Instant",
        };
    }

    [Header("Lists")]
    public static List<RectInt> rooms = new List<RectInt>();
    public List<RectInt> completedRooms = new List<RectInt>();
    public static List<RectInt> doors = new List<RectInt>();

    private Graph<RectInt> graph = new Graph<RectInt>();

    private HashSet<RectInt> visited = new HashSet<RectInt>();

    private RectInt room;
    private Coroutine splitDelayCoroutine;
    private Coroutine splitInstantCoroutine;
    private Tilemap tilemap;
    private bool roomGenerationComplete = false;
    private bool doorGenerationComplete = false;
    public bool sufficient = false;

    void Start()
    {
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        tileMapGenerator = GetComponent<TileMapGenerator>();
        marchingSquaresSpawner = GetComponent<MarchingSquaresSpawner>();
        floodfillSpawner = GetComponent<FloodfillSpawner>();

        CreateMainRoom();

        // if (generationType == "Small delay")
        // {
        //     StartCoroutine(SplitRoomsWithDelay());
        // }
        if (generationType == "Instant")
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

        foreach (var v in visited)
        {
            AlgorithmsUtils.DebugRectInt(v, Color.yellow);
        }

        foreach (var n in graph.adjacencyList.Keys)
        {
            Vector3 center = new Vector3(n.x + n.width / 2f, 0, n.y + n.height / 2f);
            DebugExtension.DebugWireSphere(center, Color.cyan, 0.5f);
            // Draw edges
            foreach (var neighbor in graph.GetNeighbors(n))
            {
                Vector3 neighborCenter = new Vector3(neighbor.x + neighbor.width / 2f, 0, neighbor.y + neighbor.height / 2f);
                Debug.DrawLine(center, neighborCenter, Color.magenta);
            }
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
            marchingSquaresSpawner.GenerateWalls();
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

        if (rooms.Count == 0 && !roomGenerationComplete)
        {
            roomGenerationComplete = true;
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

    private IEnumerator FindConnectedRooms()
    {
        Queue<RectInt> toProcess = new Queue<RectInt>();

        // Enqueue all rooms to process
        foreach (var room in rooms) 
        {
            toProcess.Enqueue(room);
        }

        while (toProcess.Count > 0)
        {
            RectInt current = toProcess.Dequeue();
            int attemptsLeft = 8;

            completedRooms.Add(current);
            rooms.Remove(current);

            visited.Clear();
            visited.Add(current);

            while (attemptsLeft > 0)
            {
                bool foundConnection = false;

                foreach (var other in rooms)
                {
                    if (visited.Contains(other)) continue;

                    if (AlgorithmsUtils.Intersects(current, other) == true)
                    {
                        visited.Add(other);
                        foundConnection = true;
                        // Create a door between the two rooms
                        CreateDoorBetweenRooms(current, other);
                        // yield return null;
                        break; // restart attempts
                    }
                }

                if (foundConnection)
                {
                    attemptsLeft = 8;
                }
                else
                {
                    attemptsLeft--;
                }
            }
        }

        if (!doorGenerationComplete)
        {
            doorGenerationComplete = true;
            visited.Clear();
            SetListToDefault();
        }
        yield break;
    }

    public void CreateDoorBetweenRooms(RectInt roomA, RectInt roomB)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        // Get the intersection area between the two rooms
        RectInt intersection = AlgorithmsUtils.Intersect(roomA, roomB);

        if (intersection.width <= 2 && intersection.height <= 2)
        {
            return; // If the intersection is too small, do not create a door
        }

        // Calculate the center of the intersection to use as the door position
        Vector3Int doorPosition = new Vector3Int(
            intersection.x + intersection.width / 2,
            intersection.y + intersection.height / 2,
            0
        );

        // Randomize the door location within the intersection area
        if (intersection.width > 2)
        {
            doorPosition.x = Random.Range(intersection.x + 1, intersection.x + intersection.width - 1);
        }
        if (intersection.height > 2)
        {
            doorPosition.y = Random.Range(intersection.y + 1, intersection.y + intersection.height - 1);
        }

        tilemap.SetTile(doorPosition, tile);
        doors.Add(new RectInt(doorPosition.x, doorPosition.y, 1, 1));
    }

    private IEnumerator GenerateGraph()
    {
        yield return GenerateNodes();

        foreach (var room in rooms)
        {
            yield return FindDoorsInRoom(room);
        }

        StartCoroutine(BFSSearch());
    }

    private IEnumerator GenerateNodes()
    {
        foreach (var room in rooms)
        {
            graph.AddNode(room);
            // yield return null;
        }

        foreach (var door in doors)
        {
            graph.AddNode(door);
            // yield return null;
        }

        yield return null;
    }

    private IEnumerator FindDoorsInRoom(RectInt room)
    {
        foreach (var door in doors)
        {
            if (AlgorithmsUtils.Intersects(room, door))
            {
                graph.AddEdge(room, door);
            }
        }

        yield return null;
    }

    private IEnumerator BFSSearch()
    {
        visited.Clear();

        yield return StartCoroutine(graph.BFS(rooms[0], visited));
    }

    public RectInt GetDungeonBounds()
    {
        return startingRoomSize;
    }
    
    public List<RectInt> GetRooms()
    {
        return rooms;
    }

    public List<RectInt> GetDoors()
    {
        return doors;
    }

    public IEnumerator FloodFillRoom(RectInt roomRect)
    {
        yield return StartCoroutine(floodfillSpawner.FloodFillRoom(roomRect));
    }

    public void SpawnSimpleAssets()
    {
        GameObject wallPrefab = Resources.Load<GameObject>("Prefabs/Wall");
        GameObject wallParent = new GameObject("RoomWalls");
        wallParent.transform.SetParent(this.transform);
        NavMeshModifier wallNavMeshModifier = wallParent.AddComponent<NavMeshModifier>();
        wallNavMeshModifier.overrideArea = true;
        wallNavMeshModifier.area = 1; // Not walkable area

        GameObject floorPrefab = Resources.Load<GameObject>("Prefabs/Floor");
        GameObject floorParent = new GameObject("RoomFloors");
        floorParent.transform.SetParent(this.transform);

        HashSet<Vector2Int> visitedWallPositions = new HashSet<Vector2Int>();
        visitedWallPositions.Clear();

        // Register door positions first
        foreach (var door in doors)
        {
            for (int x = door.xMin; x < door.xMax; x++)
            {
                for (int y = door.yMin; y < door.yMax; y++)
                {
                    // No wall at the door position
                    visitedWallPositions.Add(new Vector2Int(x, y));

                    // Instantiates a floor prefab at the door position
                    Vector3 worldPos = new Vector3(x + 0.5f, 0, y + 0.5f);
                    GameObject floor = Instantiate(floorPrefab, worldPos, floorPrefab.transform.rotation, floorParent.transform);
                    floor.name = $"Floor_Door_{x}_{y}";
                }
            }
        }

        // Place walls & floors for each room
        foreach (var room in rooms)
        {
            // Place walls around the room
            for (int x = room.x; x < room.x + room.width; x++)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    if (x == room.x || x == room.x + room.width - 1 ||
                        y == room.y || y == room.y + room.height - 1)
                    {
                        Vector2Int cellPos = new Vector2Int(x, y);

                        if (!visitedWallPositions.Contains(cellPos))
                        {
                            visitedWallPositions.Add(cellPos);

                            Vector3 worldPos = new Vector3(x + 0.5f, 0.5f, y + 0.5f);
                            GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity, wallParent.transform);
                            wall.name = $"Wall_{x}_{y}";
                        }
                    }
                }
            }

            // Place floors inside the room
            for (int x = room.x + 1; x < room.x + room.width - 1; x++)
            {
                for (int y = room.y + 1; y < room.y + room.height - 1; y++)
                {
                    Vector3 worldPos = new Vector3(x + 0.5f, 0, y + 0.5f);
                    GameObject floor = Instantiate(floorPrefab, worldPos, floorPrefab.transform.rotation, floorParent.transform);
                    floor.name = $"Floor_{x}_{y}";
                }
            }
        }

        BakeNavMesh();
    }

    public void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("NavMeshSurface is not assigned.");
        }
    }

    // private void GenerationTypeChanged()
    // {
    //     // if (generationType == "Small delay")
    //     // {
    //     //     // Checks if Coroutine is null
    //     //     splitDelayCoroutine ??= StartCoroutine(SplitRoomsWithDelay());
    //     // }
    //     if (generationType == "Instant")
    //     {
    //         splitInstantCoroutine ??= StartCoroutine(SplitRoomsInstantly());
    //     }
    //     else
    //     {
    //         StopAllCoroutines();
    //     }
    // }

    // private IEnumerator SplitRoomsWithDelay()
    // {
    //     while (rooms.Count > 0)
    //     {
    //         SplitRooms();
    //         yield return new WaitForSeconds(0.2f);
    //     }

    //     SetListToDefault();
    //     yield return StartCoroutine(FindConnectedRooms());

    //     yield return StartCoroutine(GenerateGraph());

    //     tileMapGenerator.GenerateTileMap();
    //     // yield return StartCoroutine(marchingSquaresSpawner.GenerateWalls());
    // }

    private IEnumerator SplitRoomsInstantly()
    {
        while (rooms.Count > 0)
        {
            SplitRooms();
            yield return null;
        }

        SetListToDefault();
        yield return StartCoroutine(FindConnectedRooms());

        yield return StartCoroutine(GenerateGraph());

        tileMapGenerator.GenerateTileMap();

        if (!sufficient)
        {
            marchingSquaresSpawner.GenerateWalls();
            yield return StartCoroutine(FloodFillRoom(rooms[0]));
            BakeNavMesh();
            Debug.Log("Dungeon generation complete.");
        }

        if (sufficient)
        {
            SpawnSimpleAssets();
        }
    }
}
