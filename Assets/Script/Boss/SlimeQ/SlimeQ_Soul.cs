using UnityEngine;

public class YScalePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("한 번 늘었다가 원래로 돌아오기까지의 총 시간(초)")]
    [Min(0.01f)] public float cycleDuration = 1.0f;

    [Tooltip("Y 스케일이 얼마나 증가할지(원래값 + delta)")]
    public float scaleDeltaY = -0.1f;

    [Tooltip("시작 딜레이(초)")]
    [Min(0f)] public float startDelay = 0f;

    [Tooltip("애니메이션 곡선 느낌을 더 주고 싶으면 사용(1이면 기본)")]
    [Range(0.1f, 3f)] public float sharpness = 1f;

    private Vector3 baseScale;
    private float t;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        t = 0f;
        // 혹시 비활성/활성 반복 시 스케일이 꼬이는 것 방지
        transform.localScale = baseScale;
    }

    private void Update()
    {
        if (cycleDuration <= 0f) return;

        t += Time.deltaTime;

        if (t < startDelay) return;

        float time = t - startDelay;

        // 0~1로 정규화된 진행도 (한 사이클)
        float phase = (time % cycleDuration) / cycleDuration;

        // 사인: 0 -> 1 -> 0 (한 사이클)
        // sin(0)=0, sin(pi/2)=1, sin(pi)=0
        float wave = Mathf.Sin(phase * Mathf.PI);

        // sharpness로 약간의 "쫀득함" 조절 (1=기본, >1 더 뾰족)
        if (Mathf.Abs(sharpness - 1f) > 0.001f)
            wave = Mathf.Pow(wave, sharpness);

        float y = baseScale.y + scaleDeltaY * wave;

        transform.localScale = new Vector3(baseScale.x, y, baseScale.z);
    }
}
