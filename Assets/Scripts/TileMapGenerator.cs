using System;
using System.Text;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(3)]
public class TileMapGenerator : MonoBehaviour
{
    
    [SerializeField]
    private UnityEvent onGenerateTileMap;
    
    [SerializeField]
    DungeonGenerator dungeonGenerator;
    
    private int [,] _tileMap;
    
    private void Start()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }
    
    [Button]
    public void GenerateTileMap()
    {
        int [,] tileMap = new int[dungeonGenerator.GetDungeonBounds().height, dungeonGenerator.GetDungeonBounds().width];
        int rows = tileMap.GetLength(0);
        int cols = tileMap.GetLength(1);

        //Fill the map with empty spaces
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                tileMap[i, j] = 0; //0 represents empty space
            }
        }

        //Draw the edges of the rooms
        foreach (var room in dungeonGenerator.GetRooms())
        {
            for (int i = room.y; i < room.y + room.height; i++)
            {
                tileMap[i, room.x] = 1; //Left
                tileMap[i, room.x + room.width - 1] = 1; //Right
            }
            for (int j = room.x; j < room.x + room.width; j++)
            {
                tileMap[room.y, j] = 1; //Top
                tileMap[room.y + room.height - 1, j] = 1; //Bottom
            }
        }

        //Draw the doors
        List<RectInt> doors = dungeonGenerator.GetDoors();
        foreach (var door in doors)
        {
            for (int i = door.y; i < door.y + door.height; i++)
            {
                for (int j = door.x; j < door.x + door.width; j++)
                {
                    tileMap[i, j] = 0;
                }
            }
        }

        _tileMap = tileMap;
        
        onGenerateTileMap.Invoke();
    }

    public string ToString(bool flip)
    {
        if (_tileMap == null) return "Tile map not generated yet.";
        
        int rows = _tileMap.GetLength(0);
        int cols = _tileMap.GetLength(1);
        
        var sb = new StringBuilder();
    
        int start = flip ? rows - 1 : 0;
        int end = flip ? -1 : rows;
        int step = flip ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append((_tileMap[i, j]==0?'0':'#')); //Replaces 1 with '#' making it easier to visualize
            }
            sb.AppendLine();
        }
    
        return sb.ToString();
    }
    
    public int[,] GetTileMap()
    {
        return _tileMap.Clone() as int[,];
    }
    
    [Button]
    public void PrintTileMap()
    {
        Debug.Log(ToString(true));
    }
}
