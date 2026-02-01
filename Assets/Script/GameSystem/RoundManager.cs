using System;
using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    // ✅ 라운드 클리어 이벤트 (1부터 시작하는 라운드 번호로 통일)
    public event Action<int> OnRoundCleared;

    [Header("Round Data List (Order = Round Order)")]
    public RoundData[] rounds;

    [Header("References")]
    public Spawner spawner;

    [Header("Score Settings")]
    public int baseClearScore = 1000;
    public float clearGrowth = 1.10f;

    private int roundIndex = 0;          // 0-based 내부 인덱스
    private float elapsed = 0f;
    private bool roundRunning = false;
    private bool spawningStopped = false;

    private int aliveCount = 0;
    private bool[] eventDone;

    // ✅ 선택지(또는 다음 진행)를 기다리는 상태
    private bool waitingForChoice = false;
    private bool clearedAlready = false;

    public int TotalScore { get; private set; } = 0;

    private void Awake()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<Spawner>();
    }

    private void Start()
    {
        if (rounds == null || rounds.Length == 0)
        {
            Debug.LogError("[RoundManager] rounds is empty. Assign RoundData assets.");
            return;
        }

        StartRound(0);
    }

    private void Update()
    {
        // ✅ 선택지 대기 중이면 라운드 진행 로직을 돌리지 않음
        if (waitingForChoice) return;

        // 라운드 진행 중이 아니면 아무 것도 안 함
        if (!roundRunning) return;

        RoundData current = rounds[roundIndex];

        elapsed += Time.deltaTime;

        // 1) 스폰 스케줄 실행
        if (!spawningStopped)
        {
            for (int i = 0; i < current.schedule.Count; i++)
            {
                if (eventDone[i]) continue;

                SpawnEvent e = current.schedule[i];
                if (elapsed >= e.time)
                {
                    eventDone[i] = true;
                    StartCoroutine(DoSpawnEvent(e));
                }
            }
        }

        // 2) 라운드 시간이 끝나면 스폰 중지
        if (!spawningStopped && elapsed >= current.roundDuration)
        {
            spawningStopped = true;
            Debug.Log($"[Round] Time End. Waiting remaining monsters... alive={aliveCount}");
        }

        // 3) 시간이 끝났고 + 남은 몬스터가 0이면 클리어
        if (spawningStopped && aliveCount <= 0)
        {
            ClearRoundOnce();
        }
    }

    private IEnumerator DoSpawnEvent(SpawnEvent e)
    {
        if (spawner == null || e.prefab == null) yield break;

        if (e.interval <= 0f)
        {
            spawner.SpawnBurst(e.prefab, e.count);
            yield break;
        }

        for (int i = 0; i < e.count; i++)
        {
            spawner.Spawn(e.prefab);
            yield return new WaitForSeconds(e.interval);
        }
    }

    public void StartRound(int index)
    {
        roundIndex = Mathf.Clamp(index, 0, rounds.Length - 1);

        RoundData current = rounds[roundIndex];

        elapsed = 0f;
        roundRunning = true;
        spawningStopped = false;

        waitingForChoice = false;
        clearedAlready = false;

        eventDone = new bool[current.schedule.Count];

        Debug.Log($"[Round] Start Round {GetCurrentRoundNumber()} (duration={current.roundDuration})");
    }

    private void ClearRoundOnce()
    {
        if (clearedAlready) return;
        clearedAlready = true;

        // 라운드 진행 종료
        roundRunning = false;

        // 점수 계산
        float mult = Mathf.Pow(clearGrowth, roundIndex);
        int clearScore = Mathf.RoundToInt(baseClearScore * mult);
        TotalScore += clearScore;
        // 라운드 클리어 로그
        int clearedRoundNumber = GetCurrentRoundNumber();
        Debug.Log($"[Round] Clear Round {clearedRoundNumber}! +{clearScore}  total={TotalScore}");

        // ✅ 여기서 선택지(또는 다음 처리) 대기 상태로 전환
        waitingForChoice = true;

        // ✅ 선택지 매니저에게 "몇 라운드 클리어했는지" 알려줌 (1부터)
        OnRoundCleared?.Invoke(clearedRoundNumber);
    }

    public int GetCurrentRoundNumber()
    {
        // 내부 index(0-based) -> 라운드 번호(1-based)
        return roundIndex + 1;
    }

    // ✅ ChoiceManager가 선택지 적용 후 호출하는 함수
    public void StartNextRoundFromChoice()
    {
        waitingForChoice = false;
        StartNextRound();
    }

    private void StartNextRound()
    {
        int next = roundIndex + 1;
        if (next >= rounds.Length)
        {
            next = 0; // 테스트용 순환. 원하면 게임 종료 처리로 바꿔도 됨.
        }

        StartRound(next);
    }

    // --------------------------
    // Alive Count API
    // --------------------------
    public void OnMonsterSpawned()
    {
        aliveCount++;
    }

    public void OnMonsterDespawned()
    {
        aliveCount--;
        if (aliveCount < 0) aliveCount = 0;
    }
}
