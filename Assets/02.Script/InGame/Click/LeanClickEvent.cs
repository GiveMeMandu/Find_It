using Lean.Common;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;

public class LeanClickEvent : LeanSelectableByFinger
{
	public bool Enable = true;

	public UnityEvent OnMouseDownEvent;
	public UnityEvent OnMouseUpEvent;
	public UnityEvent OnClickEvent;

	private int _clickCount = 0;
	public int _maxClickCount = -1;

	private void Awake()
	{
		// UnityEvent 초기화 (런타임에 AddComponent로 추가된 경우 대비)
		if (OnMouseDownEvent == null)
		{
			OnMouseDownEvent = new UnityEvent();
		}
		if (OnMouseUpEvent == null)
		{
			OnMouseUpEvent = new UnityEvent();
		}
		if (OnClickEvent == null)
		{
			OnClickEvent = new UnityEvent();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		LeanTouch.OnFingerDown += HandleFingerDown;
		LeanTouch.OnFingerUp += HandleFingerUp;
		LeanTouch.OnFingerTap += HandleFingerTap;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		LeanTouch.OnFingerDown -= HandleFingerDown;
		LeanTouch.OnFingerUp -= HandleFingerUp;
		LeanTouch.OnFingerTap -= HandleFingerTap;
	}

	protected override void OnDestroy()
	{
		// Ensure static events are unsubscribed if object destroyed
		LeanTouch.OnFingerDown -= HandleFingerDown;
		LeanTouch.OnFingerUp -= HandleFingerUp;
		LeanTouch.OnFingerTap -= HandleFingerTap;

		base.OnDestroy();
	}

	private void HandleFingerDown(LeanFinger finger)
	{
		if (!Enable) return;

		// InputManager의 isEnabled 상태 확인
		if (Global.InputManager != null && !IsInputEnabled()) return;

		// UI가 위에 있으면 클릭 차단
		if (IsUIBlockingClick(finger.ScreenPosition)) return;

		if (IsFingerOverThis(finger) == false) return;

		// Hidden object priority check
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		// Fire down event
		OnMouseDownEvent?.Invoke();

		// Mark this selectable as selected by this finger
		SelectSelf(finger);
	}

	private void HandleFingerUp(LeanFinger finger)
	{
		if (!Enable) return;

		// InputManager의 isEnabled 상태 확인
		if (Global.InputManager != null && !IsInputEnabled()) return;

		// UI가 위에 있으면 클릭 차단
		if (IsUIBlockingClick(finger.ScreenPosition)) return;

		if (IsFingerOverThis(finger) == false) return;

		// Hidden object priority check
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		OnMouseUpEvent?.Invoke();
		// selection removal is handled by LeanSelectableByFinger.HandleFingerUp
	}

	private void HandleFingerTap(LeanFinger finger)
	{
		if (!Enable) return;

		// InputManager의 isEnabled 상태 확인
		if (Global.InputManager != null && !IsInputEnabled()) return;

		// UI가 위에 있으면 클릭 차단
		if (IsUIBlockingClick(finger.ScreenPosition)) return;

		if (IsFingerOverThis(finger) == false) return;

		// Prevent tap if max reached
		if (_maxClickCount != -1 && _clickCount >= _maxClickCount) return;

		// Hidden object priority check using EventSystem raycast
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		// Fire click event
		Debug.Log($"[LeanClickEvent] {gameObject.name}: About to invoke OnClickEvent. OnClickEvent is null: {OnClickEvent == null}, Listener count: {(OnClickEvent != null ? OnClickEvent.GetPersistentEventCount() : 0)}");
		OnClickEvent?.Invoke();
		_clickCount++;

		Debug.Log("LeanClickEvent OnFingerTap " + gameObject.name + " _clickCount: " + _clickCount);
	}

	public void IsEnable(bool enable)
	{
		Enable = enable;
	}

	public void SetMaxClickCount(int maxCount)
	{
		_maxClickCount = maxCount;
	}

	public void ResetClickCount()
	{
		_clickCount = 0;
	}

	public int GetClickCount()
	{
		return _clickCount;
	}

	/// <summary>
	/// InputManager의 isEnabled 상태를 확인하는 헬퍼 메서드
	/// </summary>
	private bool IsInputEnabled()
	{
		return Global.InputManager != null && Global.InputManager.isEnabled;
	}

	/// <summary>
	/// UI가 현재 위치를 차단하고 있는지 확인 (현재 오브젝트가 아닌 다른 UI)
	/// </summary>
	private bool IsUIBlockingClick(Vector2 screenPosition)
	{
		if (EventSystem.current != null)
		{
			var ped = new PointerEventData(EventSystem.current)
			{
				position = screenPosition
			};

			var raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(ped, raycastResults);

			// UI 결과가 있고, 첫 번째 히트가 현재 오브젝트가 아니면 UI가 차단하고 있음
			if (raycastResults.Count > 0)
			{
				// 첫 번째 UI 요소가 현재 오브젝트 또는 자식이 아니면 차단
				var firstHit = raycastResults[0];
				if (firstHit.gameObject != gameObject && !firstHit.gameObject.transform.IsChildOf(transform))
				{
					Debug.Log($"[LeanClickEvent] {gameObject.name}: Blocked by UI {firstHit.gameObject.name}");
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Checks whether the finger screen position overlaps this GameObject (UI/3D/2D).
	/// </summary>
	private bool IsFingerOverThis(LeanFinger finger)
	{
		// UI check
		if (EventSystem.current != null)
		{
			var ped = new PointerEventData(EventSystem.current)
			{
				position = finger.ScreenPosition
			};

			var raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(ped, raycastResults);

			foreach (var result in raycastResults)
			{
				if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
				{
					return true;
				}
			}
		}

		// 3D physics check
		var cam = Camera.main ?? Camera.current;
		if (cam != null)
		{
			var ray = cam.ScreenPointToRay(finger.ScreenPosition);

			if (Physics.Raycast(ray, out var hit))
			{
				if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
				{
					return true;
				}
			}

			// 2D check - 모든 겹치는 콜라이더를 확인
			var wp = cam.ScreenToWorldPoint(new Vector3(finger.ScreenPosition.x, finger.ScreenPosition.y, cam.nearClipPlane));
			Collider2D[] allHits = Physics2D.OverlapPointAll(wp);
			
			foreach (var hit2d in allHits)
			{
				if (hit2d != null && (hit2d.gameObject == gameObject || hit2d.transform.IsChildOf(transform)))
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// HiddenObj 우선순위 검사: 
	/// 1. OverHiddenObjectLayer 객체는 HiddenObj보다 우선순위가 높음
	/// 2. HiddenObj가 있고 찾아지지 않은 상태면 일반 객체의 클릭을 막음
	/// </summary>
	private bool CheckHiddenObjectPriority(Vector2 screenPosition)
	{
		Camera cam = Camera.main;
		if (cam == null) return true;

		Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
		
		// 현재 오브젝트가 HiddenObj 컴포넌트를 가지고 있는지 확인
		bool isCurrentHiddenObj = GetComponent<HiddenObj>() != null;
		
		// OverHiddenObjectLayer 체크 - 이 레이어는 HiddenObj보다 우선순위가 높음
		int overHiddenObjectLayerMask = 1 << Helper.LayerManager.OverHiddenObjectLayer;
		Collider2D overHiddenObjHit = Physics2D.OverlapPoint(worldPoint, overHiddenObjectLayerMask);
		
		if (overHiddenObjHit != null)
		{
			// 현재 오브젝트가 OverHiddenObjectLayer면 통과
			if (gameObject.layer == Helper.LayerManager.OverHiddenObjectLayer)
			{
				return true;
			}
			// 현재 오브젝트가 HiddenObj면 OverHiddenObjectLayer에 의해 차단됨
			else if (isCurrentHiddenObj)
			{
				Debug.Log($"OverHiddenObject {overHiddenObjHit.gameObject.name} has priority, blocking HiddenObj {gameObject.name}");
				return false;
			}
		}

		// 현재 오브젝트가 HiddenObj라면 일반 객체보다 우선순위가 높음
		if (isCurrentHiddenObj)
		{
			return true;
		}

		// HiddenObject 레이어 체크
		int hiddenObjectLayerMask = 1 << Helper.LayerManager.HiddenObjectLayer;
		Collider2D hiddenObjHit = Physics2D.OverlapPoint(worldPoint, hiddenObjectLayerMask);

		if (hiddenObjHit != null)
		{
			HiddenObj hiddenObj = hiddenObjHit.GetComponent<HiddenObj>();
			if (hiddenObj != null && !hiddenObj.IsFound)
			{
				Debug.Log($"HiddenObj {hiddenObj.gameObject.name} has priority, blocking other clicks");
				return false; // HiddenObj가 일반 객체보다 우선순위를 가짐
			}
		}

		return true; // 다른 오브젝트 클릭 허용
	}

	/// <summary>
	/// 하이어라키 우선순위 검사: 겹치는 객체들 중에서 하이어라키상 가장 위에 있는 객체만 클릭을 허용
	/// </summary>
	private bool CheckHierarchyPriority(Vector2 screenPosition)
	{
		Camera cam = Camera.main;
		if (cam == null) return true;

		Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
		
		// 모든 2D 콜라이더를 검사하여 겹치는 LeanClickEvent를 가진 객체들을 찾음
		Collider2D[] overlappingColliders = Physics2D.OverlapPointAll(worldPoint);
		
		List<LeanClickEvent> overlappingClickEvents = new List<LeanClickEvent>();
		
		foreach (var collider in overlappingColliders)
		{
			LeanClickEvent clickEvent = collider.GetComponent<LeanClickEvent>();
			if (clickEvent != null && clickEvent.Enable)
			{
				overlappingClickEvents.Add(clickEvent);
			}
		}
		
		// 겹치는 객체가 1개 이하면 그냥 허용
		if (overlappingClickEvents.Count <= 1)
		{
			return true;
		}
		
		// 하이어라키 순서로 정렬 (더 깊은 계층이 우선, 같은 계층에서는 부모의 sibling index가 클수록 우선)
		overlappingClickEvents.Sort((a, b) => 
		{
			// 먼저 계층 깊이를 비교 (더 깊은 계층이 우선)
			int depthA = GetHierarchyDepth(a.transform);
			int depthB = GetHierarchyDepth(b.transform);
			
			if (depthA != depthB)
			{
				return depthB.CompareTo(depthA); // 더 깊은 계층이 우선 (내림차순)
			}
			
			// 같은 깊이라면 부모 계층의 sibling index로 비교
			// 같은 부모를 가진 경우
			if (a.transform.parent == b.transform.parent)
			{
				return b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex());
			}
			
			// 다른 부모를 가진 경우, 각각의 루트 부모의 sibling index 비교
			Transform aParent = a.transform.parent;
			Transform bParent = b.transform.parent;
			
			// 같은 조부모를 찾을 때까지 올라가기
			while (aParent != null && bParent != null && aParent.parent != bParent.parent)
			{
				if (aParent.parent == null || bParent.parent == null) break;
				aParent = aParent.parent;
				bParent = bParent.parent;
			}
			
			// 같은 조부모를 가진 부모들의 sibling index 비교
			if (aParent != null && bParent != null && aParent.parent == bParent.parent)
			{
				return bParent.GetSiblingIndex().CompareTo(aParent.GetSiblingIndex());
			}
			
			// 최종적으로 루트까지 올라가서 비교
			Transform aRoot = a.transform;
			Transform bRoot = b.transform;
			
			while (aRoot.parent != null) aRoot = aRoot.parent;
			while (bRoot.parent != null) bRoot = bRoot.parent;
			
			return bRoot.GetSiblingIndex().CompareTo(aRoot.GetSiblingIndex());
		});
		
		// 디버그 로그: 겹치는 모든 객체와 우선순위 표시
		// Debug.Log($"[{gameObject.name}] CheckHierarchyPriority - Overlapping objects at touch position:");
		for (int i = 0; i < overlappingClickEvents.Count; i++)
		{
			var obj = overlappingClickEvents[i];
			string hierarchy = GetHierarchyPath(obj.transform);
			int depth = GetHierarchyDepth(obj.transform);
			string marker = (obj == this) ? " <- THIS OBJECT" : "";
			// Debug.Log($"  [{i}] {obj.gameObject.name} (Sibling: {obj.transform.GetSiblingIndex()}, Depth: {depth}) - {hierarchy}{marker}");
		}
		
		// 가장 위에 있는(첫 번째) 객체만 클릭을 허용
		bool isTopMost = overlappingClickEvents[0] == this;
		
		if (!isTopMost)
		{
			Debug.Log($"[{gameObject.name}] BLOCKED: {overlappingClickEvents[0].gameObject.name} has priority, blocking {gameObject.name}");
		}
		else
		{
			Debug.Log($"[{gameObject.name}] ALLOWED: Top priority object clicked: {gameObject.name}");
		}
		
		return isTopMost;
	}
	
	/// <summary>
	/// 오브젝트의 전체 하이어라키 경로를 반환
	/// </summary>
	private string GetHierarchyPath(Transform transform)
	{
		string path = transform.name;
		Transform current = transform.parent;
		
		while (current != null)
		{
			path = current.name + "/" + path;
			current = current.parent;
		}
		
		return path;
	}
	
	/// <summary>
	/// 오브젝트의 하이어라키 깊이를 반환 (루트부터 몇 단계인지)
	/// </summary>
	private int GetHierarchyDepth(Transform transform)
	{
		int depth = 0;
		Transform current = transform;
		
		while (current.parent != null)
		{
			depth++;
			current = current.parent;
		}
		
		return depth;
	}
}
