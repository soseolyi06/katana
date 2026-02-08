using System;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;

    [Header("Pattern Settings")]
    [SerializeField] private float patternCooldown = 1.5f;
    [SerializeField] private List<BossPattern> patterns = new List<BossPattern>();

    private float nextPatternTime;
    private Animator currentAnimator;

    private BossPattern currentPattern;
    private bool firedThisAttack;

    public Transform Player => player;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    private void Update()
    {
        if (!player) return;

        HandleCurrentAttackTiming();

        if (Time.time < nextPatternTime) return;
        if (IsInAttackState()) return;

        TrySelectAndStartPattern();
    }

    private void TrySelectAndStartPattern()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        List<BossPattern> candidates = new List<BossPattern>();
        int totalWeight = 0;

        foreach (var p in patterns)
        {
            if (!p.enabled) continue;
            if (dist < p.minRange || dist > p.maxRange) continue;
            if (p.weight <= 0) continue;

            // 액션 오브젝트 없으면 후보 제외
            if (p.actionObject == null) continue;

            // 액션 컴포넌트(IBossPatternAction) 없으면 후보 제외
            if (!HasActionComponent(p.actionObject)) continue;

            candidates.Add(p);
            totalWeight += p.weight;
        }

        if (candidates.Count == 0 || totalWeight <= 0)
        {
            nextPatternTime = Time.time + 0.25f;
            return;
        }

        int roll = UnityEngine.Random.Range(1, totalWeight + 1);
        int sum = 0;

        BossPattern chosen = null;
        foreach (var p in candidates)
        {
            sum += p.weight;
            if (roll <= sum)
            {
                chosen = p;
                break;
            }
        }

        if (chosen == null)
        {
            nextPatternTime = Time.time + 0.25f;
            return;
        }

        StartPattern(chosen);
    }

    private void StartPattern(BossPattern pattern)
    {
        currentPattern = pattern;
        firedThisAttack = false;

        currentAnimator = pattern.animatorOverride != null ? pattern.animatorOverride : animator;

        currentAnimator.ResetTrigger(pattern.animTrigger);
        currentAnimator.SetTrigger(pattern.animTrigger);

        nextPatternTime = Time.time + patternCooldown;
    }

    private void HandleCurrentAttackTiming()
    {
        if (currentPattern == null) return;
        if (currentAnimator == null) return;

        var st = currentAnimator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(currentPattern.animStateName)) return;

        float t = st.normalizedTime;

        if (!firedThisAttack && t >= currentPattern.hitNormalizedTime)
        {
            firedThisAttack = true;
            ExecutePatternAction(currentPattern);
        }

        if (t >= 1f)
        {
            currentPattern = null;
            firedThisAttack = false;
            currentAnimator = null;
        }
    }

    private bool IsInAttackState()
    {
        if (currentPattern == null) return false;
        if (currentAnimator == null) return false;

        var st = currentAnimator.GetCurrentAnimatorStateInfo(0);
        return st.IsName(currentPattern.animStateName);
    }

    private void ExecutePatternAction(BossPattern p)
    {
        if (p.actionObject == null) return;

        var comps = p.actionObject.GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] is IBossPatternAction action)
            {
                action.Execute(this, p);
                return;
            }
        }

        // Debug.LogWarning($"No IBossPatternAction found on {p.actionObject.name}");
    }

    private bool HasActionComponent(GameObject obj)
    {
        var comps = obj.GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] is IBossPatternAction) return true;
        }
        return false;
    }
}

public interface IBossPatternAction
{
    void Execute(BossController owner, BossPattern pattern);
}

[Serializable]
public class BossPattern
{
    public bool enabled = true;

    [Header("Identity")]
    public string name = "Skill1";

    [Header("Animator Override (Optional)")]
    public Animator animatorOverride;

    [Header("Animator Link")]
    public string animTrigger = "Skill1";
    public string animStateName = "SlimeQ_Skill1";
    [Range(0f, 1f)] public float hitNormalizedTime = 0.35f;

    [Header("Chance & Range")]
    [Range(0, 100)] public int weight = 100;
    public float minRange = 0f;
    public float maxRange = 6f;

    [Header("Action Object (Required)")]
    public GameObject actionObject; // ✅ 보스 자식 Empty (실제 공격 구현 담당)
}
