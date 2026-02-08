using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChoicePicker
{
    /// <summary>
    /// 기존 기능: slots에서 count개를 가중치 랜덤으로 중복 없이 뽑기
    /// </summary>
    public static List<ChoiceDefinition> PickUnique(List<ChoiceSlot> slots, int count)
    {
        return PickUniqueFiltered(slots, count, null);
    }

    /// <summary>
    /// 추가 기능: filter를 통과한 후보들만 대상으로 count개를 가중치 랜덤으로 중복 없이 뽑기
    /// filter가 null이면 전체 후보 대상.
    /// </summary>
    public static List<ChoiceDefinition> PickUniqueFiltered(
        List<ChoiceSlot> slots,
        int count,
        Func<ChoiceDefinition, bool> filter)
    {
        // 후보 풀 구성 (choice != null, weight > 0, filter 통과)
        List<ChoiceSlot> pool = new List<ChoiceSlot>();
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s == null) continue;
            if (s.choice == null) continue;
            if (s.weight <= 0) continue;

            if (filter != null && !filter(s.choice))
                continue;

            pool.Add(s);
        }

        List<ChoiceDefinition> result = new List<ChoiceDefinition>();

        count = Mathf.Clamp(count, 0, pool.Count);

        for (int i = 0; i < count; i++)
        {
            ChoiceSlot picked = PickOneByWeight(pool);
            if (picked == null) break;

            result.Add(picked.choice);
            pool.Remove(picked); // ✅ 중복 방지
        }

        return result;
    }

    private static ChoiceSlot PickOneByWeight(List<ChoiceSlot> pool)
    {
        // 유효 후보(Choice가 있고 weight>0)
        int total = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].choice == null) continue;
            if (pool[i].weight <= 0) continue;
            total += pool[i].weight;
        }

        // 모두 0이면: fallback(균등 랜덤)
        if (total <= 0)
        {
            List<ChoiceSlot> valid = new List<ChoiceSlot>();
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].choice != null) valid.Add(pool[i]);

            if (valid.Count == 0) return null;
            return valid[UnityEngine.Random.Range(0, valid.Count)];
        }

        int r = UnityEngine.Random.Range(0, total);
        int acc = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            var s = pool[i];
            if (s.choice == null || s.weight <= 0) continue;

            acc += s.weight;
            if (r < acc)
                return s;
        }

        return null;
    }
}
