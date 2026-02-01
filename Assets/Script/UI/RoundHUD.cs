using TMPro;
using UnityEngine;

public class RoundHUD : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private TextMeshProUGUI roundText;

    private void Awake()
    {
        if (roundManager == null) roundManager = FindFirstObjectByType<RoundManager>();
    }

    private void Start()
    {
        Refresh();
    }

    // 필요하면 라운드 변경 이벤트로 갱신하는 쪽이 더 깔끔하지만,
    // 지금은 가장 단순한 방식으로도 충분히 안정적으로 동작해.
    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (roundManager == null || roundText == null) return;

        int n = roundManager.GetCurrentRoundNumber(); // 1부터
        int chapter = (n - 1) / 10 + 1;
        int stage = (n - 1) % 10 + 1;

        roundText.text = $"스테이지 : {chapter}-{stage}";
    }
}
