using UnityEngine;
using UnityEngine.Events;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    [Min(1)] public int maxHp = 500;
    [SerializeField] private int currentHp;

    [Header("Events (UI/연출 연결용)")]
    public UnityEvent<int, int> onHpChanged;   // (current, max)
    public UnityEvent onDied;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    private bool dead;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 1, maxHp);
        if (currentHp <= 0) currentHp = maxHp;

        // 시작 시 UI 초기 갱신용
        onHpChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;
        if (amount <= 0) return;

        currentHp -= amount;
        if (currentHp < 0) currentHp = 0;

        onHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp == 0)
            Die();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        onDied?.Invoke();

        // 지금은 일단 제거(나중에 컷신/페이즈/드랍 등으로 교체)
        Destroy(gameObject);
    }
}
