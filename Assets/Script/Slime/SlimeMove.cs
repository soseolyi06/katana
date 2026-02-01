using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SlimeStatManager))] // ✅ 스탯 스크립트가 반드시 있어야 함
public class SlimeMove : MonoBehaviour
{
    // =========================================================
    // ✅ SlimeMove의 역할
    // - 항상 아래로 이동한다
    // - Goal에 닿으면 플레이어 HP를 깎고 자신은 사라진다
    // - "이동속도/공격력" 같은 값은 SlimeStatManager에서 가져온다
    // =========================================================

    private Rigidbody2D rb;           // 이동에 사용할 Rigidbody
    private SlimeStatManager stat;    // ✅ 슬라임의 스탯 데이터(HP/공격/이속 등)

    void Awake()
    {
        // 1) 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        stat = GetComponent<SlimeStatManager>();

        // 2) 기본 세팅 (탑다운)
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 3) 안전장치: 혹시 스탯이 없으면(세팅 실수) 바로 알림
        if (stat == null)
        {
            Debug.LogError("[SlimeMove] SlimeStatManager가 없습니다! 슬라임 오브젝트에 SlimeStatManager를 붙이세요.");
        }
    }

        void Start()
    {
        // hasWeapon이 true일 때만 weaponShooter를 켠다
        if (stat.hasWeapon && stat.weaponShooter != null)
            stat.weaponShooter.enabled = true;

        // hasWeapon이 false면 꺼둔다
        if (!stat.hasWeapon && stat.weaponShooter != null)
            stat.weaponShooter.enabled = false;
    }

    void FixedUpdate()
    {
        // stat이 없으면 움직일 수 없음(NullReference 방지)
        if (stat == null) return;

        // ✅ 항상 아래로 이동
        // - 이동속도는 SlimeStatManager의 moveSpeed를 사용
        rb.linearVelocity = Vector2.down * stat.moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Goal 태그에 닿았을 때만 처리
        if (!other.CompareTag("Goal"))
            return;

        // 1) Player 태그 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // 2) PlayerHP 가져오기
            // - PlayerHP가 자식 오브젝트에 있어도 찾을 수 있게 InChildren 사용
            PlayerHP playerHp = playerObj.GetComponentInChildren<PlayerHP>();

            if (playerHp != null && stat != null)
            {
                // 3) ✅ 슬라임 공격력만큼 플레이어 체력을 깎는다
                // - 공격력은 SlimeStatManager의 attack을 사용
                playerHp.TakeDamage(stat.attack);
            }
        }

        // 4) Goal에 도달했으니 슬라임 제거
        Destroy(gameObject);
    }
}
