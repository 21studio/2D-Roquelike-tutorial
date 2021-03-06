﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public int playerDamage; // 적이 플레이어 공격할 때 뺄셈할 음식 포인트

    private Animator animator;
    private Transform target; // 플레이어 위치를 저장하고, 적이 어디로 향할지 알려줌
    private bool skipMove; // 적이 턴마다 움직이게 하는데 사용

    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;
        
    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this); // 이렇게함으로서 이제 Enemy에서 public으로 선언한 MoveEnemy 함수를 호출할 수 있다
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        if (skipMove) // skipMove 참이면 턴을 스킵. 즉 매번 턴이 돌아올 때만 움직이게 된다
        {
            skipMove = false;
            return;
        }

        base.AttemptMove <T> (xDir, yDir);

        skipMove = true;
    }

    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;

        if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon) // x좌표가 대충 같은지 체크하는 것이고, 이는 적과 플레이어가 같은 열에 속한다는 의미다
            yDir = target.position.y > transform.position.y ? 1 : -1; // target(player)의 y좌표가 transform(enemy)보다 크다면 up(1), 아니라면 down(-1)
        else
            xDir = target.position.x > transform.position.x ? 1 : -1;

        AttemptMove <Player> (xDir, yDir);
    }

    protected override void OnCantMove <T> (T component)
    {
        Player hitPlayer = component as Player;

        animator.SetTrigger ("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);

        hitPlayer.LoseFood(playerDamage);
    }
}
