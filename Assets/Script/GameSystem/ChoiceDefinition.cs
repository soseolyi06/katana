using UnityEngine;

public enum ChoiceType
{
    Stat,
    Passive,
    Tradeoff,
    Active
}

[CreateAssetMenu(menuName = "Game/Choice Definition", fileName = "ChoiceDef_")]
public class ChoiceDefinition : ScriptableObject
{
    [Header("UI")]
    public string title;
    [TextArea] public string description;
    public ChoiceType type;

    // ✅ 추가: 선택지 대표 아이콘/배경
    // icon: 공격력/이속/회복 등 "효과 대표 아이콘" (버프 UI에도 재사용)
    // background: 이 선택지만의 특별 배경(옵션). 비워두면 type별 기본 배경 사용
    public Sprite icon;
    public Sprite background;

    [Header("Stat / Tradeoff Values")]
    public int addAttack;
    public int addMaxHp;
    public float addMoveSpeed;
    public float addDashCooldownDelta; // 예: -0.1f면 쿨타임 감소

    [Header("Passive Flags")]
    public bool healOnKill;
    public int healOnKillAmount;
    public bool healOnRoundClear;
    public int healOnRoundClearAmount;

    [Header("Active Skill")]
    public GameObject activeSkillPrefab; // Player에 붙일 스킬 프리팹(선택)
}
