using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f; // 레벨이 시작되기 전에 대기시간
    public float turnDelay = .1f; // 턴 사이에 게임이 얼마 동안 대기하는가    
    public static GameManager instance = null;
    public BoardManager boardScript;
    
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    private Text levelText;
    private GameObject levelImage;
    
    private int level = 1;    
    
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup; // 게임 보드를 만드는 중인지 체크 (플레이어 움직임을 막기 위해)
        
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    private void OnLevelWasLoaded (int index) // 유니티 API : 씬이 로드 될때마다 호출
    // private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        level++;
        InitGame();
    }

    // void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    // {
    //     level++;
    //     InitGame();
    // }

    // void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnLevelFinishedLoading;
    // }

    // void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    // }

    void InitGame()
    {
        doingSetup = true;
        
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved.";
        levelImage.SetActive(true);
        enabled = false;
    }
    
    void Update()
    {
        if (playersTurn || enemiesMoving || doingSetup) // playersTurn 혹은 적이 이동중이라면, 아래 코드를 실행하지 않기 위해 리턴함
            return;

        StartCoroutine (MoveEnemies()); // 위 조건이 모두 거짓이면 코루틴 실행
    }

    public void AddEnemyToList(Enemy script) // 적들이 자신을 게임매니저에 등록하도록 해서 게임매니저가 적들이 움직이도록 명령할 수 있게 함
    {
        enemies.Add (script);
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        
        if (enemies.Count == 0) // 또한 적이 아무도 없다면, 즉 첫 레벨
        {
            yield return new WaitForSeconds(turnDelay); // 대기하는 적이 없지만 일단 플레이어가 기다리게 한다
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy(); // 적들이 움직이도록 명령
            yield return new WaitForSeconds(enemies[i].moveTime); // 다음 적을 호출하기 전에 moveTime 만큼 대기
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}