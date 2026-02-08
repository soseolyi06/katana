using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryPassive : MonoBehaviour
{
    [Header("Unlock")]
    public bool unlocked = false;

    [Header("Layers")]
    public LayerMask enemyLayer;
    public LayerMask bulletLayer; // Bullet 레이어

    [Header("Parry (Cone) - Upgradeable")]
    public int parryDamage = 10;
    public float parryRadius = 3.5f;
    [Range(1f, 180f)] public float parryAngle = 90f;

    [Header("Cone Feel Tuning")]
    [Tooltip("대각선에서 판정이 빡빡하게 느껴질 때 3~8 정도로 올리면 체감이 좋아짐")]
    public float angleTolerance = 5f;

    [Header("Origin Offset")]
    [Tooltip("판정 원/부채꼴의 중심점을 forward 반대(뒤) 방향으로 얼마나 이동할지")]
    public float originBackOffset = 0.4f; // 0.2~0.8 추천

    [Header("Hit Stop")]
    [Tooltip("플레이어/적/투사체만 잠깐 멈춤")]
    public float hitStopTime = 0.1f;

    [Header("VFX/SFX (Play AFTER hit stop)")]
    public AudioClip parrySfx;
    public GameObject parryVfxPrefab;
    public float vfxLifeTime = 1.0f;

    // --- refs ---
    private PlayerMove playerMove;
    private AudioSource audioSource;

    // ✅ 대쉬 1회당 패링 1회 제한
    private int lastParryDashId = -1;

    // ✅ 패링 순간 방향 고정(히트스톱 이후 판정에 사용)
    private Vector2 lockedForward = Vector2.down;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// SwordHitbox에서 Bullet과 충돌하면 호출
    /// </summary>
    public void TryParry(GameObject bulletObj)
    {
        if (!enabled) return;   // ✅ 스크립트가 꺼져있으면 어떤 상황에서도 패링 불가
        if (!unlocked) return;
        if (playerMove == null || !playerMove.IsDashing) return;
        if (bulletObj == null) return;

        // Bullet 레이어 체크(안전)
        int bit = 1 << bulletObj.layer;
        if ((bulletLayer.value & bit) == 0) return;

        // ✅ 대쉬 1회당 1번만
        if (playerMove.DashId == lastParryDashId) return;
        lastParryDashId = playerMove.DashId;

        // ✅ 방향을 고정(대각선 미묘함 방지)
        lockedForward = GetDashForward().normalized;

        // 즉시 반응감용: 닿은 bullet은 바로 제거 (선택)
        Destroy(bulletObj);

        // ✅ 히트스톱 → 판정(고정 방향) → 피드백
        StartCoroutine(HitStopThenResolveParry());
    }

    private Vector2 GetDashForward()
    {
        // “바라보는 방향”은 대쉬 방향 기준
        if (playerMove != null)
        {
            if (playerMove.DashDir.sqrMagnitude > 0.001f) return playerMove.DashDir;
            if (playerMove.LastMoveDir.sqrMagnitude > 0.001f) return playerMove.LastMoveDir;
        }
        return Vector2.down;
    }

    private IEnumerator HitStopThenResolveParry()
    {
        // 1) 히트스톱(플레이어/적/투사체만)
        yield return StartCoroutine(HitStopOnly(hitStopTime));

        // ✅ 정지 해제 직후 보정 대쉬
        if (playerMove != null)
        {
            // lockedForward는 패링 발동 순간 고정한 방향
            playerMove.NudgeDash(lockedForward, 1f, 0.08f);
        }

        // 2) 히트스톱 끝난 직후 판정(✅ lockedForward 사용)
        ResolveParry(lockedForward);

        // 3) 피드백 (원하는 타이밍)
        PlayFeedback();
    }

    private void ResolveParry(Vector2 forward)
    {
        Vector2 f = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector2.down;

        // ✅ 중심점: 플레이어보다 살짝 뒤
        Vector2 origin = (Vector2)transform.position - f * originBackOffset;

        // Bullet 삭제 + Enemy 데미지를 같은 기준(origin, f)으로 처리
        DestroyBulletsInCone_Dot(origin, f);
        DealEnemiesInCone_Dot(origin, f);
    }

    private IEnumerator HitStopOnly(float duration)
    {
        // 플레이어/적/투사체만 멈춤 (Rigidbody2D simulated false)
        var bodies = new List<Rigidbody2D>();
        CollectBodies(enemyLayer, bodies);
        CollectBodies(bulletLayer, bodies);

        var prb = GetComponent<Rigidbody2D>();
        if (prb != null && !bodies.Contains(prb)) bodies.Add(prb);

        var oldVel = new Dictionary<Rigidbody2D, Vector2>(bodies.Count);

        for (int i = 0; i < bodies.Count; i++)
        {
            var rb = bodies[i];
            if (rb == null) continue;

            oldVel[rb] = rb.linearVelocity;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(duration);

        foreach (var kv in oldVel)
        {
            if (kv.Key == null) continue;
            kv.Key.simulated = true;
            kv.Key.linearVelocity = kv.Value;
        }
    }

    private void DestroyBulletsInCone_Dot(Vector2 origin, Vector2 forward)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, parryRadius, bulletLayer);
        if (hits == null) return;

        float halfRad = (parryAngle * 0.5f + angleTolerance) * Mathf.Deg2Rad;
        float minDot = Mathf.Cos(halfRad);

        HashSet<GameObject> toDestroy = new HashSet<GameObject>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            // ✅ 체감 개선: transform.position 대신 bounds.center
            Vector2 target = col.bounds.center;
            Vector2 to = target - origin;
            if (to.sqrMagnitude < 0.0001f) continue;

            float dot = Vector2.Dot(forward, to.normalized);
            if (dot < minDot) continue;

            toDestroy.Add(col.gameObject);
        }

        foreach (var go in toDestroy)
        {
            if (go != null) Destroy(go);
        }
    }

    private void DealEnemiesInCone_Dot(Vector2 origin, Vector2 forward)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, parryRadius, enemyLayer);
        if (hits == null) return;

        float halfRad = (parryAngle * 0.5f + angleTolerance) * Mathf.Deg2Rad;
        float minDot = Mathf.Cos(halfRad);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            Vector2 target = col.bounds.center;
            Vector2 to = target - origin;
            if (to.sqrMagnitude < 0.0001f) continue;

            float dot = Vector2.Dot(forward, to.normalized);
            if (dot < minDot) continue;

            var stat = col.GetComponent<SlimeStatManager>() ?? col.GetComponentInParent<SlimeStatManager>();
            if (stat != null)
            {
                stat.TakeDamage(parryDamage);
                continue;
            }

            // ✅ 보스
            var bossHp = col.GetComponent<BossHealth>() ?? col.GetComponentInParent<BossHealth>();
            if (bossHp != null)
            {
                bossHp.TakeDamage(parryDamage);
                continue;
            }
        }
    }

    private void PlayFeedback()
    {
        if (parrySfx != null)
        {
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(parrySfx);
        }

        if (parryVfxPrefab != null)
        {
            var vfx = Instantiate(parryVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, vfxLifeTime);
        }
    }

    private void CollectBodies(LayerMask mask, List<Rigidbody2D> list)
    {
        // 패링 발동 순간에만 탐색 (적당히 무난)
        var bodies = GameObject.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        for (int i = 0; i < bodies.Length; i++)
        {
            var rb = bodies[i];
            if (rb == null) continue;

            int bit = 1 << rb.gameObject.layer;
            if ((mask.value & bit) == 0) continue;

            if (!list.Contains(rb))
                list.Add(rb);
        }
    }

    // --- Gizmos: 부채꼴 윤곽 (선택했을 때) ---
    private void OnDrawGizmosSelected()
    {
        if (!unlocked) return;

        Vector2 forward = Vector2.down;
        if (Application.isPlaying)
            forward = lockedForward.sqrMagnitude > 0.001f ? lockedForward.normalized : Vector2.down;

        Vector2 origin = (Vector2)transform.position - forward * originBackOffset;

        // 반경 원
        Gizmos.DrawWireSphere(origin, parryRadius);

        // 좌/우 경계선
        float half = (parryAngle * 0.5f + angleTolerance);
        Vector2 left = Rotate(forward, -half).normalized;
        Vector2 right = Rotate(forward, half).normalized;

        Gizmos.DrawLine(origin, origin + left * parryRadius);
        Gizmos.DrawLine(origin, origin + right * parryRadius);
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
