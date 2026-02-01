using UnityEngine;

public class SlimeWeapon : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;        // 자식 Empty (FirePoint)
    public GameObject bulletPrefab;    // 총알 프리팹

    [Header("Fire")]
    public float fireInterval = 3.0f;  // 몇 초마다 발사할지

    public float bulletterm = 1.0f;       // 총알 텀

    bool isBursting = false; // 2연사 중인지 여부
    
    public float bulletSpeed = 2f;     // 총알 속도
    public Vector2 fireDirection = Vector2.down; // 발사 방향(기본: 아래)

    private float timer = 0f;
    private SlimeStatManager stat;

    void Awake()
    {
        // 같은 오브젝트(슬라임)에 붙은 스탯 가져오기
        stat = GetComponent<SlimeStatManager>();

        // firePoint를 인스펙터에 안 넣었으면 "FirePoint"라는 이름의 자식을 찾아봄(선택)
        if (firePoint == null)
        {
            Transform t = transform.Find("FirePoint");
            if (t != null) firePoint = t;
        }
    }

    void Start()
    {
        // 무기 보유가 아니면 발사 기능 자체를 꺼버림(실수 방지)
        if (stat != null && stat.hasWeapon == false)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // 필수 참조가 없으면 실행하지 않음
        if (firePoint == null) return;
        if (bulletPrefab == null) return;

                // 2연사 진행 중이면 Update에서 아무것도 하지 않음
        if (isBursting) return;

        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            // 첫 발
            Fire();

            // "지금 2연사 중이다" 표시
            isBursting = true;

            // bulletterm 후 두 번째 발 예약
            Invoke(nameof(FireSecond), bulletterm);

            // 쿨타임 리셋
            timer = 0f;
        }
    }

    void FireSecond()
    {
        // 두 번째 발
        Fire();

        // 2연사 종료 → 다시 쿨타임 돌기 가능
        isBursting = false;
    }

    void Fire()
    {
        // 1) firePoint 위치에 총알 생성
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 2) 총알 스크립트에게 속도/방향 전달
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetUp(fireDirection.normalized, bulletSpeed);
        }
    }
}
