using System.Collections;
using System.Collections.Generic; // List 사용
using UnityEngine;
using System; // 직렬화 (Serializable) 사용
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count (int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public int columns = 8;
    public int rows = 8;
    
    public Count wallCount = new Count (5, 9);
    public Count foodCount = new Count (1, 5);
    
    public GameObject exit;
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    private Transform boardHolder;
    private List <Vector3> gridPositions = new List<Vector3>();

    void InitialiseList()
    {
        gridPositions.Clear();

        for (int x = 1; x < columns - 1; x++) // column(열)을 먼저 만든다. (1,1) 시작해서 (6,1)
        {
            for (int y = 1; y < rows - 1; y++) // 안에서 row(행)을 만든다. (1,1)(1,2)(1,3)(1,4)(1,5)(1,6)
            {
                gridPositions.Add(new Vector3(x,y,0f));
            }
        }
    }

    void BoradSetup () // 배경인 바닥 타일들 + 바깥의 벽 타일을 배치한다
    {
        boardHolder = new GameObject ("Board").transform;

        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range (0, floorTiles.Length)];
                
                if (x == -1 || x == columns || y == -1 || y == rows)
                    toInstantiate = outerWallTiles[Random.Range (0, outerWallTiles.Length)];

                GameObject instance = Instantiate(toInstantiate, new Vector3(x,y,0f), Quaternion.identity) as GameObject;

                instance.transform.SetParent(boardHolder);
            }
        }
    }
    
    Vector3 RandomPosition() // 리스트에서 랜덤 위치를 뽑는다
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 RandomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return RandomPosition;
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) // 선택한 장소에 실제로 타일을 소환하는 함수
    {
        int objectCount = Random.Range (minimum, maximum + 1); // 주어진 오브젝트를 얼마나 스폰할지 조정

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
            Instantiate (tileChoice, randomPosition, Quaternion.identity);
        }
    }
    
    public void SetupScene (int level)
    {
        BoradSetup();
        InitialiseList();
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        
        int enemyCount = (int)Mathf.Log(level, 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        Instantiate(exit, new Vector3(columns - 1, rows -1, 0f), Quaternion.identity);
    }
}
