using UnityEngine;

public class SlimeStatManager : MonoBehaviour
{
    // =========================================================
    // ✅ SlimeStatManager의 역할
    // - 이 슬라임이 어떤 성능을 가지는지 "데이터"로 정의한다
    // - 이동 / 공격 / 체력 등의 수치를 보관만 한다
    // - 실제 행동(이동, 데미지 처리)은 SlimeMove가 담당한다
    // =========================================================

    [Header("HP Stat")]
    [Tooltip("슬라임의 최대 체력")]
    public int maxHp = 20;

    [Tooltip("현재 체력 (보통은 maxHp로 시작)")]
    public int currentHp;

    [Header("Combat Stat")]
    [Tooltip("Goal에 도달했을 때 플레이어에게 주는 데미지")]
    public int attack = 5;

    [Header("Move Stat")]
    [Tooltip("아래로 이동하는 속도")]
    public float moveSpeed = 2f;

    [Header("Option")]
    [Tooltip("무기를 가진 슬라임인지 여부 (확장용)")]
    public bool hasWeapon = false;

    [Header("Weapon Slot")]
    [Tooltip("무기 보유 슬라임일 때 사용할 발사 스크립트(컴포넌트)를 넣어주세요.")]
    public MonoBehaviour weaponShooter; 
    // ↑ 나중에 BulletShooter 같은 스크립트를 여기 넣을 예정

    // 나머지 스탯들...

    void Awake()
    {
        // ---------------------------------------------------------
        // 게임 시작 시 현재 체력을 최대 체력으로 맞춘다
        // ---------------------------------------------------------
        currentHp = maxHp;
    }

    /// <summary>
    /// ✅ 슬라임이 데미지를 받을 때 호출하는 함수
    /// (나중에 플레이어 공격 구현할 때 사용)
    /// </summary>
    public void TakeDamage(int damage)
    {
        // 데미지가 0 이하이면 무시
        if (damage <= 0) return;

        currentHp -= damage;

        // 체력은 0 아래로 내려가지 않게 한다
        if (currentHp < 0)
            currentHp = 0;

        // 체력이 0이 되면 사망 처리
        if (currentHp == 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ✅ 슬라임 사망 처리
    /// </summary>
    void Die()
    {
        // (선택) 이펙트, 점수 증가, 드랍 처리 등
        // Debug.Log($"{name} Slime Die");

        Destroy(gameObject);
    }
}
