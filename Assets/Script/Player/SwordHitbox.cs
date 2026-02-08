using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwordHitbox : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask bulletLayer;   // Bullet 레이어

    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Options")]
    [SerializeField] private bool useTriggerStay = true; // 켜면 Enter 누락 방지용 Stay도 사용

    private Collider2D col;
    private bool isActive;

    // 한 번 공격 중 같은 대상 중복 타격 방지
    private HashSet<int> hitSet = new HashSet<int>();

    // 패링 패시브(없으면 null)
    private ParryPassive parry;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;           // 히트박스는 트리거 전제
        col.enabled = false;            // 시작은 꺼둠

        parry = GetComponentInParent<ParryPassive>();
    }

    /// <summary>
    /// PlayerMove(대쉬 시작/끝)에서 켜고 끄는 함수
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        // ✅ NRE 방지: col은 RequireComponent로 보장 + 그래도 안전하게
        if (col != null)
            col.enabled = active;

        // 공격이 끝나면 기록 초기화
        if (!active)
            hitSet.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        TryHandleHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Enter가 누락되는 경우를 방지하고 싶을 때만 사용
        if (!useTriggerStay) return;
        if (!isActive) return;
        TryHandleHit(other);
    }

    private void TryHandleHit(Collider2D other)
    {
        if (other == null) return;

        int otherBit = 1 << other.gameObject.layer;

        // 1) Bullet이면 -> 패링에 위임 (패링이 없으면 그냥 무시)
        if ((bulletLayer.value & otherBit) != 0)
        {
            if (parry != null)
                parry.TryParry(other.gameObject);
            return;
        }

        // 2) Enemy가 아니면 무시
        if ((enemyLayer.value & otherBit) == 0)
            return;

        // 3) 중복 타격 방지 (같은 프레임/Stay 반복 방지)
        int id = other.gameObject.GetInstanceID();
        if (hitSet.Contains(id)) return;
        hitSet.Add(id);

        // 4) 데미지 적용
        var stat = other.GetComponent<SlimeStatManager>() ?? other.GetComponentInParent<SlimeStatManager>();
        if (stat != null)
        {
            stat.TakeDamage(damage);
            SoundManager.I?.PlayEnemyHit();
            return;
        }

        // ✅ 보스도 피격 가능
        var bossHp = other.GetComponent<BossHealth>() ?? other.GetComponentInParent<BossHealth>();
        if (bossHp != null)
        {
            bossHp.TakeDamage(damage);
            SoundManager.I?.PlayEnemyHit(); // 임시
            return;
        }
    }
}
