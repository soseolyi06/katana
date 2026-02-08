using System.Collections.Generic;
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private PlayerStatManager playerStat;
    [SerializeField] private ChoiceUI choiceUI;

    [Header("Choice Round Tables")]
    [SerializeField] private List<ChoiceRoundData> choiceRounds = new();

    [Header("Active Skill Attach")]
    [Tooltip("비우면 playerStat.transform에 붙습니다.")]
    [SerializeField] private Transform activeSkillRoot;

    [SerializeField] private ParryPassive parryPassive;
    private void Awake()
    {
        if (roundManager == null) roundManager = FindFirstObjectByType<RoundManager>();
        if (playerStat == null) playerStat = FindFirstObjectByType<PlayerStatManager>();
        if (choiceUI == null) choiceUI = FindFirstObjectByType<ChoiceUI>();
        if (parryPassive == null) parryPassive = FindFirstObjectByType<ParryPassive>();

        if (activeSkillRoot == null && playerStat != null)
            activeSkillRoot = playerStat.transform;
    }

    private void OnEnable()
    {
        if (roundManager != null) roundManager.OnRoundCleared += HandleRoundCleared;
        if (choiceUI != null) choiceUI.onPicked += HandlePicked;
    }

    private void OnDisable()
    {
        if (roundManager != null) roundManager.OnRoundCleared -= HandleRoundCleared;
        if (choiceUI != null) choiceUI.onPicked -= HandlePicked;
    }

    private void HandleRoundCleared(int clearedRoundNumber)
    {
        ChoiceRoundData data = FindDataForRound(clearedRoundNumber);

        // 해당 라운드에 선택지 테이블이 없으면 바로 다음 라운드
        if (data == null || data.slots == null || data.slots.Count == 0)
        {
            roundManager.StartNextRoundFromChoice();
            return;
        }

        int pickCount = Mathf.Clamp(data.pickCount, 3, 3); // 좌/중/우 고정이면 3
        List<ChoiceDefinition> picked = PickUniqueWeighted(data.slots, pickCount);

        if (picked.Count < 3)
        {
            Debug.LogWarning("[ChoiceManager] Not enough valid choices. Going next round.");
            roundManager.StartNextRoundFromChoice();
            return;
        }

        choiceUI.Show(picked);
    }

    private ChoiceRoundData FindDataForRound(int clearedRoundNumber)
    {
        for (int i = 0; i < choiceRounds.Count; i++)
        {
            var d = choiceRounds[i];
            if (d != null && d.roundNumber == clearedRoundNumber)
                return d;
        }
        return null;
    }

    private List<ChoiceDefinition> PickUniqueWeighted(List<ChoiceSlot> slots, int count)
    {
        // 후보 풀 구성 (weight>0, choice!=null)
        var pool = new List<(ChoiceDefinition item, int weight)>();
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s == null || s.choice == null) continue;
            if (s.weight <= 0) continue;
            if (!CanAppearByParryState(s.choice)) continue;
            pool.Add((s.choice, s.weight));
        }

        return WeightedPicker.PickUniqueWeighted(pool, count);
    }

    private bool IsParryChoice(ChoiceDefinition def)
    {
        if (def == null) return false;
        return def.parryUnlock || IsParryUpgrade(def);
    }

    private bool IsParryUpgrade(ChoiceDefinition def)
    {
        if (def == null) return false;

        return (def.parryDamageAdd != 0) ||
            (Mathf.Abs(def.parryRadiusAdd) > 0.0001f) ||
            (Mathf.Abs(def.parryAngleAdd) > 0.0001f);
    }

    private bool CanAppearByParryState(ChoiceDefinition def)
    {
        // 패링 관련 선택지가 아니면 항상 통과
        if (def == null || !IsParryChoice(def)) return true;

        // ParryPassive가 없으면: 해금/강화 둘 다 안 뜨게(원하면 true로 바꿔도 됨)
       var parry = parryPassive;
        bool unlocked = (parry != null && parry.unlocked);

        bool isUnlock = def.parryUnlock;
        bool isUpgrade = IsParryUpgrade(def);

        // 미해금: 해금만 허용, 강화 금지
        if (!unlocked)
        {
            if (isUpgrade) return false;
            return true; // unlock 포함 통과
        }

        // 해금: 해금 금지, 강화 허용
        if (unlocked)
        {
            if (isUnlock) return false;
            return true; // upgrade 통과
        }

        return true;
    }


    private void HandlePicked(ChoiceDefinition def)
    {
        choiceUI.Hide();

        ApplyChoice(def);

        roundManager.StartNextRoundFromChoice();
    }

    private void ApplyChoice(ChoiceDefinition def)
    {
        if (def == null) return;

        if (playerStat == null)
        {
            Debug.LogError("[ChoiceManager] PlayerStatManager reference missing.");
            return;
        }

        // ✅ 이미 존재하는 누적 함수 사용
        if (def.addMoveSpeed != 0f) playerStat.AddMoveSpeed(def.addMoveSpeed);
        if (def.addDashCooldownDelta != 0f) playerStat.AddDashCooldown(def.addDashCooldownDelta);
        if (def.addMaxHp != 0) playerStat.AddMaxHp(def.addMaxHp);

        // 공격력은 PlayerStatManager에 API가 아직 없으므로 가정하지 않음
        if (def.addAttack != 0)
        {
            Debug.LogWarning("[ChoiceManager] addAttack is set, but PlayerStatManager has no attack stat API yet.");
        }

        // Passive는 다음 단계에서 “플레이어 처치일 때만” 이벤트에 연결해서 발동시키면 됨
        if (def.healOnKill || def.healOnRoundClear)
        {
            Debug.Log("[ChoiceManager] Passive selected. (healOnKill / healOnRoundClear) -> Next step: store & trigger via events.");
        }

        if (IsParryChoice(def))
        {
            var parry = parryPassive;
            if (parry != null)
            {
                if (def.parryUnlock)
                {
                    parry.unlocked = true;
                    parry.enabled = true;   // ✅ 네가 원하는 구조의 핵심
                    Debug.Log("[ChoiceManager] Passive unlocked: Parry");
                }

                // 강화(해금 상태일 때만 의미 있게 적용되게 하고 싶으면 아래 if로 묶기)
                if (parry.unlocked)
                {
                    parry.enabled = true;  // ✅ 강화 선택 시 사용도 ON(권장)

                    if (def.parryDamageAdd != 0) parry.parryDamage += def.parryDamageAdd;
                    if (Mathf.Abs(def.parryRadiusAdd) > 0.0001f) parry.parryRadius += def.parryRadiusAdd;
                    if (Mathf.Abs(def.parryAngleAdd) > 0.0001f) parry.parryAngle += def.parryAngleAdd;
                }
            }
            else
            {
                Debug.LogWarning("[ChoiceManager] DashParryPassive component is missing on Player.");
            }
        }

        // Active 스킬 프리팹 부착 (최소 구현)
        if (def.activeSkillPrefab != null)
        {
            Transform root = activeSkillRoot != null ? activeSkillRoot : playerStat.transform;
            Instantiate(def.activeSkillPrefab, root);
            Debug.Log("[ChoiceManager] Active skill prefab attached to player.");
        }
    }
}

public static class WeightedPicker
{
    public static List<T> PickUniqueWeighted<T>(IList<(T item, int weight)> source, int count)
    {
        var pool = new List<(T item, int weight)>();
        for (int i = 0; i < source.Count; i++)
        {
            if (source[i].weight > 0)
                pool.Add(source[i]);
        }

        var result = new List<T>(count);
        int pickCount = Mathf.Min(count, pool.Count);

        for (int p = 0; p < pickCount; p++)
        {
            int total = 0;
            for (int i = 0; i < pool.Count; i++)
                total += pool[i].weight;

            int r = Random.Range(0, total);

            int acc = 0;
            int chosenIndex = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                acc += pool[i].weight;
                if (r < acc)
                {
                    chosenIndex = i;
                    break;
                }
            }

            result.Add(pool[chosenIndex].item);
            pool.RemoveAt(chosenIndex); // ✅ 중복 방지
        }

        return result;
    }
}
