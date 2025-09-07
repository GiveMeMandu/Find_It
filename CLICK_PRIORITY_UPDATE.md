# 클릭 우선순위 시스템 업데이트

## 문제상황
- StretchVFX가 붙은 두 개의 다른 객체가 콜라이더가 겹치는 부분을 터치하면 둘 다 반응하는 문제
- 사용자는 하이어라키상 순서대로 더 위에 있는 객체만 처리되기를 원함

## 해결방법
LeanClickEvent.cs 파일에 `CheckHierarchyPriority()` 메서드를 추가하여 겹치는 객체들 중 하이어라키 순서상 가장 위에 있는 객체만 클릭을 허용하도록 수정

### 주요 변경사항

#### 1. CheckHierarchyPriority 메서드 추가
- 터치 지점에서 겹치는 모든 LeanClickEvent를 가진 객체들을 찾음
- 하이어라키 순서(sibling index)로 정렬
- 가장 위에 있는 객체만 클릭을 허용

#### 2. 모든 핸들러에 우선순위 검사 적용
- `HandleFingerDown()`: 마우스 다운 이벤트
- `HandleFingerUp()`: 마우스 업 이벤트  
- `HandleFingerTap()`: 클릭 이벤트

#### 3. 우선순위 로직
```csharp
// 같은 부모를 가진 경우 sibling index로 비교
if (a.transform.parent == b.transform.parent)
{
    return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
}

// 다른 부모를 가진 경우, 루트까지 올라가서 비교
Transform aRoot = a.transform;
Transform bRoot = b.transform;

while (aRoot.parent != null) aRoot = aRoot.parent;
while (bRoot.parent != null) bRoot = bRoot.parent;

return aRoot.GetSiblingIndex().CompareTo(bRoot.GetSiblingIndex());
```

## 사용방법
1. StretchVFXGroupSetting에서 컴포넌트를 설정하면 자동으로 적용됩니다
2. 겹치는 객체가 있는 경우, 하이어라키에서 위에 있는 객체만 클릭 반응합니다
3. 디버그 로그를 통해 어떤 객체가 우선순위를 가졌는지 확인할 수 있습니다

## 주의사항
- 이 시스템은 LeanClickEvent를 사용하는 객체들에만 적용됩니다
- 기존의 HiddenObj 우선순위 시스템과 함께 작동합니다
- 하이어라키 순서를 변경하면 클릭 우선순위도 함께 변경됩니다

## 테스트 방법
1. 두 개의 객체에 StretchVFXGroupSetting을 적용
2. 콜라이더가 겹치도록 배치
3. 겹치는 부분을 클릭했을 때 하이어라키상 위에 있는 객체만 반응하는지 확인
