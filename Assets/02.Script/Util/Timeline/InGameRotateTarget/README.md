# InGameRotateTarget 시스템

Unity Timeline을 사용하여 게임 오브젝트를 회전시키는 시스템입니다.

## 주요 기능

### 1. 회전 모드
- **Continuous**: 연속 회전 (지정된 축을 기준으로 계속 회전)
- **CurveBased**: 애니메이션 커브를 사용한 회전
- **StartToEnd**: 시작 각도에서 끝 각도로 부드럽게 회전

### 2. 회전 설정
- **회전 축**: X, Y, Z 축 중 선택 가능
- **회전 속도**: 초당 회전 각도 설정
- **로컬/월드 회전**: 로컬 좌표계 또는 월드 좌표계 선택
- **시작/종료 각도**: 회전 시작/종료 시 각도 설정

## 사용법

### Timeline에서 사용하기

1. **Timeline 생성**
   - Unity에서 Timeline 에셋 생성
   - Timeline 창에서 "Add Track" → "IngameRotateTargetTrack" 추가

2. **클립 생성**
   - 트랙에서 우클릭 → "Add IngameRotateTargetClip"
   - 클립 선택 후 Inspector에서 설정

3. **오브젝트 할당 (중요!)**
   - **직접 할당 (권장)**: 
     - `useExposedReference = false` 체크 해제
     - `rotateTargetDirect` 필드에 회전할 오브젝트를 드래그 앤 드롭
   - **ExposedReference 사용**:
     - `useExposedReference = true` 체크
     - Timeline 에셋 선택
     - Exposed References 섹션에서 '+' 클릭
     - 이름 입력 (예: 'MyRotateTarget')
     - 오브젝트 할당
     - 클립의 `rotateTarget`에서 ExposedReference 선택

4. **설정 옵션**
   ```
   회전 설정:
   - rotateTargetDirect: 직접 할당할 회전 오브젝트 (useExposedReference = false일 때)
   - rotateTarget: ExposedReference용 (useExposedReference = true일 때)
   - useExposedReference: ExposedReference 사용 여부
   - Rotation Axis: 회전 축 (기본값: Vector3.up)
   - Rotation Speed: 회전 속도 (기본값: 90도/초)
   - Use Local Rotation: 로컬 회전 사용 여부
   
   회전 모드:
   - Rotation Mode: 회전 모드 선택
   - Rotation Curve: 커브 기반 회전 시 사용할 애니메이션 커브
   
   시작/종료 설정:
   - Start Rotation: 시작 회전값
   - End Rotation: 종료 회전값
   - Set Start Rotation On Play: 재생 시 시작 회전값 적용
   - Set End Rotation On Complete: 완료 시 종료 회전값 적용
   ```

### 코드에서 사용하기

```csharp
// IngameRotateTargetExample 스크립트 사용
public class MyGameController : MonoBehaviour
{
    public IngameRotateTargetExample rotateExample;
    
    void Start()
    {
        // 회전 시작
        rotateExample.StartRotation();
        
        // 특정 각도로 회전
        rotateExample.RotateToAngle(new Vector3(0, 180, 0), 2f);
    }
}
```

## 파일 구조

```
IngameRotateTarget/
├── IngameRotateTargetBehaviour.cs      # 핵심 회전 로직
├── IngameRotateTargetClip.cs           # Timeline 클립 에셋
├── IngameRotateTargetTrack.cs          # Timeline 트랙
├── IngameRotateTargetMixerBehaviour.cs # 믹서 동작
├── IngameRotateTargetExample.cs        # 사용 예제
└── README.md                           # 이 파일
```

## 예제 시나리오

### 1. 바퀴 회전
- **모드**: Continuous
- **축**: Vector3.forward (Z축)
- **속도**: 360 (1초에 한 바퀴)

### 2. 문 열기/닫기
- **모드**: StartToEnd
- **시작 각도**: (0, 0, 0)
- **종료 각도**: (0, 90, 0)
- **커브**: EaseInOut

### 3. 팬 블레이드 회전
- **모드**: Continuous
- **축**: Vector3.up (Y축)
- **속도**: 720 (2초에 한 바퀴)

## 문제 해결

### 오브젝트 할당이 안 되는 경우
1. **콘솔 확인**: "rotateTarget이 할당되지 않았습니다!" 경고 메시지 확인
2. **ExposedReference 사용**: Timeline 에셋의 Exposed References에서 오브젝트를 등록하고 사용
4. **디버그 로그 확인**: Console 창에서 할당 상태 확인

### 일반적인 문제들
- **Timeline이 재생되지 않음**: PlayableDirector의 Play On Awake 체크 또는 코드에서 `director.Play()` 호출
- **회전이 너무 빠름/느림**: Rotation Speed 값 조정
- **잘못된 축으로 회전**: Rotation Axis 값 확인 (X축: Vector3.right, Y축: Vector3.up, Z축: Vector3.forward)

## 주의사항

1. **성능**: 많은 오브젝트를 동시에 회전시킬 때는 성능을 고려하세요.
2. **충돌**: 물리 기반 오브젝트의 경우 회전 시 충돌 처리를 확인하세요.
3. **부모-자식 관계**: 로컬 회전과 월드 회전의 차이를 이해하고 사용하세요.
4. **오브젝트 할당**: ExposedReference를 사용하여 안전하게 오브젝트를 할당하세요.

## 확장 가능성

- **이징 함수**: 다양한 이징 함수 추가
- **회전 경로**: 복잡한 회전 경로 지원
- **물리 연동**: 물리 기반 회전 지원
- **UI 연동**: UI 요소 회전 지원 