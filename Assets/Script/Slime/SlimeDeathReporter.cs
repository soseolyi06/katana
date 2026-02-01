using UnityEngine; // MonoBehaviour, GameObject, Destroy 등 Unity 기본 기능 사용

// SlimeDeathReporter 클래스
// 역할: 이 오브젝트(슬라임)가 "사라질 때" RoundManager에게 알려주는 보고 담당
// 직접 라운드를 제어하지 않고, "보고만" 한다.
public class SlimeDeathReporter : MonoBehaviour
{
    // 이 슬라임이 속한 라운드를 관리하는 RoundManager 참조
    // private이므로 외부에서 직접 접근 불가
    private RoundManager roundManager;

    // 이미 보고했는지 여부를 저장하는 플래그
    // 중복 보고를 막기 위한 안전장치
    private bool reported = false;

    // Bind 함수
    // Spawner 쪽에서 이 슬라임을 생성한 직후 호출해서
    // "너는 이 RoundManager에게 보고해라" 라고 연결해주는 역할
    public void Bind(RoundManager manager)
    {
        // 전달받은 RoundManager를 저장
        roundManager = manager;
    }

    // ReportDespawn 함수
    // 이 슬라임이 사라졌다는 사실을 라운드 매니저에게 보고하는 함수
    public void ReportDespawn()
    {
        // 이미 한 번 보고했다면
        // 더 이상 아무 것도 하지 않고 바로 종료
        if (reported) return;

        // 이제부터는 "이미 보고했다" 상태로 변경
        reported = true;

        // roundManager가 null이 아닐 때만
        // OnMonsterDespawned()를 호출
        // ?. 는 null 체크 연산자
        roundManager?.OnMonsterDespawned();
    }

    // OnDestroy
    // 이 MonoBehaviour가 붙은 GameObject가 파괴될 때
    // Unity가 자동으로 호출해주는 생명주기 함수
    private void OnDestroy()
    {
        // Destroy(gameObject)가 호출되면
        // 무조건 이 함수가 실행된다.

        // 슬라임이 어떤 이유로든 사라질 때
        // (플레이어에게 죽거나, 화면 밖으로 나가거나, 강제 삭제되거나)
        // 라운드 매니저에게 "1회만" 보고하도록 한다.
        ReportDespawn();
    }
}
