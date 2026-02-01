using System;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [Header("Cards (Assign 3 cards: Left/Mid/Right)")]
    [SerializeField] private ChoiceCard leftCard;
    [SerializeField] private ChoiceCard midCard;
    [SerializeField] private ChoiceCard rightCard;

    public event Action<ChoiceDefinition> onPicked;

    private void Awake()
    {
        // 시작 시 숨김(라운드 클리어 때만 켬)
        gameObject.SetActive(false);

        // 카드 클릭 이벤트 연결
        if (leftCard != null) leftCard.onSelected += HandlePick;
        if (midCard != null) midCard.onSelected += HandlePick;
        if (rightCard != null) rightCard.onSelected += HandlePick;
    }

    private void OnDestroy()
    {
        if (leftCard != null) leftCard.onSelected -= HandlePick;
        if (midCard != null) midCard.onSelected -= HandlePick;
        if (rightCard != null) rightCard.onSelected -= HandlePick;
    }

    public void Show(IReadOnlyList<ChoiceDefinition> defs)
    {
        if (defs == null || defs.Count < 3)
        {
            Debug.LogError("[ChoiceUI] Need exactly 3 choices to show.");
            return;
        }

        gameObject.SetActive(true);

        leftCard.Bind(defs[0]);
        midCard.Bind(defs[1]);
        rightCard.Bind(defs[2]);

        leftCard.SetInteractable(true);
        midCard.SetInteractable(true);
        rightCard.SetInteractable(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HandlePick(ChoiceDefinition def)
    {
        // 중복 클릭 방지(선택 즉시 잠금)
        leftCard.SetInteractable(false);
        midCard.SetInteractable(false);
        rightCard.SetInteractable(false);

        onPicked?.Invoke(def);
    }
}
