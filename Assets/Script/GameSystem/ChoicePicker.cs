using System.Collections.Generic;
using UnityEngine;

public static class ChoicePicker
{
    public static List<ChoiceDefinition> PickUnique(List<ChoiceSlot> slots, int count)
    {
        List<ChoiceSlot> pool = new List<ChoiceSlot>(slots);
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
            // choice가 있는 것만 대상으로
            List<ChoiceSlot> valid = new List<ChoiceSlot>();
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].choice != null) valid.Add(pool[i]);

            if (valid.Count == 0) return null;
            return valid[Random.Range(0, valid.Count)];
        }

        int r = Random.Range(0, total);
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
