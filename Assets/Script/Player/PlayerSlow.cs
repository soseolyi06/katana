using System.Collections.Generic;
using UnityEngine;

public interface IPlayerSpeedAffectable
{
    void ApplySpeedMultiplier(float mul);
    void ClearSpeedMultiplier(float mul);
    float GetCurrentSpeedMultiplier();
}

public class PlayerSlow : MonoBehaviour, IPlayerSpeedAffectable
{
    private readonly List<float> multipliers = new List<float>();

    public void ApplySpeedMultiplier(float mul)
    {
        if (!multipliers.Contains(mul)) multipliers.Add(mul);
    }

    public void ClearSpeedMultiplier(float mul)
    {
        multipliers.Remove(mul);
    }

    public float GetCurrentSpeedMultiplier()
    {
        float m = 1f;
        for (int i = 0; i < multipliers.Count; i++) m *= multipliers[i];
        return m;
    }
}
