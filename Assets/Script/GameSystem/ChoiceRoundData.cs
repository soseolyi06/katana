using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChoiceSlot
{
    public ChoiceDefinition choice;
    [Range(0, 100)] public int weight = 10;
}

[CreateAssetMenu(menuName = "Game/Choice Round Data", fileName = "ChoiceRound_")]
public class ChoiceRoundData : ScriptableObject
{
    [Tooltip("이 데이터가 적용될 라운드(1부터)")]
    public int roundNumber = 1;

    [Tooltip("이 풀에서 몇 개를 보여줄지 (기본 3)")]
    public int pickCount = 3;

    public List<ChoiceSlot> slots = new List<ChoiceSlot>();
}
