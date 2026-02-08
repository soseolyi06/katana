using UnityEngine;

public class SlimeQ_Bolls : MonoBehaviour, IBossSpawnedSkill
{
    [Header("Move")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float arriveDistance = 0.1f; // 목표 도착 판정
    private Vector2 targetPos;
    private bool initialized;

    [Header("Damage")]
    private int damage;

    [Header("On Arrive")]
    [SerializeField] private GameObject groundPrefab; // 장판 프리팹
    [SerializeField] private float groundLifetime = 3.0f;

    [Header("Life")]
    [SerializeField] private float lifeTimeFailSafe = 6f;

    public void Init(Vector2 fixedTargetPos, int dmg, float spd, GameObject ground, float groundLife)
    {
        targetPos = fixedTargetPos;
        damage = dmg;
        speed = spd;

        groundPrefab = ground;
        groundLifetime = groundLife;

        initialized = true;

        // 이동 방향에 맞게 회전(스프라이트가 오른쪽을 향한다고 가정)
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifeTimeFailSafe);
    }

    private void Update()
    {
        if (!initialized) return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, speed * Time.deltaTime);
        transform.position = next;

        if (Vector2.Distance(next, targetPos) <= arriveDistance)
        {
            ExplodeAndSpawnGround();
        }
    }

    // “도착 폭발” 처리
    private void ExplodeAndSpawnGround()
    {
        // 폭발 VFX/사운드 넣고 싶으면 여기
        // Instantiate(explosionVfx, targetPos, Quaternion.identity);

        if (groundPrefab != null)
        {
            var g = Instantiate(groundPrefab, targetPos, Quaternion.identity);

            // ✅ 장판이 자기 수명을 관리하도록 duration만 세팅
            var slow = g.GetComponent<SlimeQ_GroundSlow>();
            if (slow != null)
                slow.duration = groundLifetime;
        }

        Destroy(gameObject);
    }

    // 날아오는 중 플레이어가 맞으면 즉시 데미지(원하면 사용)
    private void OnTriggerEnter2D(Collider2D other)
    {
        var hp = other.GetComponentInParent<PlayerHP>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            ExplodeAndSpawnGround();
        }
    }
}
