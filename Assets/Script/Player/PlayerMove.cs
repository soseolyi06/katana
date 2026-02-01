using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    // =========================================================
    // ✅ Player는 "최종 스탯을 계산하지 않는다"
    // - 스탯 계산: PlayerStatManager가 담당 (base + bonus = final)
    // - Player는: final 값만 받아서 이동/대시에 "적용"만 한다
    // =========================================================

    [Header("Input")]
    private Vector2 inputDir;                   // 현재 입력 방향(플레이어가 누르는 방향)
    private Vector2 lastMoveDir = Vector2.down; // 마지막 이동 방향(입력 없을 때 대시 방향으로 사용)

    [Header("State")]
    private bool isDashing = false; // 지금 대시 중인지 여부
    private bool canDash = true;    // 대시 쿨타임이 끝나서 대시가 가능한지 여부

    [Header("Cache (Final Stats From Manager)")]
    // ---------------------------------------------------------
    // ✅ "최종 스탯 캐시"
    // - 매니저(PlayerStatManager)가 계산한 final 값을
    //   Player가 매번 직접 접근해도 되지만,
    //   초보자 기준으로 "한 번 받아서 변수에 담아 쓰는" 방식이 직관적이라 캐시를 둠.
    // - RefreshFinalStats()가 이 값을 갱신해준다.
    // ---------------------------------------------------------
    private float moveSpeed;     // 최종 이동속도
    private float dashSpeed;     // 최종 대시속도
    private float dashDuration;  // 최종 대시 유지시간
    private float dashCooldown;  // 최종 대시 쿨타임

    [Header("Attack (Dash Sword)")]
    public SwordHitbox swordHitbox; // Player 자식 Sword의 SwordHitbox 연결

    private Rigidbody2D rb;
    private Animator ani;

    void Awake()
    {
        // ---------------------------------------------------------
        // Rigidbody2D / Animator 컴포넌트를 가져온다
        // ---------------------------------------------------------
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();

        // 탑다운이면 보통 중력 0 (위로 떨어지지 않게)
        rb.gravityScale = 0f;

        // 벽에 부딪혀 회전하는 것 방지(회전 고정)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        // ---------------------------------------------------------
        // ✅ 게임 시작 시점에 한 번, 최종 스탯을 매니저에게서 받아온다
        // ---------------------------------------------------------
        RefreshFinalStats();
    }

    void Update()
    {
        // ---------------------------------------------------------
        // 1) 입력은 Update에서 읽는다 (프레임마다 입력 갱신)
        // ---------------------------------------------------------
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        inputDir = new Vector2(x, y);

        // ---------------------------------------------------------
        // 2) 입력이 있으면 마지막 방향 갱신 + 대각선 속도 보정
        // - inputDir.normalized: 대각선이 더 빨라지는 현상 방지
        // ---------------------------------------------------------
        if (inputDir.sqrMagnitude > 0.001f)
        {
            inputDir = inputDir.normalized;
            lastMoveDir = inputDir;
        }
        else
        {
            inputDir = Vector2.zero;
        }

        // ---------------------------------------------------------
        // 3) (선택) 애니메이션 처리
        // - 지금은 예시로 주석만 남김
        // - ani.SetBool("Up", y > 0f);
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // 4) ✅ 매 프레임 "최종 스탯"을 갱신할지?
        // - 가장 단순한 방식: 매 프레임 받아오기 (초보에게 직관적, 성능도 보통 문제 없음)
        // - 나중에 최적화하고 싶으면: 옵션 선택 직후에만 RefreshFinalStats() 호출하는 방식으로 변경 가능
        // ---------------------------------------------------------
        RefreshFinalStats();

        // ---------------------------------------------------------
        // 5) 대시 입력
        // - 입력 방향이 없으면 lastMoveDir 방향으로 대시한다
        // ---------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector2 dashDir = (inputDir.sqrMagnitude > 0.001f) ? inputDir : lastMoveDir;
            StartCoroutine(Dash(dashDir));
        }
    }

    void FixedUpdate()
    {
        // ---------------------------------------------------------
        // Rigidbody2D 이동은 FixedUpdate에서 처리하는 게 정석
        // ---------------------------------------------------------

        // 대시 중이면 평상시 이동(걷기)은 하지 않는다
        if (isDashing) return;

        // ---------------------------------------------------------
        // ✅ 최종 이동속도(moveSpeed)를 적용해서 이동한다
        // - moveSpeed는 PlayerStatManager가 계산한 finalMoveSpeed를 받아온 값
        // ---------------------------------------------------------
        rb.linearVelocity = inputDir * moveSpeed;
    }

    /// <summary>
    /// ✅ PlayerStatManager가 계산한 "최종(final) 스탯"을 Player가 받아오는 함수
    /// - Player는 계산하지 않고 "받아 적용"만 한다
    /// </summary>
    void RefreshFinalStats()
    {
        // ---------------------------------------------------------
        // 매니저가 아직 없으면(씬에 없거나 생성 순서 문제) 안전하게 빠져나간다
        // ---------------------------------------------------------
        if (PlayerStatManager.I == null) return;

        // ---------------------------------------------------------
        // ✅ final 값을 그대로 가져와서 Player 내부 캐시에 저장
        // ---------------------------------------------------------
        moveSpeed = PlayerStatManager.I.finalMoveSpeed;

        dashSpeed = PlayerStatManager.I.finalDashSpeed;
        dashDuration = PlayerStatManager.I.finalDashDuration;
        dashCooldown = PlayerStatManager.I.finalDashCooldown;
    }

    IEnumerator Dash(Vector2 dir)
    {
        // ---------------------------------------------------------
        // 이미 대시 중이거나 쿨타임이면 종료
        // ---------------------------------------------------------
        if (!canDash || isDashing) yield break;

        canDash = false;
        isDashing = true;

        // ✅ 대시 시작: 검 히트박스 ON
        if (swordHitbox != null) swordHitbox.SetActive(true);
        // ---------------------------------------------------------
        // ✅ 대시 시작: 최종 대시속도(dashSpeed)를 적용
        // - dir.normalized: 혹시 모를 대각선 보정(방향 벡터는 크기 1로)
        // ---------------------------------------------------------
        rb.linearVelocity = dir.normalized * dashSpeed;

        // ---------------------------------------------------------
        // ✅ 대시 유지 시간: 최종 대시 지속시간(dashDuration) 만큼 기다린다
        // ---------------------------------------------------------
        yield return new WaitForSeconds(dashDuration);

        // ---------------------------------------------------------
        // 대시 종료:
        // - 속도를 0으로 만들어 멈춘다
        // - 다음 FixedUpdate에서 inputDir * moveSpeed로 평상시 이동이 다시 적용됨
        // ---------------------------------------------------------
        if (swordHitbox != null) swordHitbox.SetActive(false); // 대시 끝나면 검 히트박스 OFF
        rb.linearVelocity = Vector2.zero;

        isDashing = false;

        // ---------------------------------------------------------
        // ✅ 쿨타임: 최종 쿨타임(dashCooldown) 만큼 기다린 뒤 다시 대시 가능
        // ---------------------------------------------------------
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
