using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// abstract 키워드 : 클래스들과 클래스 멤버들을 만들 때 기능을 완성하지 않아도 되게 하고, 해당 클래스는 반드시 파생클래스로 삽입
public abstract class MovingObject : MonoBehaviour 
{
    public float moveTime = .1f; // 수 초 동안 오브젝트를 움직이게 할 시간 단위
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;
        
    protected virtual void Start() // protected virtual 함수는 자식 클래스가 덮어써서 재정의 할 수 있다 (오버라이드)
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime; // moveTime의 역수를 저장함으로서, 나누기 대신에 효율적인 곱하기를 쓴다
    }

    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position; // 현재 오브젝트의 위치를 이차원 벡터로 형변환 할 수 있다. (z축 날림)
        Vector2 end = start + new Vector2 (xDir, yDir);

        boxCollider.enabled = false; // Ray 를 사용 할 때 자기 자신의 충돌체에 부딪치지 않게 하기 위함
        hit = Physics2D.Linecast (start, end, blockingLayer);
        boxCollider.enabled = true;

        if (hit.transform == null) // 라인으로 검사한 공간이 열려 있고 그곳으로 이동 할 수 있다는 뜻
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    protected IEnumerator SmoothMovement (Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude; // sqrMagnitude(벡터 길이 제곱)이 magnitude(벡터 길이) 보다 계산이 빠르다

        while (sqrRemainingDistance > float.Epsilon) // Epsilon 은 0에 가까운 엄청 작은 수 (극한)
        {
            Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, inverseMoveTime * Time.deltaTime); // Vector3.MoveTowards 는 현재 포인트를 직선상에 목표 포인트로 이동
            rb2D.MovePosition(newPosition); // MovePosition 을 사용, 위에서 찾은 새 지점으로 이동시킨다
            sqrRemainingDistance = (transform.position - end).sqrMagnitude; // 그 다음 이동 이후에 남은 거리를 다시 계산
            yield return null; // 루프를 갱신하기 전에 다음 프레임을 기다린다
        }
    }

    protected virtual void AttemptMove <T> (int xDir, int yDir) // 일반형 입력 T 는 막혔을 때, 유닛이 반응할 컴포넌트 타입을 가리키기 위해 사용(적의 상대는 플레이어, 플레이어 상대는 벽)
        where T : Component // where 키워드로 T 가 컴포넌트 종류를 가리키게 한다
    {
        RaycastHit2D hit;
        bool canMove = Move (xDir, yDir, out hit); // 이동에 성공하면 true, 실패하면 false

        if (hit.transform == null) // Move 에서 라인 캐스트가 다른 것과 부딪히지 않았다면 리턴 (이후 코드 실행 안함)
            return;

        T hitComponent = hit.transform.GetComponent<T>(); // 만약 무언가와 부딪혔다면, 충돌한 오브젝트의 컴포넌트의 레퍼런스를 T 타입의 컴포넌트에 할당

        if (!canMove && hitComponent != null) // 움직이던 오브젝트가 막혔고, 상호작용할 수 있는 오브젝트와 충돌했음을 뜻함
            OnCantMove(hitComponent);
    }
    
    // 여기 추상문은 사용할 것들이 현재 존재하지 않거나, 불완전하게 만들어졌음을 의미한다 (상속한 자식 클래스에서 완성하면 됨)
    // 또한, 추상화 함수이기 때문에 괄호를 사용하지 않는다
    protected abstract void OnCantMove <T> (T component) // 일반형(Generic) 입력 T를 T형의 component 라는 변수로서 받아온다
        where T : Component;
}
