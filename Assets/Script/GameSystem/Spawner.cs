using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public float spawnY = 4.6f;
    public float spawnXMin = -2.4f;
    public float spawnXMax = 2.5f;

    [Header("Boss Spawn")]
    public Transform bossSpawnPoint;   // 보스 고정 위치(씬에서 지정)

    private RoundManager roundManager;

    private void Awake()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
    }

    // (기존 기능 유지) 일반 적: 랜덤 X로 소환
    public GameObject Spawn(GameObject prefab)
    {
        if (prefab == null) return null;

        float x = Random.Range(spawnXMin, spawnXMax);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        return SpawnAt(prefab, pos);
    }

    // (추가) 원하는 위치에 소환 (보스/특수 몹/연출용 등 공통)
    public GameObject SpawnAt(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        var reporter = obj.GetComponent<SlimeDeathReporter>();
        if (reporter != null && roundManager != null)
        {
            reporter.Bind(roundManager);
        }

        roundManager?.OnMonsterSpawned();
        return obj;
    }

    // (추가) 보스 소환: 무조건 지정 위치
    public GameObject SpawnBoss(GameObject bossPrefab)
    {
        if (bossSpawnPoint == null)
        {
            Debug.LogError("[Spawner] bossSpawnPoint가 지정되지 않았습니다.");
            return null;
        }

        return SpawnAt(bossPrefab, bossSpawnPoint.position);
    }

    public void SpawnBurst(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
            Spawn(prefab);
    }
}
