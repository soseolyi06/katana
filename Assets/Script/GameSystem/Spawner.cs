using UnityEngine; // Unity 엔진 기본 기능(Instantiate, Random, Vector3 등)을 쓰기 위한 네임스페이스

// Spawner 클래스는 "어떤 프리팹을 화면의 특정 위치 범위에서 생성(스폰)하는 역할"을 한다.
// MonoBehaviour를 상속했기 때문에, Unity 씬 안의 GameObject에 붙어서 동작할 수 있다.
public class Spawner : MonoBehaviour
{
    // 인스펙터에서 보기 좋게 구역 제목을 만들어주는 속성(기능에는 영향 없음)
    [Header("Spawn Area")]
    
    // 스폰될 Y 좌표(기본 6.0f)
    // 주석에 적힌 것처럼 화면 상단 근처에서 생성하려는 의도
    public float spawnY = 4.6f;     // 화면 상단 Y
    
    // 스폰될 X 좌표의 최소/최대 범위
    // 랜덤으로 이 범위 안에서 x를 뽑아서 생성 위치를 만든다.
    public float spawnXMin = -2.4f;
    public float spawnXMax = 2.5f;

    // 라운드(웨이브) 관리하는 RoundManager를 저장해두는 변수
    // private 이므로 외부(다른 스크립트)에서 직접 접근 불가
    private RoundManager roundManager;

    // Awake는 Unity 라이프사이클 함수 중 하나
    // 보통 Start보다 더 먼저 호출되며,
    // "이 스크립트가 씬에서 활성화될 때 초기에 준비할 것"을 여기에 넣는다.
    private void Awake()
    {
        // 씬 안에서 RoundManager 타입의 오브젝트를 하나 찾아서 참조를 저장
        // FindFirstObjectByType<T>()는 씬에 존재하는 컴포넌트 중 첫 번째를 가져온다.
        // (RoundManager가 씬에 없으면 null이 들어갈 수 있음)
        roundManager = FindFirstObjectByType<RoundManager>();
    }

    // Spawn 함수: 프리팹(GameObject)을 받아서 1개를 생성하고, 생성된 오브젝트를 반환한다.
    // 반환 타입이 GameObject인 이유: 생성한 오브젝트를 호출한 쪽에서 계속 쓰게 해주기 위해서
    public GameObject Spawn(GameObject prefab)
    {
        // 안전장치: prefab이 null이면 생성할 수 없으니 바로 null 반환
        if (prefab == null) return null;

        // X 좌표를 spawnXMin~spawnXMax 사이에서 랜덤으로 뽑는다.
        float x = Random.Range(spawnXMin, spawnXMax);
        
        // 실제 스폰 위치 벡터를 만든다.
        // (x, spawnY, 0f) -> 2D 게임이라 z는 보통 0으로 두는 경우가 많다.
        Vector3 pos = new Vector3(x, spawnY, 0f);

        // 프리팹을 실제 씬에 생성한다.
        // Instantiate(원본, 위치, 회전)
        // Quaternion.identity는 "회전 없음(기본 회전)"을 의미
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        // ---- 여기부터는 스폰된 몬스터와 RoundManager를 "연결"하는 파트 ----

        // 생성된 obj에 SlimeDeathReporter 컴포넌트가 붙어있는지 찾아본다.
        // (없으면 reporter는 null)
        // var는 컴파일러가 타입을 추론하게 하는 문법인데,
        // 여기서는 SlimeDeathReporter 타입으로 추론된다.
        var reporter = obj.GetComponent<SlimeDeathReporter>();
        
        // reporter가 존재하고(round 보고할 수 있고),
        // roundManager도 존재할 때(씬에 라운드 매니저가 있을 때)만 연결
        if (reporter != null && roundManager != null)
        {
            // reporter에게 "라운드 매니저를 기억해둬"라고 연결해주는 느낌
            // 보통 몬스터가 죽거나 사라질 때 roundManager에게 알려주기 위해 쓰는 구조
            reporter.Bind(roundManager);
        }

        // ---- 여기부터는 라운드 쪽 카운트/상태 갱신 파트 ----

        // 살아있는 몬스터 수 +1 같은 처리를 라운드 매니저에게 알려준다.
        // ?. (null 조건 연산자) :
        // roundManager가 null이 아니면 OnMonsterSpawned()를 호출하고,
        // null이면 아무 것도 하지 않는다(에러 방지)
        roundManager?.OnMonsterSpawned();

        // 생성된 오브젝트를 반환
        return obj;
    }

    // SpawnBurst 함수: 같은 프리팹을 count만큼 반복해서 스폰한다.
    // (예: 한 라운드에 5마리 한꺼번에 생성 같은 용도)
    public void SpawnBurst(GameObject prefab, int count)
    {
        // i가 0부터 count-1까지 반복
        for (int i = 0; i < count; i++)
            // 한 번 반복할 때마다 Spawn(prefab) 호출 -> 몬스터 1개 생성
            Spawn(prefab);
    }
}
