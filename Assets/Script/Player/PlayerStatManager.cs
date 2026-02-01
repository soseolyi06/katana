using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    // -----------------------------
    // [싱글톤] 어디서든 PlayerStatManager.I 로 접근하기 위한 장치
    // 초보자에게는 "전역 접근용 한 개만 존재하는 매니저"로 이해하면 됨
    // -----------------------------
    public static PlayerStatManager I;

    // -----------------------------
    // [이동 스탯] base / bonus / final
    // base  : 기본값(캐릭터 원래 능력)
    // bonus : 옵션(버프)로 누적되는 변화량
    // final : base + bonus의 결과(플레이어가 실제로 쓰는 값)
    // -----------------------------
    [Header("Move Speed")]
    public float baseMoveSpeed = 3f;     // 기본 이동속도
    public float moveSpeedBonus = 0f;    // 옵션 누적 이동속도
    public float finalMoveSpeed;         // 최종 이동속도 (플레이어가 사용)

    // -----------------------------
    // [대쉬 스탯] base / bonus / final
    // dashSpeed     : 대쉬 순간 속도
    // dashDuration  : 대쉬 유지 시간
    // dashCooldown  : 대쉬 재사용 대기시간
    // -----------------------------
    [Header("Dash")]
    public float baseDashSpeed = 8f;
    public float dashSpeedBonus = 0f;
    public float finalDashSpeed;

    public float baseDashDuration = 0.15f;
    public float dashDurationBonus = 0f;
    public float finalDashDuration;

    public float baseDashCooldown = 0.8f;
    public float dashCooldownBonus = 0f;
    public float finalDashCooldown;

    // =========================================================
    // [HP STAT] 최대 체력 관련 스탯
    // - Base  : 캐릭터의 기본 최대 체력
    // - Bonus : 옵션 / 아이템 / 버프로 추가되는 값
    // - Final : 실제 Player가 참조하는 "최종 최대 체력"
    // =========================================================
    [Header("HP Stat")]
    public int maxHpBase = 100;     // 기본 최대 체력
    public int maxHpBonus = 0;      // 추가 최대 체력(누적)
    public int finalMaxHp;          // 최종 최대 체력 (Player가 참조)

    // -----------------------------
    // Awake: 오브젝트가 생성될 때 가장 먼저 실행되는 함수 중 하나
    // 여기서 싱글톤 설정 + 최초 final 계산
    // -----------------------------
    private void Awake()
    {
        // 이미 I가 있다면(= 씬에 같은 매니저가 2개 이상), 중복 제거
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this; // 이제부터 어디서든 PlayerStatManager.I 로 접근 가능

        // 처음 시작할 때 final 값을 계산해둠
        RecalculateFinal();
    }

    // -----------------------------
    // RecalculateFinal:
    // "base + bonus = final" 을 한 번에 계산하는 함수
    // 옵션이 들어올 때마다 호출해서 최신 final 값을 만들어야 함
    // -----------------------------
    public void RecalculateFinal()
    {
        // 이동
        finalMoveSpeed = baseMoveSpeed + moveSpeedBonus;

        // 대쉬
        finalDashSpeed = baseDashSpeed + dashSpeedBonus;
        finalDashDuration = baseDashDuration + dashDurationBonus;
        finalDashCooldown = baseDashCooldown + dashCooldownBonus;

        // =====================================================
        // [HP] 최종 최대 체력 계산
        // =====================================================

        finalMaxHp = maxHpBase + maxHpBonus;

        // 안전장치: 최대 체력은 최소 1 이상
        if (finalMaxHp < 1)
            finalMaxHp = 1;
    }

    // -----------------------------
    // AddXXX:
    // 옵션(버프)으로 bonus를 누적시키는 함수들
    // "충돌 없음(단순 +=)" 규칙 그대로 구현
    // -----------------------------
    public void AddMoveSpeed(float value)
    {
        moveSpeedBonus += value;  // 같은 옵션 여러 번 받으면 누적(스택)
        RecalculateFinal();       // 누적 후 최종값 갱신
    }

    public void AddDashSpeed(float value)
    {
        dashSpeedBonus += value;
        RecalculateFinal();
    }

    public void AddDashDuration(float value)
    {
        dashDurationBonus += value;
        RecalculateFinal();
    }

    public void AddDashCooldown(float value)
    {
        dashCooldownBonus += value;
        RecalculateFinal();
    }

    /// <summary>
    /// 최대 체력을 증가/감소시키는 함수
    /// - value 값만큼 maxHpBonus를 누적한다
    /// - 옵션/아이템/버프에서 호출하는 용도
    /// </summary>
    public void AddMaxHp(int value)
    {
        // 1) 보너스 누적
        maxHpBonus += value;

        // 2) 최종 스탯 재계산
        RecalculateFinal();
    }

}