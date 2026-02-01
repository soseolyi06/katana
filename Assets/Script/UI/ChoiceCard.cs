using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceCard : MonoBehaviour
{
    [Header("Wired UI References (Assign in Inspector)")]
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Default Backgrounds By Type (Optional)")]
    [SerializeField] private Sprite statBg;
    [SerializeField] private Sprite passiveBg;
    [SerializeField] private Sprite tradeoffBg;
    [SerializeField] private Sprite activeBg;

    public ChoiceDefinition CurrentDefinition { get; private set; }

    /// <summary>
    /// 카드가 선택되었을 때 호출됩니다.
    /// ChoiceManager가 이 이벤트에 구독해서 적용 로직을 처리하면 됩니다.
    /// </summary>
    public event Action<ChoiceDefinition> onSelected;

    private void Reset()
    {
        // 자동 참조 시도(오브젝트 구조가 동일하면 대부분 자동으로 채워짐)
        button = GetComponent<Button>();

        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.name.Equals("BackGround", StringComparison.OrdinalIgnoreCase)) backgroundImage = img;
            else if (img.name.Equals("Icon", StringComparison.OrdinalIgnoreCase)) iconImage = img;
        }

        var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmps)
        {
            if (t.name.Equals("Title", StringComparison.OrdinalIgnoreCase)) titleText = t;
            else if (t.name.Equals("Description", StringComparison.OrdinalIgnoreCase)) descriptionText = t;
            
        }
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    public void Bind(ChoiceDefinition def)
    {
        CurrentDefinition = def;

        if (def == null)
        {
            // 방어적으로 비워두기
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (titleText != null) titleText.text = def.title;
        if (descriptionText != null) descriptionText.text = def.description;

        

        // Icon: 없으면 숨김
        if (iconImage != null)
        {
            if (def.icon != null)
            {
                iconImage.sprite = def.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.sprite = null;
                iconImage.gameObject.SetActive(false);
            }
        }

        // Background: def.background 우선, 없으면 타입별 기본 배경
        if (backgroundImage != null)
        {
            Sprite bg = def.background != null ? def.background : GetDefaultBg(def.type);
            if (bg != null) backgroundImage.sprite = bg;
        }

        // 버튼 활성화(필요하면 ChoiceManager에서 잠금 처리 가능)
        if (button != null) button.interactable = true;
    }

    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
    }

    private void HandleClick()
    {
        if (CurrentDefinition == null) return;
        onSelected?.Invoke(CurrentDefinition);
    }

    private void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    private Sprite GetDefaultBg(ChoiceType type)
    {
        return type switch
        {
            ChoiceType.Stat => statBg,
            ChoiceType.Passive => passiveBg,
            ChoiceType.Tradeoff => tradeoffBg,
            ChoiceType.Active => activeBg,
            _ => null
        };
    }
}
