using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Coroutine shakeCo;
    Vector3 originalLocalPos;

    [Header("Default Shake")]
    public float defaultDuration = 0.12f;
    public float defaultStrength = 0.15f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        // 중복 방지: 기존 쉐이크가 있으면 끊고 새로 시작(덮어쓰기 방식)
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeCo(duration, strength));
    }

    public void ShakeDefault()
    {
        Shake(defaultDuration, defaultStrength);
    }

    IEnumerator ShakeCo(float duration, float strength)
    {
        float t = 0f;

        while (t < duration)
        {
            // 2D 탑다운이면 Z 흔들림은 보통 빼는 게 좋음
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            t += Time.unscaledDeltaTime; // 히트스톱(시간정지) 중에도 흔들리게
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeCo = null;
    }
}
