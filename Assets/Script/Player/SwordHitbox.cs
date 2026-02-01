using System.Collections.Generic; // HashSet<T> 같은 컬렉션 자료구조 사용
using UnityEngine;                // MonoBehaviour, Collider2D 등 Unity 기능

// [RequireComponent]
// 이 스크립트가 붙는 GameObject에는
// 반드시 Collider2D가 있어야 한다는 강제 규칙
// 없으면 Unity가 자동으로 추가해준다.
[RequireComponent(typeof(Collider2D))]
public class SwordHitbox : MonoBehaviour
{
    public LayerMask enemyLayer;
    // 적에게 줄 데미지 값
    // 인스펙터에서 조절 가능
    public int damage = 5;

    // 이 히트박스에 붙어있는 Collider2D 참조
    private Collider2D col;

    // 현재 히트박스가 활성화 상태인지 여부
    // (공격 중인지 아닌지 판단용)
    private bool isActive;

    // 한 번의 공격(대시, 휘두르기 등) 동안
    // 같은 적을 여러 번 때리는 것을 방지하기 위한 집합
    // int 값은 GameObject의 고유 ID
    private readonly HashSet<int> hitSet = new HashSet<int>();

    // Awake: 오브젝트가 활성화될 때 가장 먼저 호출
    private void Awake()
    {
        // 같은 오브젝트에 붙어 있는 Collider2D 가져오기
        col = GetComponent<Collider2D>();

        // 트리거로 설정
        // 물리적으로 밀어내지 않고
        // 겹쳤는지 여부만 감지하게 만든다
        col.isTrigger = true;

        // 시작할 때는 공격 비활성화 상태
        SetActive(false);
    }

    // 히트박스 활성/비활성 제어 함수
    // 보통 공격 시작/끝 애니메이션 타이밍에 호출됨
    public void SetActive(bool active)
    {
        // 내부 상태 저장
        isActive = active;

        // 콜라이더 자체를 켜거나 끈다
        col.enabled = active;

        // 비활성화될 때
        // 이전 공격에서 기록한 타격 대상 초기화
        if (!active)
            hitSet.Clear();

        Debug.Log($"[Sword] Active = {active}");    
    }

    // Trigger 상태에서 다른 Collider2D와 겹쳤을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 히트박스가 비활성 상태면 무시
        if (!isActive) return;

        // Enemy 레이어가 아니면 공격 대상이 아님
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        // 충돌한 GameObject의 고유 ID 가져오기
        int id = other.gameObject.GetInstanceID();

        // 이미 이번 공격에서 맞은 적이면 다시 처리하지 않음
        if (hitSet.Contains(id)) return;

        // 이번 공격에서 맞았다고 기록
        hitSet.Add(id);

        // 적(GameObject)에서 SlimeStatManager 찾기
        // 바로 붙어있을 수도 있고
        SlimeStatManager stat = other.GetComponent<SlimeStatManager>();

        // 자식 오브젝트에 붙어 있을 수도 있어서
        // 못 찾았으면 부모 쪽에서도 다시 탐색
        if (stat == null)
            stat = other.GetComponentInParent<SlimeStatManager>();

        // StatManager를 찾았다면 데미지 적용
        if (stat != null)
        {
            // TakeDamage:
            // 체력 감소 → 0 이하가 되면 Die() 호출 → Destroy
            stat.TakeDamage(damage);
            if (stat != null)
            {
                stat.TakeDamage(damage);

                // ✅ 유효 타격 1회당 사운드 1회
                if (SoundManager.I != null)
                    SoundManager.I.PlayEnemyHit();
            }
        }
    }
}
