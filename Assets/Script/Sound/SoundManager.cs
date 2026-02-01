using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip enemyHitClip;  // ✅ 적 타격(유효 히트) 사운드

    private void Awake()
    {
        // Singleton
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 자동 세팅
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // 2D SFX 기본값
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
    }

    /// <summary>
    /// 적에게 유효 타격 발생 시 1회 재생
    /// </summary>
    public void PlayEnemyHit()
    {
        if (enemyHitClip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(enemyHitClip);
    }

    /// <summary>
    /// 원하는 클립을 원샷으로 재생(추후 확장용)
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
