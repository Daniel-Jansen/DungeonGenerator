using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt room;
    public bool splitHorizontally;


    private WaitForSeconds oneSecondPause;
	private WaitForSeconds halfASecondPause;


    void Start()
    {
        oneSecondPause = new WaitForSeconds(1);
		halfASecondPause = new WaitForSeconds(0.5f);

        StartCoroutine(GenerateDungeon());
    }

    void Update()
    {
        AlgorithmsUtils.DebugRectInt(room, Color.red);
    }

    IEnumerator GenerateDungeon()
    {
        CreateRoom();
        yield return halfASecondPause;
        yield return SplitRooms();
    }

    void CreateRoom()
    {
        room = new RectInt(0, 0, 100, 50);
    }

    IEnumerator SplitRooms()
    {
        
        yield return null;
    }
}
