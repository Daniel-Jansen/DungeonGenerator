using System.Collections;
using UnityEngine;

public class MarchingSquaresSpawner : MonoBehaviour
{
    [SerializeField] private TileMapGenerator tileMapGenerator;
    [SerializeField] private MarchingSquareConfig[] lookupTable = new MarchingSquareConfig[16];

    private Transform wallParent;

    private void Start()
    {
        tileMapGenerator = GetComponent<TileMapGenerator>();
    }

    public void GenerateWalls()
    {
        int[,] map = tileMapGenerator.GetTileMap();
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        wallParent = new GameObject("RoomWalls").transform;
        wallParent.SetParent(this.transform);

        for (int y = 0; y < rows - 1; y++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                int a = map[y, x];
                int b = map[y, x + 1];
                int c = map[y + 1, x];
                int d = map[y + 1, x + 1];

                int squareIndex = (a << 3) | (b << 2) | (c << 1) | d;

                StartCoroutine(SpawnGeometry(squareIndex, x, y));
                // yield return null;
            }
        }
    }

    IEnumerator SpawnGeometry(int squareIndex, int tileX, int tileY)
    {
        if (squareIndex == 0 || squareIndex == 15)
            yield return null;

        MarchingSquareConfig config = lookupTable[squareIndex];
        if (config != null && config.prefab != null)
        {
            Vector3 position = new Vector3(tileX + 1f, 0, tileY + 1f);
            Quaternion rotation = Quaternion.Euler(0, config.rotationDegrees, 0);

            Instantiate(config.prefab, position, rotation, wallParent);
        }
    }
}
