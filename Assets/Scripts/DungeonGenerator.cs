using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public Vector2Int startingRoomSize = new Vector2Int(100, 100);
    public RectInt room;
    public bool willSplitHorizontally;
    // public float delay = 0.5f;

    public List<RectInt> rooms = new List<RectInt>();

    private WaitForSeconds oneSecondPause;
    private WaitForSeconds halfASecondPause;


    void Start()
    {
        oneSecondPause = new WaitForSeconds(1);
        halfASecondPause = new WaitForSeconds(0.5f);

        CreateRoom();
        // StartCoroutine(GenerateDungeon());
    }

    void Update()
    {
        foreach (var r in rooms)
        {
            AlgorithmsUtils.DebugRectInt(r, Color.red);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            splitHorizontally();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            splitVertically();
        }
    }

    void CreateRoom()
    {
        room = new RectInt(0, 0, startingRoomSize.x, startingRoomSize.y);
        rooms.Add(room);
    }

    void splitHorizontally()
    {
        RectInt roomToSplit = rooms[0];
        int splitPoint = roomToSplit.y + roomToSplit.height / 2;
        RectInt topRoom = new RectInt(roomToSplit.x, splitPoint, roomToSplit.width, roomToSplit.height - (splitPoint - roomToSplit.y));
        RectInt bottomRoom = new RectInt(roomToSplit.x, roomToSplit.y, roomToSplit.width, splitPoint - roomToSplit.y + 1);
        rooms.RemoveAt(0);
        rooms.Add(topRoom);
        rooms.Add(bottomRoom);
    }

    void splitVertically()
    {
        RectInt roomToSplit = rooms[0];
        int splitPoint = roomToSplit.x + roomToSplit.width / 2;
        RectInt leftRoom = new RectInt(roomToSplit.x, roomToSplit.y, splitPoint - roomToSplit.x + 1, roomToSplit.height);
        RectInt rightRoom = new RectInt(splitPoint, roomToSplit.y, roomToSplit.width - (splitPoint - roomToSplit.x), roomToSplit.height);
        rooms.RemoveAt(0);
        rooms.Add(leftRoom);
        rooms.Add(rightRoom);
    }

    // IEnumerator GenerateDungeon()
    // {
    //     CreateRoom();
    //     yield return halfASecondPause;
    //     yield return SplitRooms();
    // }

    // IEnumerator SplitRooms()
    // {


    //     yield return null;
    // }
}
