using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Clips")]
    [Header("Slime Sounds")]
    [SerializeField] private AudioClip enemyHitClip;  // ✅ 적 타격(유효 히트) 사운드
    [Range(0f, 1f)] public float enemyHitVolume = 1f;

    [SerializeField] private AudioClip enemyDeathClip;  // 사망
    [Range(0f, 1f)] public float enemyDeathVolume = 1f;

    [SerializeField] private int maxEnemyDeathPerFrame = 2;
    private int enemyDeathCountThisFrame;
    private int lastFrame;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField, Range(0f, 1f)] private float playerHitVolume = 0.8f;
    [SerializeField] private float playerHitCooldown = 0.08f;
    private float lastPlayerHitTime = -999f;

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
        sfxSource.PlayOneShot(enemyHitClip, enemyHitVolume);
    }


 
    public void PlayEnemyDeath()
    {
        if (enemyDeathClip == null || sfxSource == null) return;

        if (Time.frameCount != lastFrame)
        {
            lastFrame = Time.frameCount;
            enemyDeathCountThisFrame = 0;
        }

        if (enemyDeathCountThisFrame >= maxEnemyDeathPerFrame)
            return;

        enemyDeathCountThisFrame++;
        sfxSource.PlayOneShot(enemyDeathClip);
    }

    // 너무 자주 맞을 때 귀/노이즈 방지용(필요하면 0.05~0.12 추천)
    public void PlayPlayerHit()
    {
        if (playerHitClip == null || sfxSource == null) return;

        if (Time.time - lastPlayerHitTime < playerHitCooldown)
            return;

        lastPlayerHitTime = Time.time;
        sfxSource.PlayOneShot(playerHitClip, playerHitVolume);
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
