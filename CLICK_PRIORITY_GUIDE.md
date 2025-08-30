# 클릭 우선순위 시스템 개선 가이드

## 문제 상황
- `HiddenObj`와 `ClickEvent` 컴포넌트가 같은 위치에 있을 때 클릭 순서가 꼬이는 문제
- 어떤 오브젝트를 먼저 클릭했는지 판별하기 어려운 상황

## 해결 방안

### 1. **자동 우선순위 시스템 (구현 완료)**

#### HiddenObj 개선사항:
- **우선순위 체크**: `CheckClickPriority()` 메서드로 클릭 우선순위 자동 판별
- **수동 우선순위**: `clickPriority` 필드로 개발자가 직접 우선순위 설정 가능
- **다중 오브젝트 처리**: 여러 HiddenObj가 겹쳐있을 때 자동으로 최우선 오브젝트 선택

#### ClickEvent 개선사항:
- **HiddenObj 양보**: 찾지 않은 HiddenObj가 있으면 ClickEvent는 실행되지 않음
- **자동 감지**: Raycast로 주변 HiddenObj 자동 감지

### 2. **우선순위 결정 규칙**

우선순위는 다음 순서로 결정됩니다:

1. **clickPriority** (높을수록 우선순위)
2. **SortingOrder** (높을수록 우선순위)
3. **Z축 위치** (카메라에 가까울수록 우선순위)

### 3. **사용 방법**

#### 기본 사용법:
```csharp
// HiddenObj에 클릭 우선순위 설정
hiddenObj.clickPriority = 10; // 높은 값일수록 우선순위

// ClickEvent는 자동으로 HiddenObj에게 우선순위를 양보함
// 별도 설정 불필요
```

#### 고급 사용법:
```csharp
// 특정 HiddenObj를 항상 최우선으로 만들기
criticalHiddenObj.clickPriority = 999;

// 디버그 로그 활성화 (HiddenObj.cs 라인 154 주석 해제)
Debug.Log($"Click Priority Check - Current: {gameObject.name}, TopPriority: {topPriorityObject.name}");
```

### 4. **추가 개선 방안**

#### A. Sorting Layer 기반 우선순위
```csharp
// HiddenObj.cs의 GetSortingOrder 메서드에 추가
private int GetSortingOrder(GameObject obj)
{
    var spriteRenderer = obj.GetComponent<SpriteRenderer>();
    if (spriteRenderer != null)
    {
        // Sorting Layer ID도 고려
        return spriteRenderer.sortingOrder + (spriteRenderer.sortingLayerID * 1000);
    }
    return 0;
}
```

#### B. 레이어 기반 우선순위
```csharp
// 특정 레이어에 있는 오브젝트에 우선순위 부여
if (obj.layer == LayerMask.NameToLayer("HiddenObjects"))
{
    priority += 100;
}
```

#### C. 거리 기반 우선순위
```csharp
// 카메라와의 거리를 고려한 우선순위
float distanceToCamera = Vector3.Distance(obj.transform.position, Camera.main.transform.position);
priority += (int)(100 / distanceToCamera); // 가까울수록 우선순위
```

### 5. **디버깅 도구**

#### 클릭 우선순위 시각화:
```csharp
// HiddenObj에 추가할 수 있는 디버그 메서드
[Button("Show Click Priority")]
public void ShowClickPriority()
{
    Debug.Log($"{gameObject.name} - Priority: {clickPriority}, SortingOrder: {GetSortingOrder(gameObject)}, Z: {transform.position.z}");
}
```

#### 겹친 오브젝트 검출:
```csharp
// LevelManager에 추가할 수 있는 디버그 메서드
[Button("Find Overlapping Objects")]
public void FindOverlappingObjects()
{
    var hiddenObjs = FindObjectsOfType<HiddenObj>();
    for (int i = 0; i < hiddenObjs.Length; i++)
    {
        for (int j = i + 1; j < hiddenObjs.Length; j++)
        {
            float distance = Vector2.Distance(hiddenObjs[i].transform.position, hiddenObjs[j].transform.position);
            if (distance < 1f) // 1 유닛 이내에 있으면 겹친 것으로 간주
            {
                Debug.LogWarning($"Overlapping objects: {hiddenObjs[i].name} and {hiddenObjs[j].name}");
            }
        }
    }
}
```

### 6. **성능 최적화**

#### 캐싱 시스템:
```csharp
// HiddenObj에 캐싱 추가
private int cachedSortingOrder = -1;
private bool sortingOrderCached = false;

private int GetSortingOrder(GameObject obj)
{
    if (obj == gameObject && sortingOrderCached)
        return cachedSortingOrder;
        
    // 기존 로직...
    
    if (obj == gameObject)
    {
        cachedSortingOrder = result;
        sortingOrderCached = true;
    }
    
    return result;
}
```

### 7. **테스트 시나리오**

1. **기본 테스트**: HiddenObj와 ClickEvent가 같은 위치에 있을 때 HiddenObj가 우선 클릭되는지 확인
2. **우선순위 테스트**: clickPriority가 다른 여러 HiddenObj 중에서 높은 우선순위가 선택되는지 확인
3. **발견 후 테스트**: HiddenObj를 찾은 후 ClickEvent가 정상 작동하는지 확인
4. **성능 테스트**: 많은 오브젝트가 겹쳐있을 때 성능이 문제없는지 확인

## 결론

이 개선된 시스템으로 다음과 같은 문제들이 해결됩니다:

- ✅ HiddenObj와 ClickEvent의 클릭 순서 꼬임 방지
- ✅ 여러 HiddenObj가 겹쳐있을 때 우선순위 자동 결정
- ✅ 개발자가 수동으로 우선순위 조정 가능
- ✅ 성능 최적화된 Raycast 기반 감지
- ✅ 디버깅 및 테스트 용이성

더 이상 클릭 순서에 대한 고민 없이 게임 개발에 집중할 수 있습니다! 🎮
