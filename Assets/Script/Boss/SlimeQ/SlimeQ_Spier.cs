using UnityEngine;

public class SlimeQ_Spier : MonoBehaviour, IBossSpawnedSkill
{
    [Header("Movement")]
    public float lifeAfterFire = 5f;

    [Header("Collision Tags")]
    public string wallTag = "Wall";
    public string playerTag = "Player";

    // Init params
    private Vector2 fixedTarget;
    private int damage;
    private float speed;
    private GameObject groundPrefab;     // 전달은 받되, 필요 없으면 그냥 null로 두면 됨
    private float groundLifetime;

    // runtime
    private bool fired;
    private Vector2 dir;
    private float fireTimer;

    public void Init(Vector2 fixedTarget, int damage, float speed, GameObject groundPrefab, float groundLifetime)
    {
        this.fixedTarget = fixedTarget;
        this.damage = damage;
        this.speed = speed;
        this.groundPrefab = groundPrefab;
        this.groundLifetime = groundLifetime;

        fired = false;
        fireTimer = 0f;
    }

    private void Update()
    {
        if (!fired) return;

        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        fireTimer += Time.deltaTime;
        if (fireTimer >= lifeAfterFire)
            Destroy(gameObject);
    }

    // ✅ 2번째 애니메이션 시작 프레임에 Animation Event로 호출
    public void Fire()
    {
        if (fired) return;

        dir = (fixedTarget - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.down;

        // 회전이 필요 없으면 이 블록은 지워도 됨
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        fired = true;
        fireTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(wallTag))
        {
            SpawnGroundIfAny();
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag(playerTag))
{
        var hp = other.GetComponent<PlayerHP>() ?? other.GetComponentInParent<PlayerHP>();
        if (hp != null)
            hp.TakeDamage(damage);

        SpawnGroundIfAny();
        Destroy(gameObject);
}
    }

    private void SpawnGroundIfAny()
    {
        if (groundPrefab == null) return;

        var g = Instantiate(groundPrefab, transform.position, Quaternion.identity);
        if (groundLifetime > 0f) Destroy(g, groundLifetime);
    }
}
