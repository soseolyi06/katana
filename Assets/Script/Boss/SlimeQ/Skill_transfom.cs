using System.Collections.Generic;
using UnityEngine;

public class Skill_transfom : MonoBehaviour, IBossPatternAction
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public int damage = 30;

    [Header("On Arrive - Ground (Optional)")]
    public bool useGround = true;
    public GameObject groundPrefab;
    public float groundLifetime = 10f;

    [Header("Fire Points")]
    [Tooltip("비워두면 이 오브젝트의 직계 자식 Transform들을 자동으로 발사구로 사용합니다.")]
    public List<Transform> firePoints = new List<Transform>();

    [Tooltip("자식 발사구를 자동 수집할지 여부")]
    public bool autoCollectChildren = true;

    public void Execute(BossController owner, BossPattern pattern)
    {
        if (projectilePrefab == null || owner == null || owner.Player == null)
            return;

        // ✅ 발사 순간의 플레이어 위치 고정 (요구사항 그대로)
        Vector2 fixedTarget = owner.Player.position;

        var points = GetFirePoints();
        if (points.Count == 0)
        {
            SpawnOne(transform.position, fixedTarget);
            return;
        }

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null) continue;
            SpawnOne(points[i].position, fixedTarget);
        }
    }

    private List<Transform> GetFirePoints()
    {
        if (!autoCollectChildren)
            return firePoints;

        if (firePoints != null && firePoints.Count > 0)
            return firePoints;

        List<Transform> list = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
            list.Add(transform.GetChild(i));
        return list;
    }

    private void SpawnOne(Vector2 spawnPos, Vector2 fixedTarget)
    {
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ✅ 특정 클래스명 제거 → 인터페이스로 초기화
        var skill = FindInterface<IBossSpawnedSkill>(go);
        if (skill != null)
      {
            GameObject gp = (useGround ? groundPrefab : null);
            float gl = (useGround ? groundLifetime : 0f);

            skill.Init(fixedTarget, damage, projectileSpeed, gp, gl);
        }
        else
        Debug.LogWarning($"[{name}] Spawned projectile '{go.name}' has no IBossSpawnedSkill. Init not called.");    
    }

    // Unity GetComponent<interface>()가 환경에 따라 애매할 수 있어서 안전하게 처리
    private T FindInterface<T>(GameObject go) where T : class
    {
        var monos = go.GetComponents<MonoBehaviour>();
        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] is T t) return t;
        }
        return null;
    }
}

// ✅ 공통 인터페이스 (아무 파일에 두거나, Skill_transfom.cs 하단에 같이 둬도 됨)
public interface IBossSpawnedSkill
{
    void Init(Vector2 fixedTarget, int damage, float speed, GameObject groundPrefab, float groundLifetime);
}
