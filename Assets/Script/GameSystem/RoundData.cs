using System;                      // [Serializable] 같은 시스템 관련 기능을 쓰기 위한 네임스페이스
using System.Collections.Generic;  // List<T> 자료구조를 쓰기 위한 네임스페이스
using UnityEngine;                 // Unity 엔진 기본 기능을 쓰기 위한 네임스페이스

// [CreateAssetMenu]
// 이 클래스는 "ScriptableObject"로서
// Unity 에디터에서 우클릭 → Create → Game → Round Data 로
// 새 에셋 파일을 만들 수 있게 해준다.
// fileName은 기본으로 생성될 파일 이름
[CreateAssetMenu(menuName = "Game/Round Data", fileName = "RoundData_01")]
public class RoundData : ScriptableObject
{
    // ===== 라운드 전체 설정 영역 =====
    [Header("Round")]
    
    // 이 라운드가 몇 초 동안 진행되는지
    // 예: 30초 동안 몬스터가 스폰되고, 시간이 끝나면 라운드 종료
    public float roundDuration = 30f;

    // ===== 스폰 스케줄 영역 =====
    [Header("Spawn Timeline")]
    
    // 이 라운드 동안 "언제 / 무엇을 / 몇 마리" 소환할지에 대한 목록
    // SpawnEvent 하나가 = 하나의 스폰 이벤트(예약)
    // new List<SpawnEvent>()를 바로 해둔 이유:
    // 에셋 생성 시 null 에러 없이 바로 사용 가능하게 하려는 목적
    public List<SpawnEvent> schedule = new List<SpawnEvent>();
}

// [Serializable]
// 이 클래스는 다른 클래스(RoundData) 안에서
// 인스펙터에 노출되고, 저장될 수 있도록 만드는 속성
// ScriptableObject는 아니고 "데이터 묶음" 역할
[Serializable]
public class SpawnEvent
{
    // Tooltip:
    // 인스펙터에서 마우스를 올리면 설명이 뜨게 해준다.

    [Tooltip("라운드가 시작된 후 몇 초 뒤에 실행될 이벤트")]
    // 라운드가 시작된 후 몇 초 뒤에 실행될 이벤트인지
    // 예: time = 5 이면 라운드 시작 5초 후에 스폰
    public float time = 0f;

    [Tooltip("소환할 몬스터 프리팹")]
    // 실제로 소환할 몬스터 프리팹
    // 슬라임 G, F, L 같은 프리팹을 여기 넣는다
    public GameObject prefab;

    // [Min(1)]
    // 인스펙터에서 이 값이 최소 1 이상만 입력되도록 제한
    // 0이나 음수로 잘못 넣는 실수를 방지
    [Min(1)]
    // 이 이벤트에서 몇 마리를 소환할지
    public int count = 1;

    [Tooltip("몬스터를 한 번에 다 소환할지, 나눠서 소환할지 결정(0보다 크면: interval 초 간격으로 하나씩 소환)")]
    // 몬스터를 한 번에 다 소환할지, 나눠서 소환할지 결정
    // 0이면: count 만큼 즉시 한 번에 소환
    // 0보다 크면: interval 초 간격으로 하나씩 소환
    public float interval = 0f;
}
