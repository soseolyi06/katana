using System.Collections.Generic;
using UnityEngine;

public class SlimeQ_GroundSlow : MonoBehaviour
{
    [Range(0.1f, 1f)] public float slowMultiplier = 0.6f;
    public float duration = 3f;

    private readonly HashSet<IPlayerSpeedAffectable> applied = new();

    private void Start() => Destroy(gameObject, duration);

    private void OnTriggerEnter2D(Collider2D other)
    {
        var mover = other.GetComponentInParent<IPlayerSpeedAffectable>();
        if (mover == null) return;

        if (applied.Add(mover))
            mover.ApplySpeedMultiplier(slowMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var mover = other.GetComponentInParent<IPlayerSpeedAffectable>();
        if (mover == null) return;

        if (applied.Remove(mover))
            mover.ClearSpeedMultiplier(slowMultiplier);
    }
}
