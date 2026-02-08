using UnityEngine;

public class BossSpawnAnchor : MonoBehaviour
{
    [Header("Detect")]
    public string bossTag = "Boss";
    public float checkInterval = 0.2f; // 몇 초마다 탐색할지

    private bool done;
    private float timer;

    private void Update()
    {
        if (done) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;

        timer = 0f;
        TryAttachBoss();
    }

    private void TryAttachBoss()
    {
        var boss = GameObject.FindWithTag(bossTag);
        if (boss == null) return;

        boss.transform.position = transform.position;
        boss.transform.rotation = transform.rotation;

        done = true; // ✅ 딱 한 번만
        enabled = false; // 자기 역할 끝
    }
}
