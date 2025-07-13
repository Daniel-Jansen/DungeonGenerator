using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodfillSpawner : MonoBehaviour
{
    private GameObject floorPrefab;
    private Transform floorParent;

    private void Start()
    {
        floorPrefab = Resources.Load<GameObject>("Prefabs/Dungeon/Floor");
    }

    public IEnumerator FloodFillRoom(RectInt roomRect)
    {
        int[,] tileMap = GetComponent<TileMapGenerator>().GetTileMap();

        int rows = tileMap.GetLength(0);
        int cols = tileMap.GetLength(1);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        floorParent = new GameObject("RoomFloors").transform;
        floorParent.SetParent(this.transform);

        // Start at room center
        Vector2Int center = new Vector2Int(
            roomRect.x + roomRect.width / 2,
            roomRect.y + roomRect.height / 2
        );

        queue.Enqueue(center);
        visited.Add(center);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Place floor tile
            Vector3 worldPos = new Vector3(current.y + 0.5f, 0, current.x + 0.5f);
            GameObject floor = Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
            floor.name = $"Floor_{current.x}_{current.y}";

            yield return null;

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // Check bounds
                if (neighbor.x >= 0 && neighbor.x < cols && neighbor.y >= 0 && neighbor.y < rows)
                {
                    // Check if neighbor is a floor tile (value 0) and not visited
                    if (tileMap[neighbor.x, neighbor.y] == 0 &&
                        !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        yield break;
    }
}
