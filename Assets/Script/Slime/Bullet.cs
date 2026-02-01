using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 dir;
    private float speed;

    [Header("Damage")]
    public int damage = 5; // SlimeF_B는 5 데미지

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Bullet은 Trigger 판정용
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    // Shooter가 총알 생성 직후 방향/속도를 넣어주는 함수
    public void SetUp(Vector2 direction, float moveSpeed)
    {
        dir = direction.normalized;
        speed = moveSpeed;

        // ✅ 5초 자동 삭제 제거
        // Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ 1) 아랫벽(Goal)에 닿으면 "그냥 삭제" (체력 깎기 X)
        if (other.CompareTag("Goal"))
        {
            Destroy(gameObject);
            return;
        }

        // ✅ 2) 플레이어에 닿으면 데미지 주고 삭제
        if (other.CompareTag("Player"))
        {
            // PlayerHP가 Player 오브젝트/자식 어디에 있어도 찾기
            PlayerHP hp = other.GetComponentInChildren<PlayerHP>();
            if (hp == null)
                hp = other.GetComponentInParent<PlayerHP>();

            if (hp != null)
            {
                hp.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // (선택) 벽/장애물에 닿아도 삭제하고 싶으면 여기에 추가하면 됨
        // if (other.CompareTag("Wall")) Destroy(gameObject);
    }
}
