using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private int food;

    private Vector2 touchOrigin = -Vector2.one; // -Vector2.one 는 스크린 밖의 위치를 의미. 이것은 터치 입력이 있는지 체크하기 위한 초기 상태임
        
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints; // 플레이어는 해당 레벨 동안 음식 점수를 관리할 수 있다
        foodText.text = "Food: " + food;
        base.Start();
    }

    private void OnDisable() // 게임오브젝트가 비활성화 되는 순간 호출
    {
        GameManager.instance.playerFoodPoints = food; // 레벨이 바뀔 때 게임매니저에 food 값을 다시 저장할 수 있다.
    }
    
    void Update()
    {
        if (!GameManager.instance.playersTurn) return; // 플레이어 턴이 아니라면 return 하여 이하 코드들이 실행되지 않음            
        
        int horizontal = 0;
        int vertical = 0;
        
    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        horizontal = (int) Input.GetAxisRaw("Horizontal");
        vertical = (int) Input.GetAxisRaw("Vertical");

        if (horizontal != 0) // 수평으로 움직이는지 체크해서 그렇다면 vertical 을 0 으로 한다. (대각선 이동을 막기 위해)
            vertical = 0;                   

    #else
        
        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1;
            }
        }
        
    #endif
        
        if (horizontal != 0 || vertical != 0) // 플레이어가 움직이려 한다는 뜻
            AttemptMove<Wall> (horizontal, vertical);
    }

    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        food--; // 움직일 때 마다 음식 점수를 1씩 잃는다 (which is one of the core mechanics of the game)
        foodText.text = "Food: " + food;

        base.AttemptMove <T> (xDir, yDir);

        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit)) //Move 함수가 true 라는 것은 플레이어가 움직일 수 있다는 뜻
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke ("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }
    
    protected override void OnCantMove <T> (T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("playerChop");
    }

    private void Restart()
    {
        // Application.LoadLevel(Application.loadedLevel);
        SceneManager.LoadScene(0);
    }

    public void LoseFood (int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }            
    }
}
