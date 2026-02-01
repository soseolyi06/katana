using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    // =========================================================
    // ✅ PlayerHP의 역할
    // 1) 현재 체력(currentHp)을 "상태값"으로 저장한다.
    // 2) 데미지/회복을 함수(TakeDamage/Heal)로만 처리한다.
    // 3) 최대 체력(maxHp)은 PlayerStatManager가 계산한 "최종값(finalMaxHp)"을 참조한다.
    // =========================================================

    [Header("HP (State)")]
    [Tooltip("현재 체력(전투 중 계속 변하는 값). 최대 체력은 StatManager의 finalMaxHp를 참조함.")]
    public int currentHp;  // 현재 체력 (상태값)

    [Tooltip("사망 상태 여부. currentHp가 0이 되면 true로 전환됨.")]
    public bool isDead = false; // 죽었는지 상태

    [Header("Debug")]
    [Tooltip("Start에서 자동으로 풀피로 시작할지 여부")]
    public bool startFullHp = true; // 시작할 때 풀피로 세팅할지

    void Start()
    {
        // ---------------------------------------------------------
        // 게임 시작 시 체력을 초기화한다.
        // - startFullHp가 true면 최대체력으로 시작(보통 RPG/로그라이크 기본)
        // ---------------------------------------------------------
        if (startFullHp)
        {
            InitHpToMax();
        }
        else
        {
            // startFullHp가 false면 최소한의 보정만 한다.
            // (예: 저장된 currentHp를 인스펙터에서 넣어두고 시작하는 경우)
            ClampHpToMax();
        }
    }

    /// <summary>
    /// ✅ StatManager의 "최종 최대체력(finalMaxHp)"을 가져와서 currentHp를 그 값으로 맞춘다.
    /// - 보통 '게임 시작/리스폰/회복 아이템으로 풀회복' 같은 곳에서 사용.
    /// </summary>
    public void InitHpToMax()
    {
        // 1) StatManager가 존재하는지 확인한다.
        // - 씬에 PlayerStatManager가 없거나 싱글톤 초기화가 안 된 상태면 NullReference가 날 수 있음
        if (PlayerStatManager.I == null)
        {
            Debug.LogWarning("[PlayerHP] PlayerStatManager.I가 null 입니다. 씬에 StatManager가 있는지 확인하세요.");
            return;
        }

        // 2) StatManager가 계산한 "최종 최대체력"을 가져온다.
        int maxHp = PlayerStatManager.I.finalMaxHp;

        // 3) 최대체력이 0 이하이면 이상한 값이므로 안전장치
        if (maxHp <= 0)
        {
            Debug.LogWarning("[PlayerHP] finalMaxHp가 0 이하입니다. 최소 1 이상이 되도록 StatManager 값을 확인하세요.");
            maxHp = 1;
        }

        // 4) 현재 체력을 최대 체력으로 맞춘다(풀피)
        currentHp = maxHp;

        // 5) 사망 상태 해제
        isDead = false;
    }

    /// <summary>
    /// ✅ 데미지를 받는 함수
    /// - damage만큼 currentHp를 줄이고, 0이면 Die() 호출
    /// </summary>
    public void TakeDamage(int damage)
    {
        // 1) 이미 죽었으면 더 이상 처리하지 않는다.
        if (isDead) return;

        // 2) 데미지가 0 이하이면 의미 없으므로 무시한다.
        if (damage <= 0) return;

        // 3) 현재 체력에서 데미지만큼 감소시킨다.
        currentHp -= damage;

        // 4) 체력이 0보다 작아지면 0으로 고정한다.
        if (currentHp < 0)
            currentHp = 0;

        // 5) 체력이 0이 되면 사망 처리
        if (currentHp == 0)
        {
            Die();
        }

        // 6) (선택) 여기서 UI 갱신 호출 가능
        // UpdateHpUI();
        // Debug.Log($"[TakeDamage] damage={damage}, currentHp={currentHp}");
    }

    /// <summary>
    /// ✅ 회복 함수
    /// - amount만큼 currentHp를 늘리되, 최종 최대체력(finalMaxHp)을 넘지 않게 한다.`   
    /// </summary>
    public void Heal(int amount)
    {
        // 1) 죽었으면 회복 불가(룰). 원하면 여기 규칙을 바꿀 수 있음.
        if (isDead) return;

        // 2) 회복량이 0 이하이면 의미 없으니 무시한다.
        if (amount <= 0) return;

        // 3) StatManager가 없으면 최대체력을 알 수 없으니 안전하게 종료한다.
        if (PlayerStatManager.I == null)
        {
            Debug.LogWarning("[PlayerHP] PlayerStatManager.I가 null 입니다. Heal을 적용할 수 없습니다.");
            return;
        }

        // 4) 최종 최대체력을 가져온다.
        int maxHp = PlayerStatManager.I.finalMaxHp;

        // 5) 최대체력이 0 이하이면 안전장치
        if (maxHp <= 0) maxHp = 1;

        // 6) 회복 적용
        currentHp += amount;

        // 7) 최대체력보다 커지면 maxHp로 고정한다.
        if (currentHp > maxHp)
            currentHp = maxHp;

        // 8) (선택) UI 갱신
        // UpdateHpUI();
    }

    /// <summary>
    /// ✅ 최대체력(finalMaxHp)이 바뀌는 상황을 대비한 보정 함수
    /// - 규칙: 현재 체력은 유지하되, maxHp를 초과하면 잘라낸다(Clamp)
    /// - 옵션으로 최대체력 증가/감소가 들어오는 로그라이크에서 특히 중요
    /// </summary>
    public void ClampHpToMax()
    {
        // 1) StatManager가 없으면 보정이 불가능하니 종료
        if (PlayerStatManager.I == null) return;

        // 2) 최종 최대체력 가져오기
        int maxHp = PlayerStatManager.I.finalMaxHp;

        // 3) 안전장치
        if (maxHp <= 0) maxHp = 1;

        // 4) 현재 체력이 최대체력을 넘으면 잘라낸다.
        if (currentHp > maxHp)
            currentHp = maxHp;

        // 5) 현재 체력이 0 미만이면 0으로
        if (currentHp < 0)
            currentHp = 0;

        // 6) 현재 체력이 0이라면 사망 상태로 맞춰줄지 여부
        // - 보통 0이면 사망 처리하는 게 자연스럽다.
        if (currentHp == 0)
            isDead = true;
    }

    /// <summary>
    /// ✅ 사망 처리 함수
    /// - 현재는 최소 구현만.
    /// - 나중에 여기서 애니메이션, 입력 잠금, 게임오버 UI 등을 호출하면 된다.
    /// </summary>
    void Die()
    {
        // 1) 사망 상태로 전환
        isDead = true;

        // 2) 사망 로그(디버그)
        Debug.Log("[PlayerHP] Player Die()");

        // 3) (선택) 이동/대시를 멈추고 싶다면 PlayerMove 같은 컴포넌트에 요청할 수 있다.
        //    지금은 결합도를 낮추기 위해 주석 처리.
        //
        // PlayerMove move = GetComponent<PlayerMove>();
        // if (move != null)
        // {
        //     move.StopAllMotion(); // 너가 만들 함수(예시)
        // }

        // 4) (선택) 게임오버 처리도 여기서 호출 가능
        // GameManager.I.GameOver();
    }
}
